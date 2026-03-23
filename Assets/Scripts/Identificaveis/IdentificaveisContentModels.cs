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
        public int profilesPerRun = 8;
        public int scenariosPerRun = 4; // legado
        public int reactionsPerRun = 4;
        public int commentsPerRun = 4;
        public int messagesPerRun = 4;
        public int justificationsPerRun = 4;
        public int recentWindowProfiles = 18;
        public int recentWindowScenarios = 8; // legado
        public int recentWindowPrompts = 24;
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
        public string phaseKey;
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
        Reactions = 5,
        Comments = 6,
        Messages = 7,
        Justifications = 8,
        Results = 9
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
        public readonly List<ScenarioContentData> activeReactions = new List<ScenarioContentData>();
        public readonly List<ScenarioContentData> activeComments = new List<ScenarioContentData>();
        public readonly List<ScenarioContentData> activeMessages = new List<ScenarioContentData>();
        public readonly List<ScenarioContentData> activeJustifications = new List<ScenarioContentData>();
        public readonly List<SessionAnswerRecord> answers = new List<SessionAnswerRecord>();

        public PhaseType phase = PhaseType.Tutorial;
        public int phaseIndex;
        public bool waitingForAdvance;
        public bool immediateFeedback = true;

        public int ProfileHits => CountHits(PhaseType.Profiles);
        public int ReactionHits => CountHits(PhaseType.Reactions);
        public int CommentHits => CountHits(PhaseType.Comments);
        public int MessageHits => CountHits(PhaseType.Messages);
        public int JustificationHits => CountHits(PhaseType.Justifications);
        public int PromptHits => ReactionHits + CommentHits + MessageHits + JustificationHits;
        public int TotalHits => ProfileHits + PromptHits;
        public int TotalQuestions => activeProfiles.Count + activeReactions.Count + activeComments.Count + activeMessages.Count + activeJustifications.Count;

        public int GetPhaseCount(PhaseType phase)
        {
            switch (phase)
            {
                case PhaseType.Profiles:
                    return activeProfiles.Count;
                case PhaseType.Reactions:
                    return activeReactions.Count;
                case PhaseType.Comments:
                    return activeComments.Count;
                case PhaseType.Messages:
                    return activeMessages.Count;
                case PhaseType.Justifications:
                    return activeJustifications.Count;
                default:
                    return 0;
            }
        }

        public int CountHits(PhaseType phase)
        {
            int total = 0;
            for (int i = 0; i < answers.Count; i++)
            {
                if (answers[i].phase == phase && answers[i].wasCorrect)
                {
                    total++;
                }
            }

            return total;
        }
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
