using UnityEngine;

namespace TrashCatcher
{
    public enum TrashType
    {
        Plastic,
        Paper,
        Glass,
        Hazardous
    }

    public static class TrashCatcherTypes
    {
        public static Color GetColor(TrashType type)
        {
            switch (type)
            {
                case TrashType.Plastic:
                    return new Color(0.1f, 0.35f, 1f);
                case TrashType.Paper:
                    return new Color(1f, 0.85f, 0.05f);
                case TrashType.Glass:
                    return new Color(0.05f, 0.75f, 0.25f);
                case TrashType.Hazardous:
                    return new Color(0.95f, 0.08f, 0.05f);
                default:
                    return Color.white;
            }
        }

        public static string GetDisplayName(TrashType type)
        {
            switch (type)
            {
                case TrashType.Plastic:
                    return "Plastic";
                case TrashType.Paper:
                    return "Paper";
                case TrashType.Glass:
                    return "Glass";
                case TrashType.Hazardous:
                    return "Hazardous";
                default:
                    return type.ToString();
            }
        }
    }
}
