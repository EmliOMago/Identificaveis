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
        private const string LegacyResourcesRoot = "Assets/Resources/Identificaveis";
        private const string LegacyPrefabsRoot = LegacyResourcesRoot + "/Prefabs";
        private const string LegacySpritesRoot = LegacyResourcesRoot + "/Sprites";
        private const string LegacyThemeAssetPath = LegacyResourcesRoot + "/IdentificaveisTheme.asset";
        private const string LegacyDataPath = LegacyResourcesRoot + "/default_content.json";

        private const string DataRoot = "Assets/Resources/Dados/Identificaveis";
        private const string DataPath = DataRoot + "/default_content.json";
        private const string PrefabsRoot = "Assets/Prefabs/Identificaveis";
        private const string SpritesRoot = "Assets/Art/Identificaveis/Sprites";
        private const string ThemeRoot = "Assets/Configs/Identificaveis";
        private const string ThemeAssetPath = ThemeRoot + "/IdentificaveisTheme.asset";

        static IdentificaveisAuthoringInstaller()
        {
            EditorApplication.delayCall += TryInstall;
        }

        [MenuItem("Identificaveis/Reorganizar assets autorais")]
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
            EnsureFolder("Assets/Resources/Dados");
            EnsureFolder(DataRoot);
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(PrefabsRoot);
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/Identificaveis");
            EnsureFolder(SpritesRoot);
            EnsureFolder("Assets/Configs");
            EnsureFolder(ThemeRoot);

            MoveIfNeeded(LegacyDataPath, DataPath);
            MoveIfNeeded(LegacyThemeAssetPath, ThemeAssetPath);
            MoveIfNeeded(LegacyPrefabsRoot + "/ActionButton.prefab", PrefabsRoot + "/ActionButton.prefab");
            MoveIfNeeded(LegacyPrefabsRoot + "/SecondaryButton.prefab", PrefabsRoot + "/SecondaryButton.prefab");
            MoveIfNeeded(LegacyPrefabsRoot + "/ChoiceButton.prefab", PrefabsRoot + "/ChoiceButton.prefab");
            MoveIfNeeded(LegacyPrefabsRoot + "/SurfaceCard.prefab", PrefabsRoot + "/SurfaceCard.prefab");
            MoveIfNeeded(LegacyPrefabsRoot + "/StatTile.prefab", PrefabsRoot + "/StatTile.prefab");
            MoveIfNeeded(LegacyPrefabsRoot + "/Chip.prefab", PrefabsRoot + "/Chip.prefab");
            MoveIfNeeded(LegacySpritesRoot + "/logo_mark.png", SpritesRoot + "/logo_mark.png");
            MoveIfNeeded(LegacySpritesRoot + "/soft_grid.png", SpritesRoot + "/soft_grid.png");
            MoveIfNeeded(LegacySpritesRoot + "/avatar_ring.png", SpritesRoot + "/avatar_ring.png");

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

            DeleteFolderIfEmpty(LegacyPrefabsRoot);
            DeleteFolderIfEmpty(LegacySpritesRoot);
            DeleteFolderIfEmpty(LegacyResourcesRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void MoveIfNeeded(string oldPath, string newPath)
        {
            if (!AssetExists(oldPath))
            {
                return;
            }

            if (AssetExists(newPath))
            {
                AssetDatabase.DeleteAsset(oldPath);
                return;
            }

            string directory = Path.GetDirectoryName(newPath).Replace("\\", "/");
            EnsureFolder(directory);
            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning("[Identificáveis] Não foi possível mover asset para a nova pasta: " + error);
            }
        }

        private static bool AssetExists(string path)
        {
            return AssetDatabase.LoadMainAssetAtPath(path) != null || File.Exists(path) || Directory.Exists(path);
        }

        private static void DeleteFolderIfEmpty(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string[] assets = AssetDatabase.FindAssets(string.Empty, new[] { path });
            if (assets.Length == 0)
            {
                AssetDatabase.DeleteAsset(path);
            }
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
            if (image.sprite != null)
            {
                image.type = Image.Type.Sliced;
            }
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
            if (image.sprite != null)
            {
                image.type = Image.Type.Sliced;
            }
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
            if (image.sprite != null)
            {
                image.type = Image.Type.Sliced;
            }
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
            if (image.sprite != null)
            {
                image.type = Image.Type.Sliced;
            }
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

            if (!importer.alphaIsTransparency)
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
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Segoe UI", "Tahoma", "Verdana" }, 16);
            }

            return font;
        }

        private static Sprite GetUiSprite()
        {
            return null;
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
