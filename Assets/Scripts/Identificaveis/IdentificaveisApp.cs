using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Identificaveis
{
    public sealed class IdentificaveisApp : MonoBehaviour
    {
        private IdentificaveisContentDatabase _database;
        private IdentificaveisThemeAsset _theme;
        private AppPreferences _preferences;
        private SessionState _session;
        private SessionAnalysis _analysis;

        [Header("Autorais opcionais")]
        [SerializeField] private IdentificaveisThemeAsset _themePreset;

        private Font _font;
        private IdentificaveisUiFactory _ui;

        private Canvas _canvas;
        private GameObject _homeScreen;
        private GameObject _messageScreen;
        private GameObject _gameplayScreen;
        private GameObject _resultsScreen;
        private CanvasGroup _activeScreen;

        private Text _homeTitle;
        private Text _homeSubtitle;
        private Text _homeModeLabel;
        private Text _homeContrastLabel;
        private Text _homeTextSizeLabel;
        private Text _homeRoundLabel;
        private IdentificaveisStatTile _homeBestTile;
        private IdentificaveisStatTile _homePlayedTile;
        private IdentificaveisStatTile _homeFormatTile;

        private Text _messageTitle;
        private Text _messageBody;
        private IdentificaveisStyledButton _messagePrimaryButton;
        private IdentificaveisStyledButton _messageSecondaryButton;
        private Action _messagePrimaryAction;
        private Action _messageSecondaryAction;

        private Text _phaseLabel;
        private Text _gameTitle;
        private Text _gameSubtitle;
        private IdentificaveisProgressRefs _progress;
        private Text _avatarMonogram;
        private Text _avatarDescription;
        private Text _contentHeading;
        private Text _contentBody;
        private GameObject _readingChip;
        private Text _readingChipLabel;
        private GameObject _feedbackCard;
        private Text _feedbackTitle;
        private Text _feedbackBody;
        private IdentificaveisStyledButton[] _optionButtons = new IdentificaveisStyledButton[4];
        private readonly ScenarioChoiceData[] _displayedScenarioChoices = new ScenarioChoiceData[4];
        private IdentificaveisStyledButton _nextButton;
        private IdentificaveisStyledButton _leaveButton;

        private Text _resultsHeadline;
        private Text _resultsBody;
        private IdentificaveisStatTile _resultsProfileTile;
        private IdentificaveisStatTile _resultsScenarioTile;
        private IdentificaveisStatTile _resultsTotalTile;
        private IdentificaveisStyledButton _resultsPrimaryButton;
        private IdentificaveisStyledButton _resultsSecondaryButton;
        private IdentificaveisStyledButton _resultsTertiaryButton;

        private float TextScale => _preferences != null && _preferences.largeText ? 1.15f : 1f;

        private void Awake()
        {
            _database = IdentificaveisContentRepository.Load();
            _theme = IdentificaveisThemeAsset.CreateRuntime(_themePreset);
            _preferences = IdentificaveisPreferencesStore.LoadPreferences();
            if (_preferences != null && _preferences.highContrast)
            {
                ApplyHighContrastPalette();
            }

            _font = LoadFont();
            _ui = new IdentificaveisUiFactory(_theme, _font, TextScale);

            EnsureEventSystem();
            BuildUi();
            ShowHome();
        }

        private void BuildUi()
        {
            GameObject canvasGo = new GameObject("IdentificaveisCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.65f;

            RectTransform root = canvasGo.GetComponent<RectTransform>();
            _ui.Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            _ui.CreateBackgroundLayer(root);

            _homeScreen = BuildHomeScreen(root);
            _messageScreen = BuildMessageScreen(root);
            _gameplayScreen = BuildGameplayScreen(root);
            _resultsScreen = BuildResultsScreen(root);

            ActivateScreen(_homeScreen.GetComponent<CanvasGroup>(), false);
            ActivateScreen(_messageScreen.GetComponent<CanvasGroup>(), false);
            ActivateScreen(_gameplayScreen.GetComponent<CanvasGroup>(), false);
            ActivateScreen(_resultsScreen.GetComponent<CanvasGroup>(), false);
        }

        private GameObject BuildHomeScreen(Transform parent)
        {
            GameObject screen = _ui.CreateScreen("HomeScreen", parent);
            RectTransform column = _ui.CreateSafeColumn("SafeArea", screen.transform, new Vector2(40f, 40f), 24f);

            GameObject hero = _ui.CreateShellCard("HeroCard", column);
            RectTransform heroContent = _ui.CreateVerticalContent(hero, new Vector2(34f, 34f), 18f, TextAnchor.UpperLeft);
            _ui.CreateChip(heroContent, "Puzzle narrativo de percepção social");
            _ui.CreateLogoMark(heroContent, 120f);

            _homeTitle = _ui.CreateText("HomeTitle", heroContent, _theme.titleSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            _homeTitle.text = string.IsNullOrEmpty(_database.displayName) ? "Identificáveis" : _database.displayName;

            _homeSubtitle = _ui.CreateText("HomeSubtitle", heroContent, _theme.subtitleSize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkSecondary);
            _homeSubtitle.text = string.IsNullOrEmpty(_database.subtitle)
                ? "Observe perfis e reações. Tente perceber o que soa humano de verdade."
                : _database.subtitle;

            _homeRoundLabel = _ui.CreateText("RoundLabel", heroContent, _theme.bodySize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkSecondary);
            _homeRoundLabel.text = BuildMenuRoundText();

            GameObject statsRow = new GameObject("StatsRow", typeof(RectTransform));
            statsRow.transform.SetParent(column, false);
            HorizontalLayoutGroup statsLayout = statsRow.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 16f;
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.childControlHeight = true;
            statsLayout.childControlWidth = true;
            statsLayout.childForceExpandHeight = false;
            statsLayout.childForceExpandWidth = true;
            ContentSizeFitter statsFit = statsRow.AddComponent<ContentSizeFitter>();
            statsFit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            statsFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _homeBestTile = _ui.CreateStatTile(statsRow.transform, "Melhor", "0/0");
            _homePlayedTile = _ui.CreateStatTile(statsRow.transform, "Partidas", "0");
            _homeFormatTile = _ui.CreateStatTile(statsRow.transform, "Rodada", "0+0");

            GameObject modeCard = _ui.CreateCard("ModeCard", column);
            RectTransform modeContent = _ui.CreateVerticalContent(modeCard, new Vector2(28f, 28f), 16f, TextAnchor.UpperLeft);
            _ui.CreateChip(modeContent, "Modo de jogo");
            _homeModeLabel = _ui.CreateText("ModeLabel", modeContent, _theme.bodySize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            Text modeNote = _ui.CreateText("ModeNote", modeContent, _theme.smallSize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkSecondary);
            modeNote.text = "No modo padrão você recebe feedback logo após cada resposta. No difícil, ele aparece só no fim de cada fase.";
            IdentificaveisStyledButton modeButton = _ui.CreateButton(modeContent.transform, "Alternar modo", IdentificaveisButtonStyle.Secondary);
            modeButton.button.onClick.AddListener(ToggleHardMode);

            GameObject actionStack = new GameObject("HomeActions", typeof(RectTransform));
            actionStack.transform.SetParent(column, false);
            VerticalLayoutGroup actionLayout = actionStack.AddComponent<VerticalLayoutGroup>();
            actionLayout.spacing = 14f;
            actionLayout.childAlignment = TextAnchor.UpperCenter;
            actionLayout.childControlHeight = true;
            actionLayout.childControlWidth = true;
            actionLayout.childForceExpandHeight = false;
            actionLayout.childForceExpandWidth = true;
            ContentSizeFitter actionFit = actionStack.AddComponent<ContentSizeFitter>();
            actionFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            IdentificaveisStyledButton startButton = _ui.CreateButton(actionStack.transform, "Começar partida", IdentificaveisButtonStyle.Primary);
            startButton.button.onClick.AddListener(StartSession);
            IdentificaveisStyledButton tutorialButton = _ui.CreateButton(actionStack.transform, "Como jogar", IdentificaveisButtonStyle.Secondary);
            tutorialButton.button.onClick.AddListener(OpenTutorialFromHome);

            return screen;
        }

        private GameObject BuildMessageScreen(Transform parent)
        {
            GameObject screen = _ui.CreateScreen("MessageScreen", parent);
            RectTransform column = _ui.CreateSafeColumn("SafeArea", screen.transform, new Vector2(46f, 46f), 24f);

            GameObject hero = _ui.CreateShellCard("MessageShell", column);
            RectTransform heroContent = _ui.CreateVerticalContent(hero, new Vector2(26f, 26f), 10f, TextAnchor.UpperLeft);
            _ui.CreateChip(heroContent, "Leitura guiada");
            _messageTitle = _ui.CreateText("MessageTitle", heroContent, _theme.titleSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            _messageTitle.text = "Mensagem";

            GameObject bodyCard = _ui.CreateCard("MessageCard", column);
            LayoutElement bodyLayout = bodyCard.GetComponent<LayoutElement>();
            if (bodyLayout == null)
            {
                bodyLayout = bodyCard.AddComponent<LayoutElement>();
            }

            bodyLayout.minHeight = 920f;
            bodyLayout.flexibleHeight = 1f;

            RectTransform bodyContent = _ui.CreateVerticalContent(bodyCard, new Vector2(34f, 34f), 18f, TextAnchor.UpperLeft);
            _messageBody = _ui.CreateText("MessageBody", bodyContent, _theme.bodySize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkSecondary);
            _messageBody.text = string.Empty;
            _messageBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _messageBody.verticalOverflow = VerticalWrapMode.Overflow;
            LayoutElement messageBodyLayout = _messageBody.GetComponent<LayoutElement>();
            if (messageBodyLayout != null)
            {
                messageBodyLayout.flexibleHeight = 1f;
            }

            GameObject actions = new GameObject("Actions", typeof(RectTransform));
            actions.transform.SetParent(column, false);
            HorizontalLayoutGroup layout = actions.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = actions.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _messagePrimaryButton = _ui.CreateButton(actions.transform, "Continuar", IdentificaveisButtonStyle.Primary);
            _messagePrimaryButton.button.onClick.AddListener(OnMessagePrimaryButtonPressed);
            _messageSecondaryButton = _ui.CreateButton(actions.transform, "Voltar", IdentificaveisButtonStyle.Secondary);
            _messageSecondaryButton.button.onClick.AddListener(OnMessageSecondaryButtonPressed);
            return screen;
        }

        private GameObject BuildGameplayScreen(Transform parent)
        {
            GameObject screen = _ui.CreateScreen("GameplayScreen", parent);
            RectTransform column = _ui.CreateSafeColumn("SafeArea", screen.transform, new Vector2(38f, 38f), 20f);

            GameObject masthead = _ui.CreateShellCard("Masthead", column);
            RectTransform mastheadContent = _ui.CreateVerticalContent(masthead, new Vector2(24f, 24f), 14f, TextAnchor.UpperLeft);
            _ui.CreateChip(mastheadContent, "Identificáveis");
            _phaseLabel = _ui.CreateText("PhaseLabel", mastheadContent, _theme.smallSize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.accent);
            _phaseLabel.text = "Fase";
            _gameTitle = _ui.CreateText("GameTitle", mastheadContent, 38, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            _gameSubtitle = _ui.CreateText("GameSubtitle", mastheadContent, _theme.bodySize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkSecondary);
            _progress = _ui.CreateProgressBar(mastheadContent);

            GameObject contentCard = _ui.CreateCard("ContentCard", column);
            RectTransform content = _ui.CreateVerticalContent(contentCard, new Vector2(28f, 28f), 18f, TextAnchor.UpperLeft);
            _contentHeading = _ui.CreateText("ContentHeading", content, _theme.bodySize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            _contentHeading.text = string.Empty;
            _contentBody = _ui.CreateText("ContentBody", content, _theme.bodySize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkPrimary);
            _contentBody.text = string.Empty;
            _readingChip = _ui.CreateChip(content, "Leia antes de decidir");
            _readingChipLabel = _readingChip.GetComponentInChildren<Text>(true);

            _feedbackCard = _ui.CreateCard("FeedbackCard", column, true);
            _feedbackCard.SetActive(false);
            RectTransform feedbackContent = _ui.CreateVerticalContent(_feedbackCard, new Vector2(22f, 22f), 10f, TextAnchor.UpperLeft);
            _feedbackTitle = _ui.CreateText("FeedbackTitle", feedbackContent, _theme.bodySize, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkInverted);
            _feedbackBody = _ui.CreateText("FeedbackBody", feedbackContent, _theme.smallSize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkInverted);

            GameObject options = new GameObject("Options", typeof(RectTransform));
            options.transform.SetParent(column, false);
            VerticalLayoutGroup optionsLayout = options.AddComponent<VerticalLayoutGroup>();
            optionsLayout.spacing = 12f;
            optionsLayout.childAlignment = TextAnchor.UpperCenter;
            optionsLayout.childControlHeight = true;
            optionsLayout.childControlWidth = true;
            optionsLayout.childForceExpandHeight = false;
            optionsLayout.childForceExpandWidth = true;
            ContentSizeFitter optionsFit = options.AddComponent<ContentSizeFitter>();
            optionsFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < _optionButtons.Length; i++)
            {
                int captured = i;
                _optionButtons[i] = _ui.CreateButton(options.transform, "Opção", IdentificaveisButtonStyle.Choice);
                if (_optionButtons[i].badge != null)
                {
                    _optionButtons[i].badge.text = ((char)('A' + i)).ToString();
                }

                _optionButtons[i].button.onClick.AddListener(() => OnOptionPressed(captured));
            }

            GameObject footer = new GameObject("Footer", typeof(RectTransform));
            footer.transform.SetParent(column, false);
            HorizontalLayoutGroup footerLayout = footer.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 14f;
            footerLayout.childAlignment = TextAnchor.MiddleCenter;
            footerLayout.childControlHeight = true;
            footerLayout.childControlWidth = true;
            footerLayout.childForceExpandHeight = false;
            footerLayout.childForceExpandWidth = true;
            ContentSizeFitter footerFit = footer.AddComponent<ContentSizeFitter>();
            footerFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _leaveButton = _ui.CreateButton(footer.transform, "Voltar ao início", IdentificaveisButtonStyle.Secondary);
            _leaveButton.button.onClick.AddListener(ShowHome);
            _nextButton = _ui.CreateButton(footer.transform, "Próximo", IdentificaveisButtonStyle.Primary);
            _nextButton.button.onClick.AddListener(AdvanceAfterFeedback);
            _nextButton.button.gameObject.SetActive(false);

            return screen;
        }

        private GameObject BuildResultsScreen(Transform parent)
        {
            GameObject screen = _ui.CreateScreen("ResultsScreen", parent);
            RectTransform column = _ui.CreateSafeColumn("SafeArea", screen.transform, new Vector2(40f, 40f), 22f);

            GameObject hero = _ui.CreateShellCard("ResultsHero", column);
            RectTransform heroContent = _ui.CreateVerticalContent(hero, new Vector2(28f, 28f), 14f, TextAnchor.UpperLeft);
            _ui.CreateChip(heroContent, "Leitura final");
            _resultsHeadline = _ui.CreateText("ResultsHeadline", heroContent, 42, FontStyle.Bold, TextAnchor.UpperLeft, _theme.inkPrimary);
            _resultsHeadline.text = "Resultados";

            GameObject stats = new GameObject("ResultsStats", typeof(RectTransform));
            stats.transform.SetParent(column, false);
            HorizontalLayoutGroup statsLayout = stats.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 14f;
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.childControlHeight = true;
            statsLayout.childControlWidth = true;
            statsLayout.childForceExpandHeight = false;
            statsLayout.childForceExpandWidth = true;
            ContentSizeFitter statsFit = stats.AddComponent<ContentSizeFitter>();
            statsFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _resultsProfileTile = _ui.CreateStatTile(stats.transform, "Fase 1", "0/0");
            _resultsScenarioTile = _ui.CreateStatTile(stats.transform, "Fases 2 a 5", "0/0");
            _resultsTotalTile = _ui.CreateStatTile(stats.transform, "Total", "0/0");

            RectTransform resultsContent = _ui.CreateScrollCard("ResultsCard", column, 920f);
            _resultsBody = _ui.CreateText("ResultsBody", resultsContent, _theme.bodySize, FontStyle.Normal, TextAnchor.UpperLeft, _theme.inkSecondary);
            _resultsBody.text = string.Empty;

            GameObject actions = new GameObject("ResultsActions", typeof(RectTransform));
            actions.transform.SetParent(column, false);
            VerticalLayoutGroup actionsLayout = actions.AddComponent<VerticalLayoutGroup>();
            actionsLayout.spacing = 12f;
            actionsLayout.childAlignment = TextAnchor.UpperCenter;
            actionsLayout.childControlHeight = true;
            actionsLayout.childControlWidth = true;
            actionsLayout.childForceExpandHeight = false;
            actionsLayout.childForceExpandWidth = true;
            ContentSizeFitter actionsFit = actions.AddComponent<ContentSizeFitter>();
            actionsFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _resultsPrimaryButton = _ui.CreateButton(actions.transform, "Jogar novamente", IdentificaveisButtonStyle.Primary);
            _resultsPrimaryButton.button.onClick.AddListener(StartSession);
            _resultsSecondaryButton = _ui.CreateButton(actions.transform, "Ler tutorial novamente", IdentificaveisButtonStyle.Secondary);
            _resultsSecondaryButton.button.onClick.AddListener(OpenTutorialFromHome);
            _resultsTertiaryButton = _ui.CreateButton(actions.transform, "Voltar ao início", IdentificaveisButtonStyle.Ghost);
            _resultsTertiaryButton.button.onClick.AddListener(ShowHome);

            return screen;
        }

        private void StartSession()
        {
            _session = IdentificaveisMatchBuilder.Build(_database, _preferences);
            _analysis = null;
            ShowMessage(
                "Antes de começar",
                "O jogo menciona demissão, humilhação pública, hospitalização, reprovação, solidão, ruptura afetiva e constrangimento digital.\n\n" +
                "A partida agora atravessa cinco fases curtas:\n" +
                "• Fase 1: decidir se um perfil parece humano ou algorítmico.\n" +
                "• Fase 2: escolher qual reação pública soa mais humana.\n" +
                "• Fase 3: reconhecer comentários que ainda parecem gente de verdade.\n" +
                "• Fase 4: ler mensagens privadas sem cair em texto bonito demais.\n" +
                "• Fase 5: distinguir justificativas humanas de respostas fabricadas.\n\n" +
                "A meta não é só acertar. É perceber o que continua humano quando o formato muda.",
                "Continuar",
                OpenTutorialAfterWarning,
                "Pular direto para a Fase 1",
                BeginProfiles);
        }

        private void OpenTutorialAfterWarning()
        {
            OpenTutorial(BeginProfiles, "Entrar na Fase 1", "Pular texto");
        }

        private void OpenTutorialFromHome()
        {
            OpenTutorial(ShowHome, "Voltar ao início", null);
        }

        private void OpenTutorial(Action primaryAction, string primaryLabel, string secondaryLabel)
        {
            string body =
                "Na Fase 1, você observa bio e publicação. A resposta certa nem sempre é a mais bagunçada; às vezes o algoritmo imita vulnerabilidade bem demais.\n\n" +
                "Nas Fases 2 a 5, o formato muda: reação pública, comentário, mensagem privada e justificativa. A pista não é elegância. É presença humana sob pressão: detalhe específico, ritmo estranho, ambivalência, humor defensivo, excesso, corte ou limite.\n\n" +
                "No modo padrão você recebe feedback após cada resposta. No modo difícil, o jogo segura esse retorno até o fim de cada fase.";

            ShowMessage(
                "Como jogar",
                body,
                primaryLabel,
                primaryAction,
                string.IsNullOrEmpty(secondaryLabel) ? null : secondaryLabel,
                string.IsNullOrEmpty(secondaryLabel) ? null : BeginProfiles);
        }

        private void OpenCredits()
        {
            ShowMessage(
                "Créditos e direção",
                "Conceito, GDD e direção autoral: Emli O'Mago.\n\n" +
                "Esta versão reorganiza a base em uma linguagem mais editorial e social: hierarquia estável, blocos de leitura, estado visual consistente e componentes reutilizáveis pensados para virar prefabs editáveis no projeto.\n\n" +
                "A lógica principal continua separada do conteúdo. Perfis e cenários permanecem no JSON, enquanto a camada visual foi preparada para variações futuras de arte e layout.",
                "Voltar ao início",
                ShowHome,
                null,
                null);
        }

        private void BeginProfiles()
        {
            if (_session == null)
            {
                _session = IdentificaveisMatchBuilder.Build(_database, _preferences);
            }

            _session.phase = PhaseType.Profiles;
            _session.phaseIndex = 0;
            _session.waitingForAdvance = false;
            RenderCurrentStep();
        }

        private void BeginReactions()
        {
            if (_session == null)
            {
                ShowHome();
                return;
            }

            _session.phase = PhaseType.Reactions;
            _session.phaseIndex = 0;
            _session.waitingForAdvance = false;
            RenderCurrentStep();
        }

        private void BeginComments()
        {
            if (_session == null)
            {
                ShowHome();
                return;
            }

            _session.phase = PhaseType.Comments;
            _session.phaseIndex = 0;
            _session.waitingForAdvance = false;
            RenderCurrentStep();
        }

        private void BeginMessages()
        {
            if (_session == null)
            {
                ShowHome();
                return;
            }

            _session.phase = PhaseType.Messages;
            _session.phaseIndex = 0;
            _session.waitingForAdvance = false;
            RenderCurrentStep();
        }

        private void BeginJustifications()
        {
            if (_session == null)
            {
                ShowHome();
                return;
            }

            _session.phase = PhaseType.Justifications;
            _session.phaseIndex = 0;
            _session.waitingForAdvance = false;
            RenderCurrentStep();
        }

        private void RenderCurrentStep()
        {
            if (_session == null)
            {
                ShowHome();
                return;
            }

            if (_session.phase == PhaseType.Profiles)
            {
                if (_session.phaseIndex >= _session.activeProfiles.Count)
                {
                    OpenProfileSummary();
                    return;
                }

                ShowGameplay();
                RenderProfile(_session.activeProfiles[_session.phaseIndex]);
                return;
            }

            if (IsPromptPhase(_session.phase))
            {
                System.Collections.Generic.List<ScenarioContentData> prompts = GetPromptList(_session.phase);
                if (_session.phaseIndex >= prompts.Count)
                {
                    AdvanceToNextPhase();
                    return;
                }

                ShowGameplay();
                RenderPrompt(_session.phase, prompts[_session.phaseIndex]);
                return;
            }

            if (_session.phase == PhaseType.Results)
            {
                ShowResults();
            }
        }

        private void RenderProfile(ProfileContentData profile)
        {
            _phaseLabel.text = "Fase 1 — Quem parece real?";
            _gameTitle.text = profile.username;
            _gameSubtitle.text = "Leia a bio e a publicação antes de decidir.";
            SetProgress(PhaseType.Profiles, _session.phaseIndex + 1, _session.activeProfiles.Count);

            _contentHeading.text = "Perfil";
            _readingChip.SetActive(true);
            _readingChipLabel.text = "Observe bio e publicação antes de escolher.";
            _contentBody.text = "Bio\n" + profile.bio + "\n\nPublicação\n\u201c" + profile.post + "\u201d";

            ConfigureOptionButton(0, "Parece humano", true, "H");
            ConfigureOptionButton(1, "Parece algoritmo", true, "IA");
            ConfigureOptionButton(2, string.Empty, false, string.Empty);
            ConfigureOptionButton(3, string.Empty, false, string.Empty);

            HideFeedback();
            _nextButton.button.gameObject.SetActive(false);
            _nextButton.label.text = "Próximo";
            SetOptionInteractable(true);
            _session.waitingForAdvance = false;
        }

        private void RenderPrompt(PhaseType phase, ScenarioContentData prompt)
        {
            _phaseLabel.text = GetPhaseLabel(phase);
            _gameTitle.text = prompt.title;
            _gameSubtitle.text = prompt.person;
            SetProgress(phase, _session.phaseIndex + 1, GetPromptList(phase).Count);

            _contentHeading.text = GetContentHeading(phase);
            _readingChip.SetActive(true);
            _readingChipLabel.text = GetReadingChipText(phase);
            _contentBody.text = prompt.eventText;

            System.Array.Clear(_displayedScenarioChoices, 0, _displayedScenarioChoices.Length);
            var options = new System.Collections.Generic.List<ScenarioChoiceData>();
            if (prompt.choices != null)
            {
                for (int i = 0; i < prompt.choices.Count; i++)
                {
                    if (prompt.choices[i] != null)
                    {
                        options.Add(prompt.choices[i]);
                    }
                }
            }

            IdentificaveisMatchBuilder.Shuffle(options);

            for (int i = 0; i < _optionButtons.Length; i++)
            {
                bool active = i < options.Count;
                _displayedScenarioChoices[i] = active ? options[i] : null;
                string badge = ((char)('A' + i)).ToString();
                ConfigureOptionButton(i, active ? options[i].text : string.Empty, active, badge);
            }

            HideFeedback();
            _nextButton.button.gameObject.SetActive(false);
            _nextButton.label.text = GetAdvanceLabel(phase);
            SetOptionInteractable(true);
            _session.waitingForAdvance = false;
        }

        private void OnOptionPressed(int optionIndex)
        {
            if (_session == null || _session.waitingForAdvance)
            {
                return;
            }

            if (_session.phase == PhaseType.Profiles)
            {
                HandleProfileAnswer(optionIndex);
                return;
            }

            if (IsPromptPhase(_session.phase))
            {
                HandlePromptAnswer(optionIndex);
            }
        }

        private void HandleProfileAnswer(int optionIndex)
        {
            ProfileContentData profile = _session.activeProfiles[_session.phaseIndex];
            string playerChoice = optionIndex == 0 ? "humano" : "algoritmo";
            bool correct = string.Equals(profile.correctType, playerChoice, StringComparison.OrdinalIgnoreCase);
            string feedback = BuildProfileFeedback(profile, correct);

            _session.answers.Add(new SessionAnswerRecord
            {
                phase = PhaseType.Profiles,
                itemId = profile.id,
                itemIndex = _session.phaseIndex,
                promptTitle = profile.username,
                playerChoice = playerChoice,
                correctChoice = profile.correctType,
                wasCorrect = correct,
                feedback = feedback,
                patternKey = correct ? string.Empty : profile.baitPattern
            });

            if (_session.immediateFeedback)
            {
                ShowFeedback(correct ? "✓ Leitura registrada" : "✕ Armadilha reconhecida", feedback, correct);
                _nextButton.label.text = _session.phaseIndex >= _session.activeProfiles.Count - 1 ? "Ver resumo da fase" : "Próximo perfil";
                _nextButton.button.gameObject.SetActive(true);
                _session.waitingForAdvance = true;
                SetOptionInteractable(false);
            }
            else
            {
                _session.phaseIndex++;
                RenderCurrentStep();
            }
        }

        private void HandlePromptAnswer(int optionIndex)
        {
            System.Collections.Generic.List<ScenarioContentData> prompts = GetPromptList(_session.phase);
            ScenarioContentData prompt = prompts[_session.phaseIndex];
            ScenarioChoiceData choice = optionIndex >= 0 && optionIndex < _displayedScenarioChoices.Length ? _displayedScenarioChoices[optionIndex] : null;
            ScenarioChoiceData humanChoice = prompt.GetHumanChoice();
            bool correct = choice != null && choice.isHuman;
            string feedback = BuildScenarioFeedback(prompt, choice, humanChoice, correct);

            _session.answers.Add(new SessionAnswerRecord
            {
                phase = _session.phase,
                itemId = prompt.id,
                itemIndex = _session.phaseIndex,
                promptTitle = prompt.title,
                playerChoice = choice != null ? choice.id : string.Empty,
                correctChoice = humanChoice != null ? humanChoice.id : string.Empty,
                wasCorrect = correct,
                feedback = feedback,
                patternKey = correct || choice == null ? string.Empty : choice.baitPattern
            });

            if (_session.immediateFeedback)
            {
                ShowFeedback(correct ? "✓ Leitura registrada" : "✕ Armadilha reconhecida", feedback, correct);
                _nextButton.label.text = GetAdvanceLabel(_session.phase);
                _nextButton.button.gameObject.SetActive(true);
                _session.waitingForAdvance = true;
                SetOptionInteractable(false);
            }
            else
            {
                _session.phaseIndex++;
                RenderCurrentStep();
            }
        }

        private void AdvanceAfterFeedback()
        {
            if (_session == null)
            {
                return;
            }

            if (_session.phase == PhaseType.Profiles || IsPromptPhase(_session.phase))
            {
                _session.phaseIndex++;
                RenderCurrentStep();
            }
        }

        private void OpenProfileSummary()
        {
            _session.phase = PhaseType.ProfileSummary;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Você concluiu a primeira leitura: aparência, coerência e estilo.");
            builder.AppendLine();
            builder.AppendLine("Acertos: " + _session.ProfileHits + " / " + _session.activeProfiles.Count + ".");
            builder.AppendLine();

            if (_session.immediateFeedback)
            {
                builder.AppendLine("Agora o jogo sai da apresentação e passa por quatro formatos de resposta: reação pública, comentário, mensagem privada e justificativa. A pergunta continua a mesma, mas o jeito de errar muda bastante.");
            }
            else
            {
                builder.AppendLine("Como você está no modo difícil, os tropeços ficam mais visíveis aqui:");
                AppendPhaseMistakes(builder, PhaseType.Profiles);
            }

            ShowMessage(
                "Resumo da Fase 1",
                builder.ToString(),
                "Seguir para as próximas fases",
                OpenTransition,
                "Voltar ao início",
                ShowHome);
        }

        private void OpenTransition()
        {
            ShowMessage(
                "Da aparência para a reação",
                "Na Fase 1 você leu superfície, composição e sinais de estabilidade.\n\n" +
                "Agora a experiência entra onde a humanidade costuma perder acabamento: reação pública, comentário atravessado, mensagem privada enviada na pressa e justificativa que tenta se explicar sem soar ensaiada.\n\n" +
                "A resposta mais humana quase nunca é a mais bonita. Ela tende a carregar contexto, atrito e uma dose de falha.",
                "Entrar na Fase 2",
                BeginReactions,
                "Voltar ao início",
                ShowHome);
        }

        private void OpenResults()
        {
            _session.phase = PhaseType.Results;
            _analysis = IdentificaveisResultAnalyzer.Analyze(_session);
            IdentificaveisPreferencesStore.RegisterMatchResult(_session.TotalHits);
            ShowResults();
        }

        private void ShowHome()
        {
            _session = null;
            RefreshHome();
            CrossfadeTo(_homeScreen);
        }

        private void ShowGameplay()
        {
            CrossfadeTo(_gameplayScreen);
        }

        private void ShowResults()
        {
            RefreshResults();
            CrossfadeTo(_resultsScreen);
        }

        private void ShowMessage(string title, string body, string primaryLabel, Action primaryAction, string secondaryLabel, Action secondaryAction)
        {
            _messageTitle.text = title;
            _messageBody.text = body;
            _messagePrimaryAction = primaryAction;
            _messageSecondaryAction = secondaryAction;
            _messagePrimaryButton.label.text = primaryLabel;
            bool hasSecondary = !string.IsNullOrEmpty(secondaryLabel) && secondaryAction != null;
            _messageSecondaryButton.button.gameObject.SetActive(hasSecondary);
            if (hasSecondary)
            {
                _messageSecondaryButton.label.text = secondaryLabel;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_messageScreen.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_messageBody.rectTransform);
            CrossfadeTo(_messageScreen);
        }

        private void OnMessagePrimaryButtonPressed()
        {
            Action action = _messagePrimaryAction;
            if (action != null)
            {
                action.Invoke();
            }
        }

        private void OnMessageSecondaryButtonPressed()
        {
            Action action = _messageSecondaryAction;
            if (action != null)
            {
                action.Invoke();
            }
        }

        private void RefreshHome()
        {
            int maxScore = _database.profilesPerRun + _database.reactionsPerRun + _database.commentsPerRun + _database.messagesPerRun + _database.justificationsPerRun;
            _homeTitle.text = string.IsNullOrEmpty(_database.displayName) ? "Identificáveis" : _database.displayName;
            _homeSubtitle.text = string.IsNullOrEmpty(_database.subtitle)
                ? "Descubra como confundimos sinais de humanidade com sinais de otimização."
                : _database.subtitle;
            _homeRoundLabel.text = BuildMenuRoundText();
            _homeBestTile.value.text = IdentificaveisPreferencesStore.LoadBestScore() + "/" + maxScore;
            _homePlayedTile.value.text = IdentificaveisPreferencesStore.LoadPlayedCount().ToString();
            _homeFormatTile.value.text = maxScore + " leituras";
            RefreshMenuLabels();
        }

        private void RefreshResults()
        {
            if (_session == null || _analysis == null)
            {
                return;
            }

            int promptTotal = _session.activeReactions.Count + _session.activeComments.Count + _session.activeMessages.Count + _session.activeJustifications.Count;

            _resultsHeadline.text = _analysis.headline;
            _resultsProfileTile.value.text = _session.ProfileHits + "/" + _session.activeProfiles.Count;
            _resultsScenarioTile.value.text = _session.PromptHits + "/" + promptTotal;
            _resultsTotalTile.value.text = _session.TotalHits + "/" + _session.TotalQuestions;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(IdentificaveisResultAnalyzer.BuildStatsLine(_session));
            builder.AppendLine();
            builder.AppendLine(_analysis.body);

            if (_analysis.dominantPatterns.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Padrões dominantes nos seus erros:");
                for (int i = 0; i < _analysis.dominantPatterns.Count; i++)
                {
                    builder.AppendLine("• " + _analysis.dominantPatterns[i]);
                }
            }

            if (_analysis.commentedMistakes.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Erros comentados:");
                for (int i = 0; i < _analysis.commentedMistakes.Count; i++)
                {
                    builder.AppendLine("• " + _analysis.commentedMistakes[i]);
                }
            }

            builder.AppendLine();
            builder.AppendLine(IdentificaveisResultAnalyzer.BuildClosingText());
            _resultsBody.text = builder.ToString();
        }

        private void ShowFeedback(string title, string body, bool correct)
        {
            _feedbackCard.SetActive(true);
            Image image = _feedbackCard.GetComponent<Image>();
            if (image != null)
            {
                image.color = correct ? _theme.success : _theme.error;
            }

            _feedbackTitle.text = title;
            _feedbackBody.text = body;
        }

        private void HideFeedback()
        {
            _feedbackCard.SetActive(false);
            _feedbackTitle.text = string.Empty;
            _feedbackBody.text = string.Empty;
        }

        private void ConfigureOptionButton(int index, string label, bool active, string badge)
        {
            if (index < 0 || index >= _optionButtons.Length)
            {
                return;
            }

            _optionButtons[index].button.gameObject.SetActive(active);
            if (active)
            {
                _optionButtons[index].label.text = label;
                if (_optionButtons[index].badge != null)
                {
                    _optionButtons[index].badge.text = badge;
                }
            }
        }

        private void SetOptionInteractable(bool interactable)
        {
            for (int i = 0; i < _optionButtons.Length; i++)
            {
                if (_optionButtons[i] != null && _optionButtons[i].button != null)
                {
                    _optionButtons[i].button.interactable = interactable;
                }
            }
        }


        private bool IsPromptPhase(PhaseType phase)
        {
            return phase == PhaseType.Reactions
                || phase == PhaseType.Comments
                || phase == PhaseType.Messages
                || phase == PhaseType.Justifications;
        }

        private System.Collections.Generic.List<ScenarioContentData> GetPromptList(PhaseType phase)
        {
            switch (phase)
            {
                case PhaseType.Reactions:
                    return _session.activeReactions;
                case PhaseType.Comments:
                    return _session.activeComments;
                case PhaseType.Messages:
                    return _session.activeMessages;
                case PhaseType.Justifications:
                    return _session.activeJustifications;
                default:
                    return _session.activeReactions;
            }
        }

        private void AdvanceToNextPhase()
        {
            if (_session.phase == PhaseType.Reactions)
            {
                BeginComments();
                return;
            }

            if (_session.phase == PhaseType.Comments)
            {
                BeginMessages();
                return;
            }

            if (_session.phase == PhaseType.Messages)
            {
                BeginJustifications();
                return;
            }

            if (_session.phase == PhaseType.Justifications)
            {
                OpenResults();
            }
        }

        private string GetPhaseLabel(PhaseType phase)
        {
            switch (phase)
            {
                case PhaseType.Reactions:
                    return "Fase 2 — Que reação soa humana?";
                case PhaseType.Comments:
                    return "Fase 3 — Que comentário parece gente?";
                case PhaseType.Messages:
                    return "Fase 4 — O que soaria humano no privado?";
                case PhaseType.Justifications:
                    return "Fase 5 — Que justificativa parece menos fabricada?";
                default:
                    return "Fase";
            }
        }

        private string GetContentHeading(PhaseType phase)
        {
            switch (phase)
            {
                case PhaseType.Comments:
                    return "Publicação";
                case PhaseType.Messages:
                    return "Contexto";
                default:
                    return "Situação";
            }
        }

        private string GetReadingChipText(PhaseType phase)
        {
            switch (phase)
            {
                case PhaseType.Reactions:
                    return "Escolha a reação que parece mais humana.";
                case PhaseType.Comments:
                    return "Escolha o comentário que parece menos fabricado.";
                case PhaseType.Messages:
                    return "Escolha a mensagem que parece escrita por alguém de verdade.";
                case PhaseType.Justifications:
                    return "Escolha a justificativa que parece mais humana.";
                default:
                    return "Leia antes de decidir.";
            }
        }

        private string GetAdvanceLabel(PhaseType phase)
        {
            System.Collections.Generic.List<ScenarioContentData> prompts = GetPromptList(phase);
            bool last = _session.phaseIndex >= prompts.Count - 1;
            switch (phase)
            {
                case PhaseType.Reactions:
                    return last ? "Entrar na Fase 3" : "Próxima situação";
                case PhaseType.Comments:
                    return last ? "Entrar na Fase 4" : "Próximo comentário";
                case PhaseType.Messages:
                    return last ? "Entrar na Fase 5" : "Próxima mensagem";
                case PhaseType.Justifications:
                    return last ? "Ver resultados" : "Próxima justificativa";
                default:
                    return "Próximo";
            }
        }

        private void SetProgress(PhaseType phase, int currentIndex, int total)
        {
            string phaseName;
            switch (phase)
            {
                case PhaseType.Profiles:
                    phaseName = "Fase 1";
                    break;
                case PhaseType.Reactions:
                    phaseName = "Fase 2";
                    break;
                case PhaseType.Comments:
                    phaseName = "Fase 3";
                    break;
                case PhaseType.Messages:
                    phaseName = "Fase 4";
                    break;
                case PhaseType.Justifications:
                    phaseName = "Fase 5";
                    break;
                default:
                    phaseName = "Etapa";
                    break;
            }

            _progress.label.text = phaseName + " • " + currentIndex + " / " + total;
            float amount = total <= 0 ? 0f : Mathf.Clamp01((float)currentIndex / total);
            if (_progress.fill != null)
            {
                RectTransform fillRect = _progress.fill.rectTransform;
                fillRect.anchorMax = new Vector2(amount, 1f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
            }
        }

        private void CrossfadeTo(GameObject screen)
        {
            CanvasGroup target = screen != null ? screen.GetComponent<CanvasGroup>() : null;
            if (target == null)
            {
                return;
            }

            if (_activeScreen == target)
            {
                ActivateScreen(target, true);
                return;
            }

            StopAllCoroutines();
            StartCoroutine(CrossfadeRoutine(target));
        }

        private IEnumerator CrossfadeRoutine(CanvasGroup target)
        {
            CanvasGroup previous = _activeScreen;
            if (previous != null)
            {
                ActivateScreen(previous, true);
            }

            ActivateScreen(target, true);
            target.alpha = 0f;

            float duration = Mathf.Max(0.01f, _theme.transitionDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (previous != null)
                {
                    previous.alpha = 1f - t;
                }

                target.alpha = t;
                yield return null;
            }

            if (previous != null)
            {
                ActivateScreen(previous, false);
            }

            ActivateScreen(target, true);
            target.alpha = 1f;
            _activeScreen = target;
        }

        private void ActivateScreen(CanvasGroup group, bool active)
        {
            if (group == null)
            {
                return;
            }

            group.gameObject.SetActive(active);
            group.interactable = active;
            group.blocksRaycasts = active;
            if (!active)
            {
                group.alpha = 0f;
            }
        }

        private string BuildProfileFeedback(ProfileContentData profile, bool correct)
        {
            string expected = string.Equals(profile.correctType, "humano", StringComparison.OrdinalIgnoreCase) ? "humano" : "algorítmico";
            if (correct)
            {
                return "Leitura correta. " + profile.signalKey;
            }

            return "A leitura mais adequada aqui era " + expected + ". " + profile.signalKey;
        }

        private string BuildScenarioFeedback(ScenarioContentData scenario, ScenarioChoiceData picked, ScenarioChoiceData humanChoice, bool correct)
        {
            if (correct)
            {
                return scenario.reading;
            }

            string target = humanChoice != null ? humanChoice.text : "resposta humana não definida";
            string selected = picked != null ? picked.text : "opção inválida";
            string noun = "resposta";
            if (scenario != null)
            {
                switch (scenario.phaseKey)
                {
                    case "reacao":
                        noun = "reação";
                        break;
                    case "comentario":
                        noun = "comentário";
                        break;
                    case "mensagem":
                        noun = "mensagem";
                        break;
                    case "justificativa":
                        noun = "justificativa";
                        break;
                }
            }

            return "A " + noun + " mais humana aqui era: “" + target + "”. Você escolheu: “" + selected + "”. " + scenario.reading;
        }

        private void AppendPhaseMistakes(StringBuilder builder, PhaseType phase)
        {
            int shown = 0;
            for (int i = 0; i < _session.answers.Count; i++)
            {
                SessionAnswerRecord answer = _session.answers[i];
                if (answer.phase != phase || answer.wasCorrect)
                {
                    continue;
                }

                builder.AppendLine("• " + answer.promptTitle + " — " + answer.feedback);
                shown++;
                if (shown >= 2)
                {
                    break;
                }
            }

            if (shown == 0)
            {
                builder.AppendLine("• Nenhum erro nesta fase.");
            }
        }

        private string BuildMenuRoundText()
        {
            int total = _database.profilesPerRun + _database.reactionsPerRun + _database.commentsPerRun + _database.messagesPerRun + _database.justificationsPerRun;
            return "Cada rodada atravessa 5 fases e " + total + " leituras. Uma partida costuma durar de 5 a 8 minutos.";
        }

        private void RefreshMenuLabels()
        {
            if (_homeModeLabel != null)
            {
                _homeModeLabel.text = "Modo: " + (_preferences.hardMode ? "Difícil" : "Padrão");
            }

            if (_homeContrastLabel != null)
            {
                _homeContrastLabel.gameObject.SetActive(false);
            }

            if (_homeTextSizeLabel != null)
            {
                _homeTextSizeLabel.gameObject.SetActive(false);
            }
        }

        private void ToggleHardMode()
        {
            _preferences.hardMode = !_preferences.hardMode;
            IdentificaveisPreferencesStore.SavePreferences(_preferences);
            RefreshMenuLabels();
        }

        private void ToggleContrast()
        {
            _preferences.highContrast = !_preferences.highContrast;
            IdentificaveisPreferencesStore.SavePreferences(_preferences);
            ApplyHighContrastPalette();
            RebuildForPreferenceChange();
        }

        private void ToggleTextSize()
        {
            _preferences.largeText = !_preferences.largeText;
            IdentificaveisPreferencesStore.SavePreferences(_preferences);
            RebuildForPreferenceChange();
        }

        private void ApplyHighContrastPalette()
        {
            if (!_preferences.highContrast)
            {
                return;
            }

            _theme.backgroundTop = new Color(0.05f, 0.06f, 0.10f, 1f);
            _theme.backgroundBottom = new Color(0.08f, 0.10f, 0.14f, 1f);
            _theme.shellOverlay = new Color(0.11f, 0.14f, 0.20f, 0.96f);
            _theme.surfacePrimary = new Color(0.12f, 0.15f, 0.22f, 1f);
            _theme.surfaceSecondary = new Color(0.16f, 0.19f, 0.27f, 1f);
            _theme.surfaceTertiary = new Color(0.17f, 0.26f, 0.42f, 1f);
            _theme.inkPrimary = Color.white;
            _theme.inkSecondary = new Color(0.88f, 0.92f, 0.97f, 1f);
            _theme.inkInverted = Color.white;
            _theme.accent = new Color(0.50f, 0.74f, 1f, 1f);
            _theme.accentSoft = new Color(0.20f, 0.26f, 0.38f, 1f);
            _theme.outline = new Color(0.36f, 0.44f, 0.58f, 1f);
            _theme.shadow = new Color(0f, 0f, 0f, 0.32f);
        }

        private void RebuildForPreferenceChange()
        {
            if (_canvas != null)
            {
                Destroy(_canvas.gameObject);
            }

            _theme = IdentificaveisThemeAsset.CreateRuntime(_themePreset);
            if (_preferences.highContrast)
            {
                ApplyHighContrastPalette();
            }

            _ui = new IdentificaveisUiFactory(_theme, _font, TextScale);
            _activeScreen = null;
            BuildUi();
            ShowHome();
        }

        private string BuildMonogram(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return "?";
            }

            string cleaned = source.Replace("@", string.Empty).Trim();
            if (cleaned.Length == 1)
            {
                return cleaned.Substring(0, 1).ToUpperInvariant();
            }

            string[] parts = cleaned.Split(new[] { ' ', '_', '-', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpperInvariant();
            }

            return cleaned.Substring(0, Mathf.Min(2, cleaned.Length)).ToUpperInvariant();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystemGo);
        }

        private Font LoadFont()
        {
            try
            {
                Font builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (builtin != null)
                {
                    return builtin;
                }
            }
            catch
            {
            }

            try
            {
                return Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Liberation Sans", "Helvetica", "Tahoma", "DejaVu Sans" }, 16);
            }
            catch
            {
                return Font.CreateDynamicFontFromOSFont("Arial", 16);
            }
        }
    }
}
