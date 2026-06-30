using UnityEngine;

namespace TrashCatcher
{
    public static class PrototypeAssets
    {
        public static Sprite GetTrashSprite(TrashType type)
        {
            switch (type)
            {
                case TrashType.Plastic:
                    return LoadSprite("Art/Sprites/Trash/plastic_trash");
                case TrashType.Paper:
                    return LoadSprite("Art/Sprites/Trash/paper_trash");
                case TrashType.Glass:
                    return LoadSprite("Art/Sprites/Trash/glass_trash");
                case TrashType.Hazardous:
                    return LoadSprite("Art/Sprites/Trash/hazardous_trash");
                default:
                    return null;
            }
        }

        public static Sprite GetBinSprite(TrashType type)
        {
            switch (type)
            {
                case TrashType.Plastic:
                    return LoadSprite("Art/Sprites/Bins/plastic_bin");
                case TrashType.Paper:
                    return LoadSprite("Art/Sprites/Bins/paper_bin");
                case TrashType.Glass:
                    return LoadSprite("Art/Sprites/Bins/glass_bin");
                default:
                    return null;
            }
        }

        public static Sprite GetBackgroundSprite(int levelNumber)
        {
            return LoadSprite("Art/Sprites/Backgrounds/level" + levelNumber + "_background");
        }

        public static void AssignSpriteToFit(SpriteRenderer renderer, Sprite sprite, float width, float height)
        {
            if (sprite == null)
            {
                renderer.sprite = PrototypeSprites.Square;
                renderer.transform.localScale = new Vector3(width, height, 1f);
                return;
            }

            renderer.sprite = sprite;
            renderer.color = Color.white;

            Vector2 spriteSize = sprite.bounds.size;
            float scale = Mathf.Min(width / spriteSize.x, height / spriteSize.y);
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public static void AssignSpriteToCover(SpriteRenderer renderer, Sprite sprite, float width, float height)
        {
            if (sprite == null)
            {
                return;
            }

            renderer.sprite = sprite;
            renderer.color = Color.white;

            Vector2 spriteSize = sprite.bounds.size;
            float scale = Mathf.Max(width / spriteSize.x, height / spriteSize.y);
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static Sprite LoadSprite(string path)
        {
            return Resources.Load<Sprite>(path);
        }
    }
}
