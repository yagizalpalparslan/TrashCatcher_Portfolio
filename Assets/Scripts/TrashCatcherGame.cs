using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TrashCatcher
{
    public class TrashCatcherGame : MonoBehaviour
    {
        private const int StartingHealth = 3;
        private const int LevelTwoStartingHealth = 2;
        private const int LevelTwoScore = 50;
        private const int WinScore = 120;
        private const float FeedbackDuration = 1.15f;
        private const float LevelCompleteMessageDuration = 1.8f;
        private const float FlashDuration = 0.2f;
        private const float ScorePopupDuration = 0.6f;
        private const string LevelOneSceneName = "TrashCatcherLevel1";
        private const string LevelTwoSceneName = "TrashCatcherLevel2";
        private const string LevelTwoScenePath = "Assets/Scenes/TrashCatcherLevel2.unity";

        private static int carriedScore = -1;

        private readonly List<FallingTrash> activeTrash = new List<FallingTrash>();

        private Camera mainCamera;
        private SpriteRenderer backgroundRenderer;
        private BinController bin;
        private Text hudText;
        private Text messageText;
        private Text feedbackText;
        private Text scorePopupText;
        private Button transitionPlayButton;
        private Image flashOverlay;
        private float spawnTimer;
        private float feedbackTimer;
        private float levelTransitionTimer;
        private float flashTimer;
        private float scorePopupTimer;
        private Color flashColor;
        private Vector2 scorePopupStartPosition;
        private int score;
        private int health;
        private bool isGameOver;
        private bool hasWon;
        private bool isTransitioning;
        private LevelSettings levelSettings;

        private struct LevelSettings
        {
            public int LevelNumber;
            public bool IncludesHazardous;
            public float SpawnInterval;
            public float FallSpeed;
            public float BinWidth;
            public float BinHeight;
            public float BinSpeed;
            public float TrashSize;
            public string SceneLabel;
        }

        public float MissY { get; private set; }
        public bool IsFinished
        {
            get { return isGameOver || hasWon; }
        }

        public static TrashCatcherGame CreateGame()
        {
            GameObject root = new GameObject("Trash Catcher Game");
            return root.AddComponent<TrashCatcherGame>();
        }

        private void Awake()
        {
            levelSettings = GetSettingsForCurrentScene();
            SetupCamera();
            SetupBackground();
            SetupBin();
            SetupUi();
            ResetGame();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetGame();
                return;
            }

            UpdateFeedbackMessage();
            UpdateLevelTransition();
            UpdateVisualEffects();

            if (IsFinished || isTransitioning)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnTrash();
                spawnTimer = levelSettings.SpawnInterval;
            }

            CheckCatches();
            UpdateHud();
        }

        public void HandleTrashMissed(FallingTrash trash)
        {
            if (IsFinished)
            {
                activeTrash.Remove(trash);
                Destroy(trash.gameObject);
                return;
            }

            if (trash.Type != TrashType.Hazardous)
            {
                LoseHealth();
            }

            activeTrash.Remove(trash);
            Destroy(trash.gameObject);
            UpdateHud();
        }

        private void ResetGame()
        {
            for (int i = activeTrash.Count - 1; i >= 0; i--)
            {
                if (activeTrash[i] != null)
                {
                    Destroy(activeTrash[i].gameObject);
                }
            }

            activeTrash.Clear();
            score = 0;
            health = StartingHealth;

            if (levelSettings.LevelNumber == 2 && carriedScore >= LevelTwoScore)
            {
                score = carriedScore;
                health = LevelTwoStartingHealth;
            }
            else
            {
                carriedScore = -1;

                if (levelSettings.LevelNumber == 2)
                {
                    health = LevelTwoStartingHealth;
                }
            }

            isGameOver = false;
            hasWon = false;
            isTransitioning = false;
            spawnTimer = 0.5f;
            feedbackTimer = 0f;
            levelTransitionTimer = 0f;
            flashTimer = 0f;
            scorePopupTimer = 0f;

            if (bin != null)
            {
                bin.Configure(levelSettings.BinWidth, levelSettings.BinHeight, levelSettings.BinSpeed);
                bin.ResetBin();
            }

            if (messageText != null)
            {
                messageText.text = string.Empty;
            }

            ClearFeedback();

            ShowTransitionButton(false);
            HideVisualEffects();
            UpdateHud();
        }

        private void SetupCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            MissY = -mainCamera.orthographicSize - 0.8f;
        }

        private void SetupBin()
        {
            GameObject binObject = new GameObject("Recycling Bin");
            binObject.transform.SetParent(transform);
            binObject.transform.position = new Vector3(0f, -4.1f, 0f);
            bin = binObject.AddComponent<BinController>();
            bin.Configure(levelSettings.BinWidth, levelSettings.BinHeight, levelSettings.BinSpeed);
        }

        private void SetupBackground()
        {
            Sprite background = PrototypeAssets.GetBackgroundSprite(levelSettings.LevelNumber);
            if (background == null)
            {
                return;
            }

            GameObject backgroundObject = new GameObject("Level Background");
            backgroundObject.transform.SetParent(transform);
            backgroundObject.transform.position = new Vector3(0f, 0f, 1f);

            backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            backgroundRenderer.sortingOrder = -100;

            float height = mainCamera.orthographicSize * 2f;
            float width = height * mainCamera.aspect;
            PrototypeAssets.AssignSpriteToCover(backgroundRenderer, background, width, height);
        }

        private void SetupUi()
        {
            EnsureEventSystem();

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Trash Catcher UI");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            hudText = CreateText(canvas.transform, "HUD", 22, TextAnchor.UpperLeft);
            RectTransform hudRect = hudText.rectTransform;
            hudRect.anchorMin = new Vector2(0f, 1f);
            hudRect.anchorMax = new Vector2(1f, 1f);
            hudRect.pivot = new Vector2(0f, 1f);
            hudRect.anchoredPosition = new Vector2(16f, -12f);
            hudRect.sizeDelta = new Vector2(-32f, 120f);

            messageText = CreateText(canvas.transform, "Message", 42, TextAnchor.MiddleCenter);
            RectTransform messageRect = messageText.rectTransform;
            messageRect.anchorMin = new Vector2(0f, 0f);
            messageRect.anchorMax = new Vector2(1f, 1f);
            messageRect.pivot = new Vector2(0.5f, 0.5f);
            messageRect.anchoredPosition = Vector2.zero;
            messageRect.sizeDelta = Vector2.zero;

            feedbackText = CreateText(canvas.transform, "Feedback", 32, TextAnchor.MiddleCenter);
            feedbackText.color = Color.black;
            Outline feedbackOutline = feedbackText.gameObject.AddComponent<Outline>();
            feedbackOutline.effectColor = new Color(1f, 0.78f, 0.15f, 0.9f);
            feedbackOutline.effectDistance = new Vector2(1.5f, -1.5f);
            RectTransform feedbackRect = feedbackText.rectTransform;
            feedbackRect.anchorMin = new Vector2(0.5f, 1f);
            feedbackRect.anchorMax = new Vector2(0.5f, 1f);
            feedbackRect.pivot = new Vector2(0.5f, 1f);
            feedbackRect.anchoredPosition = new Vector2(0f, -10f);
            feedbackRect.sizeDelta = new Vector2(680f, 42f);

            transitionPlayButton = CreateButton(canvas.transform, "Level 2 Play Button", "PLAY");
            RectTransform buttonRect = transitionPlayButton.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -90f);
            buttonRect.sizeDelta = new Vector2(170f, 58f);
            transitionPlayButton.onClick.AddListener(StartLevelTwo);
            ShowTransitionButton(false);

            flashOverlay = CreateFlashOverlay(canvas.transform);

            scorePopupText = CreateText(canvas.transform, "Score Popup", 32, TextAnchor.MiddleCenter);
            scorePopupText.color = new Color(0.45f, 1f, 0.55f);
            RectTransform popupRect = scorePopupText.rectTransform;
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            scorePopupStartPosition = new Vector2(0f, -150f);
            popupRect.anchoredPosition = scorePopupStartPosition;
            popupRect.sizeDelta = new Vector2(160f, 48f);
            scorePopupText.gameObject.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            Text text = textObject.AddComponent<Text>();
            text.font = GetDefaultFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.fontStyle = FontStyle.BoldAndItalic;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(Transform parent, string name, string label)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);

            Button button = buttonObject.AddComponent<Button>();

            Text buttonText = CreateText(buttonObject.transform, "Text", 26, TextAnchor.MiddleCenter);
            buttonText.text = label;
            buttonText.color = Color.black;
            RectTransform textRect = buttonText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private Image CreateFlashOverlay(Transform parent)
        {
            GameObject overlayObject = new GameObject("Feedback Flash");
            overlayObject.transform.SetParent(parent, false);

            Image overlay = overlayObject.AddComponent<Image>();
            overlay.raycastTarget = false;
            overlay.color = Color.clear;

            RectTransform overlayRect = overlay.rectTransform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            return overlay;
        }

        private void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private void SpawnTrash()
        {
            TrashType type = PickTrashType();
            GameObject trashObject = new GameObject(TrashCatcherTypes.GetDisplayName(type) + " Trash");
            trashObject.transform.SetParent(transform);

            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;
            float x = Random.Range(-halfWidth + 0.4f, halfWidth - 0.4f);
            trashObject.transform.position = new Vector3(x, halfHeight + 0.6f, 0f);

            FallingTrash trash = trashObject.AddComponent<FallingTrash>();
            trash.Initialize(this, type, levelSettings.FallSpeed, levelSettings.TrashSize);
            activeTrash.Add(trash);
        }

        private TrashType PickTrashType()
        {
            if (!levelSettings.IncludesHazardous)
            {
                return (TrashType)Random.Range(0, 3);
            }

            return (TrashType)Random.Range(0, 4);
        }

        private void CheckCatches()
        {
            for (int i = activeTrash.Count - 1; i >= 0; i--)
            {
                FallingTrash trash = activeTrash[i];
                if (trash == null)
                {
                    activeTrash.RemoveAt(i);
                    continue;
                }

                if (bin.ColliderBounds.Intersects(trash.ColliderBounds))
                {
                    ResolveCatch(trash);

                    activeTrash.Remove(trash);
                    Destroy(trash.gameObject);

                    if (IsFinished || isTransitioning)
                    {
                        return;
                    }
                }
            }
        }

        private void ResolveCatch(FallingTrash trash)
        {
            trash.Resolve();

            if (trash.Type == TrashType.Hazardous)
            {
                ShowFeedback("Hazardous waste should be avoided.");
                PlayFlash(new Color(1f, 0.05f, 0.05f), 0.4f);
                LoseHealth();
                return;
            }

            if (trash.Type == bin.CurrentCategory)
            {
                score += 10;
                PlayFlash(new Color(0.1f, 1f, 0.2f), 0.25f);
                ShowScorePopup();
                if (score >= WinScore)
                {
                    WinGame();
                }
                else if (levelSettings.LevelNumber == 1 && score >= LevelTwoScore)
                {
                    GoToLevelTwo();
                }
                else
                {
                    ShowFeedback(TrashCatcherTypes.GetDisplayName(trash.Type) + " recycled correctly!");
                }
            }
            else
            {
                ShowFeedback(GetWrongBinMessage(trash.Type));
                PlayFlash(new Color(1f, 0.1f, 0.1f), 0.25f);
                LoseHealth();
            }
        }

        private void LoseHealth()
        {
            health -= 1;
            if (health <= 0)
            {
                health = 0;
                EndGame();
            }
        }

        private void EndGame()
        {
            isGameOver = true;
            ClearFeedback();
            messageText.text = "Game Over\nPress R to Restart";
        }

        private void WinGame()
        {
            hasWon = true;
            ClearFeedback();
            messageText.text = "You Win!\nPress R to Restart";
        }

        private void GoToLevelTwo()
        {
            carriedScore = score;
            isTransitioning = true;
            feedbackTimer = 0f;
            levelTransitionTimer = LevelCompleteMessageDuration;

            if (messageText != null)
            {
                messageText.text = "Correct sorting helps reduce landfill waste.";
            }

            ClearActiveTrash();
            ShowTransitionButton(false);
            UpdateHud();
        }

        private void UpdateLevelTransition()
        {
            if (!isTransitioning || levelTransitionTimer <= 0f)
            {
                return;
            }

            levelTransitionTimer -= Time.deltaTime;
            if (levelTransitionTimer > 0f)
            {
                return;
            }

            if (messageText != null)
            {
                messageText.text = "Level 2: Night Storm Begins!";
            }

            ShowTransitionButton(true);
        }

        private void StartLevelTwo()
        {
            ShowTransitionButton(false);

            if (Application.CanStreamedLevelBeLoaded(LevelTwoScenePath))
            {
                SceneManager.LoadScene(LevelTwoScenePath);
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(LevelTwoSceneName))
            {
                SceneManager.LoadScene(LevelTwoSceneName);
                return;
            }

            levelSettings = GetLevelTwoSettings();
            ApplyLevelTwoInCurrentScene();
        }

        private void ApplyLevelTwoInCurrentScene()
        {
            isTransitioning = false;
            ClearActiveTrash();
            score = carriedScore;
            health = LevelTwoStartingHealth;
            spawnTimer = 0.45f;

            if (bin != null)
            {
                bin.Configure(levelSettings.BinWidth, levelSettings.BinHeight, levelSettings.BinSpeed);
                bin.ResetBin();
            }

            if (messageText != null)
            {
                messageText.text = string.Empty;
            }

            UpdateHud();
        }

        private void ClearActiveTrash()
        {
            for (int i = activeTrash.Count - 1; i >= 0; i--)
            {
                if (activeTrash[i] != null)
                {
                    Destroy(activeTrash[i].gameObject);
                }
            }

            activeTrash.Clear();
        }

        private void ShowFeedback(string text)
        {
            if (feedbackText == null || IsFinished || isTransitioning)
            {
                return;
            }

            feedbackText.text = text;
            feedbackTimer = FeedbackDuration;
        }

        private void ClearFeedback()
        {
            feedbackTimer = 0f;

            if (feedbackText != null)
            {
                feedbackText.text = string.Empty;
            }

        }

        private void PlayFlash(Color color, float alpha)
        {
            if (flashOverlay == null)
            {
                return;
            }

            flashColor = color;
            flashColor.a = alpha;
            flashTimer = FlashDuration;
        }

        private void ShowScorePopup()
        {
            if (scorePopupText == null)
            {
                return;
            }

            scorePopupTimer = ScorePopupDuration;
            scorePopupText.text = "+10";
            scorePopupText.color = new Color(0.45f, 1f, 0.55f, 1f);
            scorePopupText.rectTransform.anchoredPosition = scorePopupStartPosition;
            scorePopupText.gameObject.SetActive(true);
        }

        private void UpdateVisualEffects()
        {
            if (flashTimer > 0f && flashOverlay != null)
            {
                flashTimer -= Time.deltaTime;
                float alpha = flashColor.a * Mathf.Clamp01(flashTimer / FlashDuration);
                flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);

                if (flashTimer <= 0f)
                {
                    flashOverlay.color = Color.clear;
                }
            }

            if (scorePopupTimer > 0f && scorePopupText != null)
            {
                scorePopupTimer -= Time.deltaTime;
                float progress = 1f - Mathf.Clamp01(scorePopupTimer / ScorePopupDuration);
                scorePopupText.rectTransform.anchoredPosition = scorePopupStartPosition + Vector2.up * (70f * progress);

                Color popupColor = scorePopupText.color;
                popupColor.a = 1f - progress;
                scorePopupText.color = popupColor;

                if (scorePopupTimer <= 0f)
                {
                    scorePopupText.gameObject.SetActive(false);
                }
            }
        }

        private void HideVisualEffects()
        {
            if (flashOverlay != null)
            {
                flashOverlay.color = Color.clear;
            }

            if (scorePopupText != null)
            {
                scorePopupText.gameObject.SetActive(false);
            }
        }

        private string GetWrongBinMessage(TrashType trashType)
        {
            string colorName;
            switch (trashType)
            {
                case TrashType.Plastic:
                    colorName = "blue";
                    break;
                case TrashType.Paper:
                    colorName = "yellow";
                    break;
                case TrashType.Glass:
                    colorName = "green";
                    break;
                default:
                    colorName = "correct";
                    break;
            }

            return TrashCatcherTypes.GetDisplayName(trashType) + " should go to the " + colorName + " bin.";
        }

        private void UpdateFeedbackMessage()
        {
            if (feedbackTimer <= 0f || feedbackText == null || IsFinished || isTransitioning)
            {
                return;
            }

            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0f)
            {
                ClearFeedback();
            }
        }

        private void ShowTransitionButton(bool isVisible)
        {
            if (transitionPlayButton != null)
            {
                transitionPlayButton.gameObject.SetActive(isVisible);
            }
        }

        private LevelSettings GetSettingsForCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == LevelTwoSceneName)
            {
                return GetLevelTwoSettings();
            }

            return new LevelSettings
            {
                LevelNumber = 1,
                IncludesHazardous = false,
                SpawnInterval = 1.65f,
                FallSpeed = 1.35f,
                BinWidth = 2.8f,
                BinHeight = 0.75f,
                BinSpeed = 5.2f,
                TrashSize = 0.75f,
                SceneLabel = "Level 1: Practice"
            };
        }

        private LevelSettings GetLevelTwoSettings()
        {
            return new LevelSettings
            {
                LevelNumber = 2,
                IncludesHazardous = true,
                SpawnInterval = 1.4f,
                FallSpeed = 1.5f,
                BinWidth = 2.4f,
                BinHeight = 0.7f,
                BinSpeed = 6.1f,
                TrashSize = 0.65f,
                SceneLabel = "Level 2: Challenge"
            };
        }

        private void UpdateHud()
        {
            if (hudText == null || bin == null)
            {
                return;
            }

            string objective = levelSettings.LevelNumber == 1 ? "Next: Level 2 at 50" : "Goal: 120";
            hudText.text = "Score: " + score
                + "    Health: " + health
                + "    " + levelSettings.SceneLabel
                + "    " + objective
                + "\nSelected: " + TrashCatcherTypes.GetDisplayName(bin.CurrentCategory)
                + "\n1 Plastic    2 Paper    3 Glass    Hazardous: Avoid"
                + "\nMove: A/D or Arrows    Restart: R";
        }
    }
}
