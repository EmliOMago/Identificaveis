#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Identificaveis.Editor
{
    [InitializeOnLoad]
    public static class IdentificaveisAuthoringInstaller
    {
        private const string ResourcesRoot = "Assets/Resources/Identificaveis";
        private const string PrefabsRoot = ResourcesRoot + "/Prefabs";
        private const string SpritesRoot = ResourcesRoot + "/Sprites";
        private const string ThemeAssetPath = ResourcesRoot + "/IdentificaveisTheme.asset";

        static IdentificaveisAuthoringInstaller()
        {
            EditorApplication.delayCall += TryInstall;
        }

        [MenuItem("Identificaveis/Regenerar prefabs de UI")]
        public static void ForceInstall()
        {
            Install(force: true);
        }

        private static void TryInstall()
        {
            if (Application.isPlaying)
            {
                return;
            }

            Install(force: false);
        }

        private static void Install(bool force)
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder(ResourcesRoot);
            EnsureFolder(PrefabsRoot);
            EnsureFolder(SpritesRoot);

            ConfigureSpriteImporter(SpritesRoot + "/logo_mark.png");
            ConfigureSpriteImporter(SpritesRoot + "/soft_grid.png");
            ConfigureSpriteImporter(SpritesRoot + "/avatar_ring.png");

            EnsureThemeAsset(force);
            EnsurePrefab(PrefabsRoot + "/ActionButton.prefab", CreateActionButtonPrefab, force);
            EnsurePrefab(PrefabsRoot + "/SecondaryButton.prefab", CreateSecondaryButtonPrefab, force);
            EnsurePrefab(PrefabsRoot + "/ChoiceButton.prefab", CreateChoiceButtonPrefab, force);
            EnsurePrefab(PrefabsRoot + "/SurfaceCard.prefab", CreateSurfaceCardPrefab, force);
            EnsurePrefab(PrefabsRoot + "/StatTile.prefab", CreateStatTilePrefab, force);
            EnsurePrefab(PrefabsRoot + "/Chip.prefab", CreateChipPrefab, force);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureThemeAsset(bool force)
        {
            if (!force && File.Exists(ThemeAssetPath))
            {
                return;
            }

            IdentificaveisThemeAsset asset = AssetDatabase.LoadAssetAtPath<IdentificaveisThemeAsset>(ThemeAssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<IdentificaveisThemeAsset>();
                AssetDatabase.CreateAsset(asset, ThemeAssetPath);
            }

            EditorUtility.SetDirty(asset);
        }

        private static void EnsurePrefab(string assetPath, System.Func<GameObject> createFunc, bool force)
        {
            if (!force && File.Exists(assetPath))
            {
                return;
            }

            GameObject root = createFunc();
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateActionButtonPrefab()
        {
            GameObject root = CreateButtonRoot("ActionButton", 106f);
            CreateFullLabel(root.transform, "Label", TextAnchor.MiddleCenter);
            return root;
        }

        private static GameObject CreateSecondaryButtonPrefab()
        {
            GameObject root = CreateButtonRoot("SecondaryButton", 96f);
            CreateFullLabel(root.transform, "Label", TextAnchor.MiddleCenter);
            return root;
        }

        private static GameObject CreateChoiceButtonPrefab()
        {
            GameObject root = CreateButtonRoot("ChoiceButton", 136f);
            RectTransform label = CreateText(root.transform, "Label", 28, FontStyle.Bold, TextAnchor.MiddleLeft).rectTransform;
            Stretch(label, Vector2.zero, Vector2.one, new Vector2(28f, 20f), new Vector2(-96f, -20f));

            RectTransform badge = CreateText(root.transform, "Badge", 20, FontStyle.Bold, TextAnchor.MiddleCenter).rectTransform;
            badge.anchorMin = new Vector2(1f, 0.5f);
            badge.anchorMax = new Vector2(1f, 0.5f);
            badge.pivot = new Vector2(1f, 0.5f);
            badge.sizeDelta = new Vector2(54f, 54f);
            badge.anchoredPosition = new Vector2(-18f, 0f);
            return root;
        }

        private static GameObject CreateSurfaceCardPrefab()
        {
            GameObject root = new GameObject("SurfaceCard", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Shadow));
            Image image = root.GetComponent<Image>();
            image.sprite = GetUiSprite();
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            LayoutElement element = root.GetComponent<LayoutElement>();
            element.minHeight = 120f;
            Shadow shadow = root.GetComponent<Shadow>();
            shadow.effectDistance = new Vector2(0f, -10f);
            shadow.effectColor = new Color(0.09f, 0.13f, 0.20f, 0.18f);
            return root;
        }

        private static GameObject CreateStatTilePrefab()
        {
            GameObject root = new GameObject("StatTile", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            Image image = root.GetComponent<Image>();
            image.sprite = GetUiSprite();
            image.type = Image.Type.Sliced;
            LayoutElement element = root.GetComponent<LayoutElement>();
            element.minHeight = 136f;
            element.flexibleWidth = 1f;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(root.transform, false);
            Stretch(content.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(22f, 18f), new Vector2(-22f, -18f));
            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateText(content.transform, "Caption", 20, FontStyle.Bold, TextAnchor.UpperLeft);
            CreateText(content.transform, "Value", 30, FontStyle.Bold, TextAnchor.UpperLeft);
            return root;
        }

        private static GameObject CreateChipPrefab()
        {
            GameObject root = new GameObject("Chip", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            Image image = root.GetComponent<Image>();
            image.sprite = GetUiSprite();
            image.type = Image.Type.Sliced;
            LayoutElement element = root.GetComponent<LayoutElement>();
            element.minHeight = 60f;
            HorizontalLayoutGroup layout = root.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            ContentSizeFitter fitter = root.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            CreateText(root.transform, "Label", 20, FontStyle.Bold, TextAnchor.MiddleCenter);
            return root;
        }

        private static GameObject CreateButtonRoot(string name, float minHeight)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            Image image = root.GetComponent<Image>();
            image.sprite = GetUiSprite();
            image.type = Image.Type.Sliced;
            LayoutElement element = root.GetComponent<LayoutElement>();
            element.minHeight = minHeight;
            element.flexibleWidth = 1f;
            return root;
        }

        private static Text CreateFullLabel(Transform parent, string name, TextAnchor anchor)
        {
            Text text = CreateText(parent, name, 28, FontStyle.Bold, anchor);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 14f), new Vector2(-20f, -14f));
            return text;
        }

        private static Text CreateText(Transform parent, string name, int size, FontStyle style, TextAnchor anchor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = GetFont();
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = new Color(0.1f, 0.15f, 0.24f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;
            return text;
        }

        private static void ConfigureSpriteImporter(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.alphaIsTransparency != true)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static Font GetFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private static Sprite GetUiSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
#endif
