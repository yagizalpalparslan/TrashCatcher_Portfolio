using UnityEngine;

namespace TrashCatcher
{
    public static class PrototypeSprites
    {
        private static Sprite squareSprite;

        public static Sprite Square
        {
            get
            {
                if (squareSprite == null)
                {
                    Texture2D texture = new Texture2D(1, 1);
                    texture.name = "TrashCatcherSquare";
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();

                    squareSprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, 1f, 1f),
                        new Vector2(0.5f, 0.5f),
                        1f);
                }

                return squareSprite;
            }
        }
    }
}
