using UnityEngine;

namespace Identificaveis
{
    [CreateAssetMenu(fileName = "IdentificaveisTheme", menuName = "Identificaveis/Theme")]
    public sealed class IdentificaveisThemeAsset : ScriptableObject
    {
        public Color backgroundTop = new Color(0.96f, 0.97f, 0.99f, 1f);
        public Color backgroundBottom = new Color(0.91f, 0.94f, 0.98f, 1f);
        public Color shellOverlay = new Color(1f, 1f, 1f, 0.66f);
        public Color surfacePrimary = Color.white;
        public Color surfaceSecondary = new Color(0.92f, 0.94f, 0.98f, 1f);
        public Color surfaceTertiary = new Color(0.14f, 0.19f, 0.29f, 1f);
        public Color outline = new Color(0.74f, 0.79f, 0.88f, 1f);
        public Color inkPrimary = new Color(0.10f, 0.15f, 0.24f, 1f);
        public Color inkSecondary = new Color(0.34f, 0.39f, 0.49f, 1f);
        public Color inkInverted = Color.white;
        public Color accent = new Color(0.18f, 0.35f, 0.67f, 1f);
        public Color accentSoft = new Color(0.84f, 0.89f, 0.98f, 1f);
        public Color success = new Color(0.17f, 0.62f, 0.36f, 1f);
        public Color error = new Color(0.80f, 0.24f, 0.26f, 1f);
        public Color warning = new Color(0.80f, 0.58f, 0.20f, 1f);
        public Color shadow = new Color(0.09f, 0.13f, 0.20f, 0.18f);

        public int titleSize = 62;
        public int subtitleSize = 28;
        public int bodySize = 26;
        public int smallSize = 22;
        public int buttonSize = 28;

        public float sectionSpacing = 24f;
        public float cardRadiusHint = 26f;
        public float transitionDuration = 0.18f;

        private const string ResourcePath = "Identificaveis/IdentificaveisTheme";
        private static IdentificaveisThemeAsset _source;

        public static IdentificaveisThemeAsset Load()
        {
            if (_source == null)
            {
                _source = Resources.Load<IdentificaveisThemeAsset>(ResourcePath);
            }

            IdentificaveisThemeAsset runtime = _source != null ? Instantiate(_source) : CreateInstance<IdentificaveisThemeAsset>();
            runtime.name = "IdentificaveisTheme_Runtime";
            return runtime;
        }
    }
}
