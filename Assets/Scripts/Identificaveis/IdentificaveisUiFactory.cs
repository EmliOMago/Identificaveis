using UnityEngine;
using UnityEngine.UI;

namespace Identificaveis
{
    public enum IdentificaveisButtonStyle
    {
        Primary = 0,
        Secondary = 1,
        Choice = 2,
        Ghost = 3
    }

    public sealed class IdentificaveisStyledButton
    {
        public Button button;
        public Image background;
        public Text label;
        public Text badge;
    }

    public sealed class IdentificaveisStatTile
    {
        public GameObject root;
        public Text caption;
        public Text value;
    }

    public sealed class IdentificaveisProgressRefs
    {
        public Image fill;
        public Text label;
    }

    public sealed class IdentificaveisUiFactory
    {
        private readonly IdentificaveisThemeAsset _theme;
        private readonly Font _font;
        private readonly float _textScale;
        private readonly Sprite _uiSprite;
        private readonly Sprite _logoSprite;
        private readonly Sprite _gridSprite;
        private readonly Sprite _avatarRingSprite;

        private GameObject _actionButtonPrefab;
        private GameObject _secondaryButtonPrefab;
        private GameObject _choiceButtonPrefab;
        private GameObject _cardPrefab;
        private GameObject _statTilePrefab;
        private GameObject _chipPrefab;

        public IdentificaveisUiFactory(IdentificaveisThemeAsset theme, Font font, float textScale)
        {
            _theme = theme;
            _font = font;
            _textScale = textScale;
            _uiSprite = CreateUiSprite();
            _logoSprite = CreateLogoSprite();
            _gridSprite = CreateGridSprite();
            _avatarRingSprite = CreateAvatarRingSprite();
        }

        public GameObject CreateScreen(string name, Transform parent)
        {
            GameObject screen = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            screen.transform.SetParent(parent, false);

            RectTransform rect = screen.GetComponent<RectTransform>();
            Stretch(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            CanvasGroup group = screen.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            return screen;
        }

        public GameObject CreateBackgroundLayer(Transform parent)
        {
            GameObject root = new GameObject("Background", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image top = CreateImage("TopTint", root.transform, _theme.backgroundTop);
            Stretch(top.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image bottom = CreateImage("BottomTint", root.transform, _theme.backgroundBottom);
            Stretch(bottom.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.58f), Vector2.zero, Vector2.zero);

            if (_gridSprite != null)
            {
                Image grid = CreateImage("SoftGrid", root.transform, new Color(1f, 1f, 1f, 0.18f));
                grid.sprite = _gridSprite;
                grid.preserveAspect = false;
                Stretch(grid.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            if (_logoSprite != null)
            {
                Image mark = CreateImage("BrandWatermark", root.transform, new Color(1f, 1f, 1f, 0.11f));
                mark.sprite = _logoSprite;
                mark.preserveAspect = true;
                RectTransform markRect = mark.rectTransform;
                markRect.anchorMin = new Vector2(1f, 1f);
                markRect.anchorMax = new Vector2(1f, 1f);
                markRect.pivot = new Vector2(1f, 1f);
                markRect.sizeDelta = new Vector2(240f, 240f);
                markRect.anchoredPosition = new Vector2(-42f, -30f);
            }

            return root;
        }

        public RectTransform CreateSafeColumn(string name, Transform parent, Vector2 topBottomPadding, float spacing)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            Stretch(rect, Vector2.zero, Vector2.one, new Vector2(34f, 34f), new Vector2(-34f, -34f));

            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, Mathf.RoundToInt(topBottomPadding.x), Mathf.RoundToInt(topBottomPadding.y));
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rect;
        }

        public GameObject CreateShellCard(string name, Transform parent)
        {
            GameObject card = InstantiateOrFallback(name, GetCardPrefab(), parent, CreateFallbackCard);
            Image image = card.GetComponent<Image>();
            if (image != null)
            {
                image.color = _theme.shellOverlay;
            }

            return card;
        }

        public GameObject CreateCard(string name, Transform parent, bool dark = false)
        {
            GameObject card = InstantiateOrFallback(name, GetCardPrefab(), parent, CreateFallbackCard);
            Image image = card.GetComponent<Image>();
            if (image != null)
            {
                image.color = dark ? _theme.surfaceTertiary : _theme.surfacePrimary;
            }

            Shadow shadow = card.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = card.AddComponent<Shadow>();
                shadow.effectDistance = new Vector2(0f, -10f);
            }

            shadow.effectColor = _theme.shadow;
            return card;
        }

        public RectTransform CreateVerticalContent(GameObject owner, Vector2 padding, float spacing, TextAnchor alignment)
        {
            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(owner.transform, false);
            RectTransform rect = content.GetComponent<RectTransform>();
            Stretch(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.y), Mathf.RoundToInt(padding.y));
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rect;
        }

        public RectTransform CreateScrollCard(string name, Transform parent, float minHeight)
        {
            GameObject card = CreateCard(name, parent);
            LayoutElement element = card.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = card.AddComponent<LayoutElement>();
            }

            element.minHeight = minHeight;
            element.flexibleHeight = 1f;

            GameObject root = new GameObject("ScrollRoot", typeof(RectTransform), typeof(ScrollRect));
            root.transform.SetParent(card.transform, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            Stretch(rootRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.001f);
            EnsureSprite(viewportImage);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 30);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = root.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.scrollSensitivity = 26f;

            return contentRect;
        }

        public Text CreateText(string name, Transform parent, int size, FontStyle style, TextAnchor anchor, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = Mathf.RoundToInt(size * _textScale);
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;

            ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return text;
        }

        public Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            EnsureSprite(image);
            return image;
        }

        public GameObject CreateDivider(Transform parent, float opacity = 1f)
        {
            Image image = CreateImage("Divider", parent, MultiplyAlpha(_theme.outline, opacity));
            LayoutElement element = image.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 2f;
            element.minHeight = 2f;
            return image.gameObject;
        }

        public GameObject CreateHeaderBlock(Transform parent, string phaseText, string titleText, string subtitleText, out Text phaseLabel, out Text title, out Text subtitle)
        {
            GameObject root = new GameObject("HeaderBlock", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            phaseLabel = CreateText("Phase", root.transform, _theme.smallSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.accent);
            phaseLabel.text = phaseText;

            title = CreateText("Title", root.transform, _theme.titleSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            title.text = titleText;

            subtitle = CreateText("Subtitle", root.transform, _theme.subtitleSize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkSecondary);
            subtitle.text = subtitleText;
            return root;
        }

        public IdentificaveisStyledButton CreateButton(Transform parent, string labelText, IdentificaveisButtonStyle style)
        {
            GameObject prefab;
            switch (style)
            {
                case IdentificaveisButtonStyle.Secondary:
                    prefab = GetSecondaryButtonPrefab();
                    break;
                case IdentificaveisButtonStyle.Choice:
                    prefab = GetChoiceButtonPrefab();
                    break;
                case IdentificaveisButtonStyle.Ghost:
                    prefab = GetSecondaryButtonPrefab();
                    break;
                default:
                    prefab = GetActionButtonPrefab();
                    break;
            }

            GameObject instance = InstantiateOrFallback(labelText + "Button", prefab, parent, style == IdentificaveisButtonStyle.Choice ? CreateFallbackChoiceButton : CreateFallbackActionButton);
            IdentificaveisStyledButton refs = new IdentificaveisStyledButton();
            refs.button = instance.GetComponent<Button>();
            refs.background = instance.GetComponent<Image>();
            refs.label = FindFirstText(instance.transform);
            refs.badge = FindNamedText(instance.transform, "Badge");

            ApplyButtonStyle(refs, style);
            if (refs.label != null)
            {
                refs.label.text = labelText;
            }

            return refs;
        }

        public IdentificaveisStatTile CreateStatTile(Transform parent, string captionText, string valueText)
        {
            GameObject tile = InstantiateOrFallback("StatTile", GetStatTilePrefab(), parent, CreateFallbackStatTile);
            Image image = tile.GetComponent<Image>();
            if (image != null)
            {
                EnsureSprite(image);
                image.color = _theme.surfaceSecondary;
            }

            IdentificaveisStatTile result = new IdentificaveisStatTile();
            result.root = tile;
            result.caption = FindNamedText(tile.transform, "Caption");
            result.value = FindNamedText(tile.transform, "Value");
            if (result.caption != null)
            {
                result.caption.text = captionText;
                NormalizeText(result.caption, _theme.smallSize, FontStyle.Bold, _theme.inkSecondary);
            }

            if (result.value != null)
            {
                result.value.text = valueText;
                NormalizeText(result.value, _theme.subtitleSize, FontStyle.Bold, _theme.inkPrimary);
            }

            return result;
        }

        public GameObject CreateChip(Transform parent, string text)
        {
            GameObject chip = InstantiateOrFallback("Chip", GetChipPrefab(), parent, CreateFallbackChip);
            Image image = chip.GetComponent<Image>();
            if (image != null)
            {
                EnsureSprite(image);
                image.color = _theme.accentSoft;
            }

            Text label = FindFirstText(chip.transform);
            if (label != null)
            {
                label.text = text;
                NormalizeText(label, _theme.smallSize, FontStyle.Bold, _theme.accent);
            }

            return chip;
        }

        public IdentificaveisProgressRefs CreateProgressBar(Transform parent)
        {
            GameObject root = new GameObject("ProgressBlock", typeof(RectTransform));
            root.transform.SetParent(parent, false);

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text label = CreateText("ProgressLabel", root.transform, _theme.smallSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkSecondary);
            label.text = "Progresso";

            Image track = CreateImage("Track", root.transform, _theme.surfaceSecondary);
            LayoutElement trackLayout = track.gameObject.AddComponent<LayoutElement>();
            trackLayout.minHeight = 18f;
            trackLayout.preferredHeight = 18f;

            GameObject fillRoot = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillRoot.transform.SetParent(track.transform, false);
            Image fill = fillRoot.GetComponent<Image>();
            fill.color = _theme.accent;
            EnsureSprite(fill);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0f, 0f);

            IdentificaveisProgressRefs refs = new IdentificaveisProgressRefs();
            refs.fill = fill;
            refs.label = label;
            return refs;
        }


        public Image CreateAvatarRing(Transform parent, float size)
        {
            Image image = CreateImage("AvatarRing", parent, new Color(1f, 1f, 1f, 0.92f));
            RectTransform rect = image.rectTransform;
            rect.sizeDelta = new Vector2(size, size);
            if (_avatarRingSprite != null)
            {
                image.sprite = _avatarRingSprite;
                image.preserveAspect = true;
            }

            return image;
        }

        public Image CreateLogoMark(Transform parent, float size)
        {
            Image image = CreateImage("LogoMark", parent, _theme.accent);
            RectTransform rect = image.rectTransform;
            rect.sizeDelta = new Vector2(size, size);
            if (_logoSprite != null)
            {
                image.sprite = _logoSprite;
                image.preserveAspect = true;
            }
            return image;
        }

        public void ApplyDarkText(Text text)
        {
            if (text != null)
            {
                text.color = _theme.inkPrimary;
            }
        }

        public void ApplyMutedText(Text text)
        {
            if (text != null)
            {
                text.color = _theme.inkSecondary;
            }
        }

        public void ApplyInvertedText(Text text)
        {
            if (text != null)
            {
                text.color = _theme.inkInverted;
            }
        }

        public void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private void ApplyButtonStyle(IdentificaveisStyledButton refs, IdentificaveisButtonStyle style)
        {
            if (refs == null || refs.button == null || refs.background == null)
            {
                return;
            }

            Color background;
            Color textColor;
            switch (style)
            {
                case IdentificaveisButtonStyle.Secondary:
                    background = _theme.surfaceSecondary;
                    textColor = _theme.inkPrimary;
                    break;
                case IdentificaveisButtonStyle.Choice:
                    background = _theme.surfacePrimary;
                    textColor = _theme.inkPrimary;
                    break;
                case IdentificaveisButtonStyle.Ghost:
                    background = MultiplyAlpha(_theme.surfacePrimary, 0.7f);
                    textColor = _theme.inkPrimary;
                    break;
                default:
                    background = _theme.accent;
                    textColor = _theme.inkInverted;
                    break;
            }

            EnsureSprite(refs.background);
            refs.background.color = background;
            ColorBlock colors = refs.button.colors;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.08f);
            colors.selectedColor = background;
            colors.disabledColor = MultiplyAlpha(background, 0.45f);
            refs.button.colors = colors;

            if (refs.label != null)
            {
                NormalizeText(refs.label, _theme.buttonSize, FontStyle.Bold, textColor);
            }

            if (refs.badge != null)
            {
                NormalizeText(refs.badge, _theme.smallSize, FontStyle.Bold, style == IdentificaveisButtonStyle.Primary ? MultiplyAlpha(_theme.inkInverted, 0.9f) : _theme.accent);
                refs.badge.gameObject.SetActive(style == IdentificaveisButtonStyle.Choice);
            }
        }

        private GameObject InstantiateOrFallback(string name, GameObject prefab, Transform parent, System.Func<Transform, GameObject> fallbackFactory)
        {
            GameObject instance = prefab != null ? Object.Instantiate(prefab, parent, false) : fallbackFactory(parent);
            instance.name = name;
            return instance;
        }

        private GameObject GetActionButtonPrefab()
        {
            return _actionButtonPrefab;
        }

        private GameObject GetSecondaryButtonPrefab()
        {
            return _secondaryButtonPrefab;
        }

        private GameObject GetChoiceButtonPrefab()
        {
            return _choiceButtonPrefab;
        }

        private GameObject GetCardPrefab()
        {
            return _cardPrefab;
        }

        private GameObject GetStatTilePrefab()
        {
            return _statTilePrefab;
        }

        private GameObject GetChipPrefab()
        {
            return _chipPrefab;
        }

        private GameObject CreateFallbackCard(Transform parent)
        {
            GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(parent, false);
            Image image = card.GetComponent<Image>();
            EnsureSprite(image);
            image.color = _theme.surfacePrimary;
            LayoutElement element = card.GetComponent<LayoutElement>();
            element.minHeight = 120f;
            return card;
        }

        private GameObject CreateFallbackActionButton(Transform parent)
        {
            GameObject button = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            button.transform.SetParent(parent, false);
            Image image = button.GetComponent<Image>();
            EnsureSprite(image);
            LayoutElement element = button.GetComponent<LayoutElement>();
            element.minHeight = 106f;
            element.flexibleWidth = 1f;

            Text label = CreateText("Label", button.transform, _theme.buttonSize, FontStyle.Bold, TextAnchor.MiddleCenter, _theme.inkInverted);
            Stretch(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 14f), new Vector2(-20f, -14f));
            label.text = "Botão";
            return button;
        }

        private GameObject CreateFallbackChoiceButton(Transform parent)
        {
            GameObject button = CreateFallbackActionButton(parent);
            LayoutElement element = button.GetComponent<LayoutElement>();
            element.minHeight = 136f;
            Image image = button.GetComponent<Image>();
            image.color = _theme.surfacePrimary;

            Text badge = CreateText("Badge", button.transform, _theme.smallSize, FontStyle.Bold, TextAnchor.MiddleCenter, _theme.accent);
            RectTransform badgeRect = badge.rectTransform;
            badgeRect.anchorMin = new Vector2(1f, 0.5f);
            badgeRect.anchorMax = new Vector2(1f, 0.5f);
            badgeRect.pivot = new Vector2(1f, 0.5f);
            badgeRect.sizeDelta = new Vector2(54f, 54f);
            badgeRect.anchoredPosition = new Vector2(-18f, 0f);
            badge.text = "A";
            return button;
        }

        private GameObject CreateFallbackStatTile(Transform parent)
        {
            GameObject tile = new GameObject("StatTile", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            tile.transform.SetParent(parent, false);
            Image image = tile.GetComponent<Image>();
            EnsureSprite(image);
            LayoutElement element = tile.GetComponent<LayoutElement>();
            element.minHeight = 136f;
            element.flexibleWidth = 1f;

            RectTransform content = CreateVerticalContent(tile, new Vector2(20f, 20f), 8f, TextAnchor.UpperLeft);
            Text caption = CreateText("Caption", content, _theme.smallSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkSecondary);
            caption.text = "Legenda";
            Text value = CreateText("Value", content, _theme.subtitleSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            value.text = "0";
            return tile;
        }

        private GameObject CreateFallbackChip(Transform parent)
        {
            GameObject chip = new GameObject("Chip", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            chip.transform.SetParent(parent, false);
            Image image = chip.GetComponent<Image>();
            EnsureSprite(image);
            image.color = _theme.accentSoft;
            LayoutElement element = chip.GetComponent<LayoutElement>();
            element.minHeight = 60f;
            element.flexibleWidth = 0f;

            HorizontalLayoutGroup layout = chip.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            ContentSizeFitter fitter = chip.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text label = CreateText("Label", chip.transform, _theme.smallSize, FontStyle.Bold, TextAnchor.MiddleCenter, _theme.accent);
            label.text = "Chip";
            return chip;
        }

        private Text FindFirstText(Transform root)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            return texts != null && texts.Length > 0 ? texts[0] : null;
        }

        private Text FindNamedText(Transform root, string token)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return texts[i];
                }
            }

            return null;
        }

        private void EnsureSprite(Image image)
        {
            if (image != null && image.sprite == null)
            {
                image.sprite = _uiSprite;
                image.type = Image.Type.Sliced;
                image.preserveAspect = false;
            }
        }

        private void NormalizeText(Text text, int size, FontStyle style, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.font = _font;
            text.fontSize = Mathf.RoundToInt(size * _textScale);
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private Sprite CreateUiSprite()
        {
            Texture2D texture = NewTexture(3, 3);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }

            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, 3f, 3f), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, new Vector4(1f, 1f, 1f, 1f));
        }

        private Sprite CreateLogoSprite()
        {
            const int size = 128;
            Texture2D texture = NewTexture(size, size);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.36f;
            float inner = size * 0.18f;
            float barHalf = size * 0.065f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    bool ring = distance <= outer && distance >= inner;
                    bool vertical = Mathf.Abs(dx) <= barHalf && Mathf.Abs(dy) <= outer;
                    bool horizontal = Mathf.Abs(dy) <= barHalf && Mathf.Abs(dx) <= outer;

                    float alpha = ring ? 0.92f : 0f;
                    if (vertical || horizontal)
                    {
                        alpha = 1f;
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            return MakeSprite(texture);
        }

        private Sprite CreateGridSprite()
        {
            const int size = 64;
            Texture2D texture = NewTexture(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool major = x % 16 == 0 || y % 16 == 0;
                    bool minor = x % 8 == 0 || y % 8 == 0;
                    float alpha = major ? 0.22f : (minor ? 0.08f : 0f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            return MakeSprite(texture);
        }

        private Sprite CreateAvatarRingSprite()
        {
            const int size = 128;
            Texture2D texture = NewTexture(size, size);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.47f;
            float inner = size * 0.39f;
            float glow = size * 0.31f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 0f;

                    if (distance <= outer && distance >= inner)
                    {
                        alpha = 1f;
                    }
                    else if (distance < inner && distance >= glow)
                    {
                        alpha = Mathf.InverseLerp(inner, glow, distance) * 0.18f;
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            return MakeSprite(texture);
        }

        private static Texture2D NewTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "IdentificaveisRuntimeTexture";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static Sprite MakeSprite(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = texture.name + "_Sprite";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private Color MultiplyAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, color.a * alpha);
        }
    }
}
