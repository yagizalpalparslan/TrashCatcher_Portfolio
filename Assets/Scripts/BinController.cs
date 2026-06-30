using UnityEngine;

namespace TrashCatcher
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class BinController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float halfWidth = 0.9f;
        private float visualWidth = 1.8f;
        private float visualHeight = 0.6f;

        private readonly TrashType[] categories =
        {
            TrashType.Plastic,
            TrashType.Paper,
            TrashType.Glass
        };

        private int categoryIndex;
        private Camera mainCamera;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D binCollider;

        public TrashType CurrentCategory
        {
            get { return categories[categoryIndex]; }
        }

        public Bounds ColliderBounds
        {
            get { return binCollider.bounds; }
        }

        public void Configure(float width, float height, float speed)
        {
            halfWidth = width * 0.5f;
            moveSpeed = speed;
            visualWidth = width;
            visualHeight = height;
            ApplyCategoryVisual();
        }

        private void Awake()
        {
            mainCamera = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();
            binCollider = GetComponent<BoxCollider2D>();
            ApplyCategoryVisual();
        }

        private void Update()
        {
            float direction = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                direction -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                direction += 1f;
            }

            Vector3 position = transform.position;
            position.x += direction * moveSpeed * Time.deltaTime;
            position.x = Mathf.Clamp(position.x, GetLeftLimit(), GetRightLimit());
            transform.position = position;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SetCategory(TrashType.Plastic);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SetCategory(TrashType.Paper);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SetCategory(TrashType.Glass);
            }
        }

        public void ResetBin()
        {
            categoryIndex = 0;
            transform.position = new Vector3(0f, -4.1f, 0f);
            ApplyCategoryVisual();
        }

        private void SetCategory(TrashType category)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i] == category)
                {
                    categoryIndex = i;
                    ApplyCategoryVisual();
                    return;
                }
            }
        }

        private void ApplyCategoryVisual()
        {
            if (spriteRenderer != null)
            {
                Sprite sprite = PrototypeAssets.GetBinSprite(CurrentCategory);
                PrototypeAssets.AssignSpriteToFit(spriteRenderer, sprite, visualWidth, visualHeight);

                if (sprite == null)
                {
                    spriteRenderer.color = TrashCatcherTypes.GetColor(CurrentCategory);
                }

                UpdateCollider();
            }
        }

        private void UpdateCollider()
        {
            if (binCollider == null)
            {
                binCollider = GetComponent<BoxCollider2D>();
            }

            if (binCollider != null && spriteRenderer.sprite != null)
            {
                binCollider.size = spriteRenderer.sprite.bounds.size;
                binCollider.offset = spriteRenderer.sprite.bounds.center;
            }
        }

        private float GetLeftLimit()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            return mainCamera == null ? -7f : -mainCamera.orthographicSize * mainCamera.aspect + GetColliderHalfWidth();
        }

        private float GetRightLimit()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            return mainCamera == null ? 7f : mainCamera.orthographicSize * mainCamera.aspect - GetColliderHalfWidth();
        }

        private float GetColliderHalfWidth()
        {
            return binCollider == null ? halfWidth : binCollider.bounds.extents.x;
        }
    }
}
