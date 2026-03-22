using System;
using System.Collections.Generic;

namespace Identificaveis
{
    [Serializable]
    public sealed class IdentificaveisContentDatabase
    {
        public string version;
        public string displayName;
        public string subtitle;
        public int profilesPerRun = 6;
        public int scenariosPerRun = 3;
        public int recentWindowProfiles = 8;
        public int recentWindowScenarios = 4;
        public List<ProfileContentData> profiles = new List<ProfileContentData>();
        public List<ScenarioContentData> scenarios = new List<ScenarioContentData>();
    }

    [Serializable]
    public sealed class ProfileContentData
    {
        public string id;
        public string correctType;
        public string avatar;
        public string username;
        public string bio;
        public string post;
        public string signalKey;
        public string baitPattern;
        public int difficulty = 1;
    }

    [Serializable]
    public sealed class ScenarioContentData
    {
        public string id;
        public string title;
        public string person;
        public string eventText;
        public string reading;
        public List<ScenarioChoiceData> choices = new List<ScenarioChoiceData>();

        public ScenarioChoiceData GetHumanChoice()
        {
            if (choices == null)
            {
                return null;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i] != null && choices[i].isHuman)
                {
                    return choices[i];
                }
            }

            return null;
        }
    }

    [Serializable]
    public sealed class ScenarioChoiceData
    {
        public string id;
        public string text;
        public bool isHuman;
        public string baitPattern;
    }

    public enum PhaseType
    {
        None = 0,
        Tutorial = 1,
        Profiles = 2,
        ProfileSummary = 3,
        Transition = 4,
        Scenarios = 5,
        Results = 6
    }

    public enum PlayerChoiceKind
    {
        None = 0,
        Human = 1,
        Algorithm = 2
    }

    [Serializable]
    public sealed class SessionAnswerRecord
    {
        public PhaseType phase;
        public string itemId;
        public string promptTitle;
        public string playerChoice;
        public string correctChoice;
        public bool wasCorrect;
        public string feedback;
        public string patternKey;
        public int itemIndex;
    }

    public sealed class SessionState
    {
        public readonly List<ProfileContentData> activeProfiles = new List<ProfileContentData>();
        public readonly List<ScenarioContentData> activeScenarios = new List<ScenarioContentData>();
        public readonly List<SessionAnswerRecord> answers = new List<SessionAnswerRecord>();

        public PhaseType phase = PhaseType.Tutorial;
        public int phaseIndex;
        public bool waitingForAdvance;
        public bool immediateFeedback = true;

        public int ProfileHits
        {
            get
            {
                int total = 0;
                for (int i = 0; i < answers.Count; i++)
                {
                    if (answers[i].phase == PhaseType.Profiles && answers[i].wasCorrect)
                    {
                        total++;
                    }
                }

                return total;
            }
        }

        public int ScenarioHits
        {
            get
            {
                int total = 0;
                for (int i = 0; i < answers.Count; i++)
                {
                    if (answers[i].phase == PhaseType.Scenarios && answers[i].wasCorrect)
                    {
                        total++;
                    }
                }

                return total;
            }
        }

        public int TotalHits => ProfileHits + ScenarioHits;
    }

    public sealed class SessionAnalysis
    {
        public string headline;
        public string body;
        public readonly List<string> dominantPatterns = new List<string>();
        public readonly List<string> commentedMistakes = new List<string>();
    }

    [Serializable]
    public sealed class AppPreferences
    {
        public bool hardMode;
        public bool highContrast;
        public bool largeText;
    }
}
