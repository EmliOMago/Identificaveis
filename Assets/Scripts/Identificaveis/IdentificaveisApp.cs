using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Identificaveis
{
    public sealed class IdentificaveisApp : MonoBehaviour
    {
        private IdentificaveisContentDatabase _database;
        private AppPreferences _preferences;
        private SessionState _session;
        private SessionAnalysis _analysis;

        private Canvas _canvas;
        private Font _font;

        private GameObject _homeScreen;
        private GameObject _messageScreen;
        private GameObject _gameplayScreen;
        private GameObject _resultsScreen;

        private Text _homeTitle;
        private Text _homeSubtitle;
        private Text _homeStats;
        private Text _homeModeLabel;
        private Text _homeContrastLabel;
        private Text _homeTextSizeLabel;

        private Text _messageTitle;
        private Text _messageBody;
        private Text _messagePrimaryLabel;
        private Text _messageSecondaryLabel;
        private Button _messagePrimaryButton;
        private Button _messageSecondaryButton;

        private Text _phaseLabel;
        private Text _progressLabel;
        private Text _cardTitle;
        private Text _cardSubtitle;
        private Text _cardBody;
        private Text _feedbackLabel;
        private Button[] _optionButtons = new Button[4];
        private Text[] _optionLabels = new Text[4];
        private Button _nextButton;
        private Text _nextButtonLabel;

        private Text _resultsTitle;
        private Text _resultsBody;
        private Text _resultsPrimaryLabel;
        private Button _resultsPrimaryButton;
        private Text _resultsSecondaryLabel;
        private Button _resultsSecondaryButton;

        private Action _messagePrimaryAction;
        private Action _messageSecondaryAction;

        private Color ThemeBackground => _preferences != null && _preferences.highContrast ? new Color(0.05f, 0.05f, 0.08f) : new Color(0.95f, 0.97f, 0.99f);
        private Color ThemePanel => _preferences != null && _preferences.highContrast ? new Color(0.11f, 0.12f, 0.17f) : Color.white;
        private Color ThemeTextPrimary => _preferences != null && _preferences.highContrast ? Color.white : new Color(0.10f, 0.16f, 0.26f);
        private Color ThemeTextSecondary => _preferences != null && _preferences.highContrast ? new Color(0.86f, 0.90f, 0.97f) : new Color(0.29f, 0.34f, 0.43f);
        private Color ThemeAccent => _preferences != null && _preferences.highContrast ? new Color(0.37f, 0.69f, 1.00f) : new Color(0.16f, 0.37f, 0.66f);
        private Color ThemeSuccess => new Color(0.15f, 0.62f, 0.34f);
        private Color ThemeError => new Color(0.78f, 0.22f, 0.22f);
        private Color ThemeMuted => _preferences != null && _preferences.highContrast ? new Color(0.18f, 0.21f, 0.28f) : new Color(0.89f, 0.91f, 0.95f);

        private float TextScale => _preferences != null && _preferences.largeText ? 1.15f : 1.0f;

        private void Awake()
        {
            _database = IdentificaveisContentRepository.Load();
            _preferences = IdentificaveisPreferencesStore.LoadPreferences();
            _font = LoadFont();
            EnsureEventSystem();
            BuildUi();
            ShowHome();
        }

        private void BuildUi()
        {
            GameObject canvasGo = new GameObject("IdentificaveisCanvas");
            canvasGo.transform.SetParent(transform, false);

            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.6f;

            Image background = canvasGo.AddComponent<Image>();
            background.color = ThemeBackground;

            RectTransform root = canvasGo.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            _homeScreen = BuildHomeScreen(root);
            _messageScreen = BuildMessageScreen(root);
            _gameplayScreen = BuildGameplayScreen(root);
            _resultsScreen = BuildResultsScreen(root);
        }

        private GameObject BuildHomeScreen(Transform parent)
        {
            GameObject panel = CreateStretchPanel("HomeScreen", parent, ThemeBackground);
            RectTransform content = CreateVerticalLayout(panel.transform, new Vector2(48, 72), 28, TextAnchor.UpperCenter);
            Stretch(content, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(40, 40), new Vector2(-40, -40));

            _homeTitle = CreateText("Title", content, 68, FontStyle.Bold, TextAnchor.MiddleCenter, ThemeTextPrimary);
            _homeTitle.text = string.IsNullOrEmpty(_database.displayName) ? "Identificáveis" : _database.displayName;

            _homeSubtitle = CreateText("Subtitle", content, 30, FontStyle.Normal, TextAnchor.MiddleCenter, ThemeTextSecondary);
            _homeSubtitle.text = "Descubra como confundimos sinais de humanidade com sinais de otimização.";

            GameObject card = CreateCard("HomeCard", content, ThemePanel);
            RectTransform cardLayout = CreateVerticalLayout(card.transform, new Vector2(36, 36), 20, TextAnchor.UpperLeft);

            _homeStats = CreateText("Stats", cardLayout, 28, FontStyle.Normal, TextAnchor.UpperLeft, ThemeTextSecondary);
            _homeStats.text = BuildMenuStatsText();

            CreateDivider(cardLayout);

            _homeModeLabel = CreateText("ModeLabel", cardLayout, 28, FontStyle.Bold, TextAnchor.UpperLeft, ThemeTextPrimary);
            Button modeButton = CreateWideButton(cardLayout, "Alternar modo");
            modeButton.onClick.AddListener(ToggleHardMode);

            _homeContrastLabel = CreateText("ContrastLabel", cardLayout, 28, FontStyle.Bold, TextAnchor.UpperLeft, ThemeTextPrimary);
            Button contrastButton = CreateWideButton(cardLayout, "Alternar contraste");
            contrastButton.onClick.AddListener(ToggleContrast);

            _homeTextSizeLabel = CreateText("TextSizeLabel", cardLayout, 28, FontStyle.Bold, TextAnchor.UpperLeft, ThemeTextPrimary);
            Button textSizeButton = CreateWideButton(cardLayout, "Alternar tamanho de texto");
            textSizeButton.onClick.AddListener(ToggleTextSize);

            CreateDivider(cardLayout);

            Text note = CreateText("Note", cardLayout, 24, FontStyle.Normal, TextAnchor.UpperLeft, ThemeTextSecondary);
            note.text = "Conteúdo separado da lógica em JSON. Você pode expandir perfis, cenários e regras sem mexer no fluxo principal.";

            GameObject actions = new GameObject("HomeActions", typeof(RectTransform));
            actions.transform.SetParent(content, false);
            HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 18;
            actionsLayout.childAlignment = TextAnchor.MiddleCenter;
            actionsLayout.childControlHeight = true;
            actionsLayout.childControlWidth = true;
            actionsLayout.childForceExpandWidth = true;
            ContentSizeFitter actionsFit = actions.AddComponent<ContentSizeFitter>();
            actionsFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Button startButton = CreateActionButton(actions.transform, "Começar");
            startButton.onClick.AddListener(StartSession);

            Button tutorialButton = CreateActionButton(actions.transform, "Como jogar");
            tutorialButton.onClick.AddListener(OpenTutorialFromHome);

            Button creditsButton = CreateActionButton(actions.transform, "Créditos");
            creditsButton.onClick.AddListener(OpenCredits);

            RefreshMenuLabels();
            return panel;
        }

        private GameObject BuildMessageScreen(Transform parent)
        {
            GameObject panel = CreateStretchPanel("MessageScreen", parent, ThemeBackground);

            RectTransform content = CreateVerticalLayout(panel.transform, new Vector2(40, 72), 24, TextAnchor.UpperCenter);
            Stretch(content, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(40, 40), new Vector2(-40, -40));

            GameObject scrollRoot = CreateCard("MessageCard", content, ThemePanel);
            LayoutElement scrollElement = scrollRoot.AddComponent<LayoutElement>();
            scrollElement.flexibleHeight = 1f;
            scrollElement.minHeight = 900f;

            RectTransform scrollContent;
            CreateScrollArea(scrollRoot.transform, out scrollContent, ThemePanel);

            _messageTitle = CreateText("MessageTitle", scrollContent, 54, FontStyle.Bold, TextAnchor.UpperLeft, ThemeTextPrimary);
            _messageBody = CreateText("MessageBody", scrollContent, 28, FontStyle.Normal, TextAnchor.UpperLeft, ThemeTextSecondary);

            GameObject actions = new GameObject("MessageActions", typeof(RectTransform));
            actions.transform.SetParent(content, false);
            HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 18;
            actionsLayout.childAlignment = TextAnchor.MiddleCenter;
            actionsLayout.childForceExpandWidth = true;
            ContentSizeFitter actionsFit = actions.AddComponent<ContentSizeFitter>();
            actionsFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _messagePrimaryButton = CreateActionButton(actions.transform, "Continuar");
            _messagePrimaryLabel = _messagePrimaryButton.GetComponentInChildren<Text>();
            _messagePrimaryButton.onClick.AddListener(() =>
            {
                _messagePrimaryAction?.Invoke();
            });

            _messageSecondaryButton = CreateActionButton(actions.transform, "Voltar");
            _messageSecondaryLabel = _messageSecondaryButton.GetComponentInChildren<Text>();
            _messageSecondaryButton.onClick.AddListener(() =>
            {
                _messageSecondaryAction?.Invoke();
            });

            return panel;
        }

        private GameObject BuildGameplayScreen(Transform parent)
        {
            GameObject panel = CreateStretchPanel("GameplayScreen", parent, ThemeBackground);
            RectTransform content = CreateVerticalLayout(panel.transform, new Vector2(40, 56), 24, TextAnchor.UpperCenter);
            Stretch(content, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(40, 40), new Vector2(-40, -40));

            GameObject header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(content, false);
            HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.spacing = 20;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = true;
            headerLayout.childControlHeight = true;
            ContentSizeFitter headerFit = header.AddComponent<ContentSizeFitter>();
            headerFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _phaseLabel = CreateText("PhaseLabel", header.transform, 28, FontStyle.Bold, TextAnchor.MiddleLeft, ThemeAccent);
            _progressLabel = CreateText("ProgressLabel", header.transform, 24, FontStyle.Normal, TextAnchor.MiddleRight, ThemeTextSecondary);

            GameObject card = CreateCard("GameplayCard", content, ThemePanel);
            LayoutElement cardLayoutElement = card.AddComponent<LayoutElement>();
            cardLayoutElement.flexibleHeight = 1f;
            cardLayoutElement.minHeight = 980f;

            RectTransform cardLayout = CreateVerticalLayout(card.transform, new Vector2(36, 36), 22, TextAnchor.UpperLeft);

            _cardTitle = CreateText("CardTitle", cardLayout, 52, FontStyle.Bold, TextAnchor.UpperLeft, ThemeTextPrimary);
            _cardSubtitle = CreateText("CardSubtitle", cardLayout, 26, FontStyle.Italic, TextAnchor.UpperLeft, ThemeTextSecondary);
            _cardBody = CreateText("CardBody", cardLayout, 30, FontStyle.Normal, TextAnchor.UpperLeft, ThemeTextPrimary);

            CreateDivider(cardLayout);

            for (int i = 0; i < _optionButtons.Length; i++)
            {
                int captured = i;
                Button option = CreateWideButton(cardLayout, "Opção " + (i + 1));
                _optionButtons[i] = option;
                _optionLabels[i] = option.GetComponentInChildren<Text>();
                option.onClick.AddListener(() => OnOptionPressed(captured));
            }

            _feedbackLabel = CreateText("Feedback", cardLayout, 26, FontStyle.Normal, TextAnchor.UpperLeft, ThemeTextSecondary);
            _feedbackLabel.gameObject.SetActive(false);

            _nextButton = CreateActionButton(content, "Próximo");
            _nextButtonLabel = _nextButton.GetComponentInChildren<Text>();
            _nextButton.onClick.AddListener(AdvanceAfterFeedback);

            return panel;
        }

        private GameObject BuildResultsScreen(Transform parent)
        {
            GameObject panel = CreateStretchPanel("ResultsScreen", parent, ThemeBackground);

            RectTransform content = CreateVerticalLayout(panel.transform, new Vector2(40, 56), 24, TextAnchor.UpperCenter);
            Stretch(content, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(40, 40), new Vector2(-40, -40));

            GameObject scrollRoot = CreateCard("ResultsCard", content, ThemePanel);
            LayoutElement scrollElement = scrollRoot.AddComponent<LayoutElement>();
            scrollElement.flexibleHeight = 1f;
            scrollElement.minHeight = 980f;

            RectTransform scrollContent;
            CreateScrollArea(scrollRoot.transform, out scrollContent, ThemePanel);

            _resultsTitle = CreateText("ResultsTitle", scrollContent, 56, FontStyle.Bold, TextAnchor.UpperLeft, ThemeTextPrimary);
            _resultsBody = CreateText("ResultsBody", scrollContent, 28, FontStyle.Normal, TextAnchor.UpperLeft, ThemeTextSecondary);

            GameObject actions = new GameObject("ResultsActions", typeof(RectTransform));
            actions.transform.SetParent(content, false);
            HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 18;
            actionsLayout.childAlignment = TextAnchor.MiddleCenter;
            actionsLayout.childForceExpandWidth = true;
            ContentSizeFitter actionsFit = actions.AddComponent<ContentSizeFitter>();
            actionsFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _resultsPrimaryButton = CreateActionButton(actions.transform, "Jogar novamente");
            _resultsPrimaryLabel = _resultsPrimaryButton.GetComponentInChildren<Text>();
            _resultsPrimaryButton.onClick.AddListener(StartSession);

            _resultsSecondaryButton = CreateActionButton(actions.transform, "Menu inicial");
            _resultsSecondaryLabel = _resultsSecondaryButton.GetComponentInChildren<Text>();
            _resultsSecondaryButton.onClick.AddListener(ShowHome);

            return panel;
        }

        private void StartSession()
        {
            _session = IdentificaveisMatchBuilder.Build(_database, _preferences);
            OpenTutorialFromFlow();
        }

        private void OpenTutorialFromHome()
        {
            ShowMessage(
                "Como jogar",
                "Fase 1 — Você verá 6 perfis. Cada um combina avatar descrito, @username, bio e uma postagem. Toque em Humano ou Algoritmo.\n\n" +
                "Fase 2 — Você verá 3 situações emocionalmente tensas. Escolha a resposta que soa mais humana entre quatro alternativas.\n\n" +
                "A chave do jogo não é descobrir fatos, mas perceber atrito: contradição, falha específica, humor involuntário, gestão de imagem, positividade genérica e estética excessivamente coerente.\n\n" +
                "No modo padrão, cada decisão mostra feedback imediato. No modo difícil, o jogo só revela o desempenho ao fim da fase.",
                "Voltar",
                ShowHome,
                null,
                null);
        }

        private void OpenTutorialFromFlow()
        {
            ShowMessage(
                "Tutorial curto",
                "Você está prestes a jogar uma rodada com " + _database.profilesPerRun + " perfis e " + _database.scenariosPerRun + " cenários.\n\n" +
                "Exemplo rápido:\n" +
                "• 'fiz uma lista pra me organizar. perdi a lista.' tende a soar humano porque carrega fracasso banal e específico.\n" +
                "• 'a consistência é o segredo de tudo' tende a soar algorítmico porque organiza a experiência em slogan.\n\n" +
                "Comece lendo pouco e desconfiando do que parece limpo demais.",
                "Continuar",
                BeginProfiles,
                "Pular",
                BeginProfiles);
        }

        private void OpenCredits()
        {
            ShowMessage(
                "Créditos e estrutura",
                "Conceito, GDD e direção autoral: Emli O’Mago.\n\n" +
                "Esta base foi preparada para funcionar sobre o template Universal 2D da Unity 6000.1.14f1, com fluxo completo em UI de Canvas, conteúdo editável em JSON e persistência local de preferências e recorde.\n\n" +
                "Próximo passo ideal: substituir a UI gerada em código por prefabs, mantendo a mesma camada de dados e sessão.",
                "Voltar",
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

            if (_session.phase == PhaseType.Scenarios)
            {
                if (_session.phaseIndex >= _session.activeScenarios.Count)
                {
                    OpenResults();
                    return;
                }

                ShowGameplay();
                RenderScenario(_session.activeScenarios[_session.phaseIndex]);
                return;
            }

            if (_session.phase == PhaseType.Results)
            {
                OpenResults();
            }
        }

        private void RenderProfile(ProfileContentData profile)
        {
            _phaseLabel.text = "Fase 1 — Quem é?";
            _progressLabel.text = (_session.phaseIndex + 1) + " / " + _session.activeProfiles.Count;
            _cardTitle.text = profile.username;
            _cardSubtitle.text = "Avatar: " + profile.avatar;
            _cardBody.text = "Bio\n" + profile.bio + "\n\nPostagem\n" + profile.post;

            ConfigureOptionButton(0, "Humano", true);
            ConfigureOptionButton(1, "Algoritmo", true);
            ConfigureOptionButton(2, string.Empty, false);
            ConfigureOptionButton(3, string.Empty, false);

            _feedbackLabel.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            _session.waitingForAdvance = false;
            SetOptionInteractable(true);
        }

        private void RenderScenario(ScenarioContentData scenario)
        {
            _phaseLabel.text = "Fase 2 — O que posta?";
            _progressLabel.text = (_session.phaseIndex + 1) + " / " + _session.activeScenarios.Count;
            _cardTitle.text = scenario.title;
            _cardSubtitle.text = scenario.person;
            _cardBody.text = "Contexto\n" + scenario.eventText;

            for (int i = 0; i < _optionButtons.Length; i++)
            {
                bool active = i < scenario.choices.Count;
                string label = active ? scenario.choices[i].text : string.Empty;
                ConfigureOptionButton(i, label, active);
            }

            _feedbackLabel.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            _session.waitingForAdvance = false;
            SetOptionInteractable(true);
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

            if (_session.phase == PhaseType.Scenarios)
            {
                HandleScenarioAnswer(optionIndex);
            }
        }

        private void HandleProfileAnswer(int optionIndex)
        {
            ProfileContentData profile = _session.activeProfiles[_session.phaseIndex];
            string playerChoice = optionIndex == 0 ? "humano" : "algoritmo";
            bool correct = string.Equals(profile.correctType, playerChoice, StringComparison.OrdinalIgnoreCase);

            string feedback = BuildProfileFeedback(profile, correct, playerChoice);
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
                _feedbackLabel.text = feedback;
                _feedbackLabel.color = correct ? ThemeSuccess : ThemeError;
                _feedbackLabel.gameObject.SetActive(true);
                _nextButton.gameObject.SetActive(true);
                _nextButtonLabel.text = _session.phaseIndex >= _session.activeProfiles.Count - 1 ? "Ver resumo da fase" : "Próximo perfil";
                _session.waitingForAdvance = true;
                SetOptionInteractable(false);
            }
            else
            {
                _session.phaseIndex++;
                RenderCurrentStep();
            }
        }

        private void HandleScenarioAnswer(int optionIndex)
        {
            ScenarioContentData scenario = _session.activeScenarios[_session.phaseIndex];
            ScenarioChoiceData choice = scenario.choices[optionIndex];
            ScenarioChoiceData humanChoice = scenario.GetHumanChoice();
            bool correct = choice != null && choice.isHuman;

            string feedback = BuildScenarioFeedback(scenario, choice, humanChoice, correct);
            _session.answers.Add(new SessionAnswerRecord
            {
                phase = PhaseType.Scenarios,
                itemId = scenario.id,
                itemIndex = _session.phaseIndex,
                promptTitle = scenario.title,
                playerChoice = choice != null ? choice.id : string.Empty,
                correctChoice = humanChoice != null ? humanChoice.id : string.Empty,
                wasCorrect = correct,
                feedback = feedback,
                patternKey = correct || choice == null ? string.Empty : choice.baitPattern
            });

            if (_session.immediateFeedback)
            {
                _feedbackLabel.text = feedback;
                _feedbackLabel.color = correct ? ThemeSuccess : ThemeError;
                _feedbackLabel.gameObject.SetActive(true);
                _nextButton.gameObject.SetActive(true);
                _nextButtonLabel.text = _session.phaseIndex >= _session.activeScenarios.Count - 1 ? "Ver resultados" : "Próximo cenário";
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

            if (_session.phase == PhaseType.Profiles)
            {
                _session.phaseIndex++;
                RenderCurrentStep();
                return;
            }

            if (_session.phase == PhaseType.Scenarios)
            {
                _session.phaseIndex++;
                RenderCurrentStep();
            }
        }

        private void OpenProfileSummary()
        {
            _session.phase = PhaseType.ProfileSummary;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Você concluiu a Fase 1.");
            builder.AppendLine();
            builder.AppendLine("Acertos: " + _session.ProfileHits + " / " + _session.activeProfiles.Count + ".");
            builder.AppendLine();

            if (_session.immediateFeedback)
            {
                builder.AppendLine("Agora a leitura sai da aparência e vai para a reação emocional.");
            }
            else
            {
                AppendPhaseMistakes(builder, PhaseType.Profiles);
            }

            builder.AppendLine();
            builder.AppendLine("Na próxima fase, escolha a resposta que parece genuinamente humana diante de um contexto emocionalmente tenso.");

            ShowMessage(
                "Resumo da Fase 1",
                builder.ToString(),
                "Ir para a Fase 2",
                BeginScenarios,
                "Menu inicial",
                ShowHome);
        }

        private void BeginScenarios()
        {
            _session.phase = PhaseType.Scenarios;
            _session.phaseIndex = 0;
            _session.waitingForAdvance = false;
            RenderCurrentStep();
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
            SetScreen(_homeScreen);
            _homeStats.text = BuildMenuStatsText();
            RefreshMenuLabels();
        }

        private void ShowGameplay()
        {
            SetScreen(_gameplayScreen);
        }

        private void ShowResults()
        {
            SetScreen(_resultsScreen);

            _resultsTitle.text = "Resultados finais";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(IdentificaveisResultAnalyzer.BuildStatsLine(_session));
            builder.AppendLine();
            builder.AppendLine(_analysis.headline);
            builder.AppendLine();
            builder.AppendLine(_analysis.body);

            if (_analysis.dominantPatterns.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Padrões mais presentes nos seus erros:");
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

        private void ShowMessage(
            string title,
            string body,
            string primaryLabel,
            Action primaryAction,
            string secondaryLabel,
            Action secondaryAction)
        {
            _messageTitle.text = title;
            _messageBody.text = body;
            _messagePrimaryLabel.text = primaryLabel;
            _messagePrimaryAction = primaryAction;

            bool secondaryVisible = !string.IsNullOrEmpty(secondaryLabel) && secondaryAction != null;
            _messageSecondaryButton.gameObject.SetActive(secondaryVisible);
            _messageSecondaryAction = secondaryAction;
            if (secondaryVisible)
            {
                _messageSecondaryLabel.text = secondaryLabel;
            }

            SetScreen(_messageScreen);
        }

        private void SetScreen(GameObject activeScreen)
        {
            _homeScreen.SetActive(activeScreen == _homeScreen);
            _messageScreen.SetActive(activeScreen == _messageScreen);
            _gameplayScreen.SetActive(activeScreen == _gameplayScreen);
            _resultsScreen.SetActive(activeScreen == _resultsScreen);
        }

        private string BuildProfileFeedback(ProfileContentData profile, bool correct, string playerChoice)
        {
            string expected = string.Equals(profile.correctType, "humano", StringComparison.OrdinalIgnoreCase) ? "Humano" : "Algoritmo";
            string prefix = correct ? "Acerto. " : "Erro. ";
            return prefix + "A leitura correta era " + expected + ". " + profile.signalKey;
        }

        private string BuildScenarioFeedback(ScenarioContentData scenario, ScenarioChoiceData choice, ScenarioChoiceData humanChoice, bool correct)
        {
            if (correct)
            {
                return "Acerto. " + scenario.reading;
            }

            string picked = choice != null ? choice.text : "opção inválida";
            string human = humanChoice != null ? humanChoice.text : "resposta humana não definida";
            return "Erro. A opção humana era: \"" + human + "\". Você escolheu uma alternativa mais próxima de uma lógica de otimização: \"" + picked + "\". " + scenario.reading;
        }

        private void AppendPhaseMistakes(StringBuilder builder, PhaseType phase)
        {
            int shown = 0;
            builder.AppendLine("Erros desta fase:");
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

        private string BuildMenuStatsText()
        {
            int maxScore = _database.profilesPerRun + _database.scenariosPerRun;
            return "Melhor pontuação: " + IdentificaveisPreferencesStore.LoadBestScore() + " / " + maxScore + "\n" +
                   "Partidas jogadas: " + IdentificaveisPreferencesStore.LoadPlayedCount() + "\n" +
                   "Rodada padrão: " + _database.profilesPerRun + " perfis + " + _database.scenariosPerRun + " cenários.";
        }

        private void RefreshMenuLabels()
        {
            _homeModeLabel.text = "Modo: " + (_preferences.hardMode ? "Difícil (feedback só ao fim)" : "Padrão (feedback imediato)");
            _homeContrastLabel.text = "Contraste: " + (_preferences.highContrast ? "Alto" : "Padrão");
            _homeTextSizeLabel.text = "Texto: " + (_preferences.largeText ? "Ampliado" : "Padrão");
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
            RebuildForPreferenceChange();
        }

        private void ToggleTextSize()
        {
            _preferences.largeText = !_preferences.largeText;
            IdentificaveisPreferencesStore.SavePreferences(_preferences);
            RebuildForPreferenceChange();
        }

        private void RebuildForPreferenceChange()
        {
            if (_canvas != null)
            {
                Destroy(_canvas.gameObject);
            }

            BuildUi();
            ShowHome();
        }

        private void ConfigureOptionButton(int index, string label, bool active)
        {
            if (index < 0 || index >= _optionButtons.Length)
            {
                return;
            }

            _optionButtons[index].gameObject.SetActive(active);
            if (active)
            {
                _optionLabels[index].text = label;
            }
        }

        private void SetOptionInteractable(bool interactable)
        {
            for (int i = 0; i < _optionButtons.Length; i++)
            {
                if (_optionButtons[i] != null)
                {
                    _optionButtons[i].interactable = interactable;
                }
            }
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
            Font builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (builtin != null)
            {
                return builtin;
            }

            Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (arial != null)
            {
                return arial;
            }

            return Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        private GameObject CreateStretchPanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private RectTransform CreateVerticalLayout(Transform parent, Vector2 padding, float spacing, TextAnchor alignment)
        {
            GameObject go = new GameObject("Layout", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();

            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.y), Mathf.RoundToInt(padding.y));
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            return rect;
        }

        private GameObject CreateCard(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            LayoutElement element = go.AddComponent<LayoutElement>();
            element.minHeight = 240f;
            return go;
        }

        private void CreateScrollArea(Transform parent, out RectTransform content, Color backgroundColor)
        {
            GameObject root = new GameObject("ScrollRoot", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = backgroundColor;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport.transform, false);
            content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(36, 36, 36, 36);
            layout.spacing = 24;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect scroll = root.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 24f;
        }

        private Text CreateText(string name, Transform parent, int size, FontStyle style, TextAnchor anchor, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = Mathf.RoundToInt(size * TextScale);
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;

            ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            return text;
        }

        private Button CreateWideButton(Transform parent, string label)
        {
            Button button = CreateButtonBase(parent, label, 26, ThemeMuted, ThemeTextPrimary);
            LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 110f;
            return button;
        }

        private Button CreateActionButton(Transform parent, string label)
        {
            Button button = CreateButtonBase(parent, label, 28, ThemeAccent, Color.white);
            LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 120f;
            element.flexibleWidth = 1f;
            return button;
        }

        private Button CreateButtonBase(Transform parent, string label, int fontSize, Color background, Color textColor)
        {
            GameObject buttonGo = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            Image image = buttonGo.GetComponent<Image>();
            image.color = background;

            Button button = buttonGo.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = background * 1.05f;
            colors.pressedColor = background * 0.95f;
            colors.selectedColor = background;
            colors.disabledColor = new Color(background.r, background.g, background.b, 0.45f);
            button.colors = colors;

            RectTransform rect = buttonGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 110f);

            Text labelText = CreateText("Label", buttonGo.transform, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, textColor);
            Stretch(labelText.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(20f, 14f), new Vector2(-20f, -14f));
            labelText.text = label;

            return button;
        }

        private void CreateDivider(Transform parent)
        {
            GameObject divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(parent, false);
            Image image = divider.GetComponent<Image>();
            image.color = ThemeMuted * 0.85f;
            LayoutElement element = divider.AddComponent<LayoutElement>();
            element.minHeight = 2f;
            element.preferredHeight = 2f;
        }

        private void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
