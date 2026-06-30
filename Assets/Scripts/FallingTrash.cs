using UnityEngine;

namespace TrashCatcher
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class FallingTrash : MonoBehaviour
    {
        private TrashCatcherGame game;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D trashCollider;
        private bool wasResolved;

        public TrashType Type { get; private set; }
        public float FallSpeed { get; private set; }
        public Bounds ColliderBounds
        {
            get { return trashCollider.bounds; }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            trashCollider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (game != null && game.IsFinished)
            {
                return;
            }

            transform.position += Vector3.down * FallSpeed * Time.deltaTime;

            if (!wasResolved && game != null && transform.position.y < game.MissY)
            {
                wasResolved = true;
                game.HandleTrashMissed(this);
            }
        }

        public void Initialize(TrashCatcherGame owner, TrashType type, float fallSpeed, float size)
        {
            game = owner;
            Type = type;
            FallSpeed = fallSpeed;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            Sprite sprite = PrototypeAssets.GetTrashSprite(type);
            PrototypeAssets.AssignSpriteToFit(spriteRenderer, sprite, size, size);

            if (sprite == null)
            {
                spriteRenderer.color = TrashCatcherTypes.GetColor(type);
            }

            UpdateCollider();
        }

        public void Resolve()
        {
            wasResolved = true;
        }

        private void UpdateCollider()
        {
            if (trashCollider == null)
            {
                trashCollider = GetComponent<BoxCollider2D>();
            }

            if (trashCollider != null && spriteRenderer.sprite != null)
            {
                trashCollider.size = spriteRenderer.sprite.bounds.size;
                trashCollider.offset = spriteRenderer.sprite.bounds.center;
            }
        }
    }
}
