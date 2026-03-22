using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Identificaveis
{
    public static class IdentificaveisPreferencesStore
    {
        private const string KeyHardMode = "Identificaveis.HardMode";
        private const string KeyHighContrast = "Identificaveis.HighContrast";
        private const string KeyLargeText = "Identificaveis.LargeText";
        private const string KeyBestScore = "Identificaveis.BestScore";
        private const string KeyPlayedCount = "Identificaveis.PlayedCount";
        private const string KeyRecentProfiles = "Identificaveis.RecentProfiles";
        private const string KeyRecentScenarios = "Identificaveis.RecentScenarios";

        public static AppPreferences LoadPreferences()
        {
            return new AppPreferences
            {
                hardMode = PlayerPrefs.GetInt(KeyHardMode, 0) == 1,
                highContrast = PlayerPrefs.GetInt(KeyHighContrast, 0) == 1,
                largeText = PlayerPrefs.GetInt(KeyLargeText, 0) == 1
            };
        }

        public static void SavePreferences(AppPreferences preferences)
        {
            PlayerPrefs.SetInt(KeyHardMode, preferences != null && preferences.hardMode ? 1 : 0);
            PlayerPrefs.SetInt(KeyHighContrast, preferences != null && preferences.highContrast ? 1 : 0);
            PlayerPrefs.SetInt(KeyLargeText, preferences != null && preferences.largeText ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static int LoadBestScore()
        {
            return PlayerPrefs.GetInt(KeyBestScore, 0);
        }

        public static int LoadPlayedCount()
        {
            return PlayerPrefs.GetInt(KeyPlayedCount, 0);
        }

        public static void RegisterMatchResult(int score)
        {
            int best = LoadBestScore();
            if (score > best)
            {
                PlayerPrefs.SetInt(KeyBestScore, score);
            }

            PlayerPrefs.SetInt(KeyPlayedCount, LoadPlayedCount() + 1);
            PlayerPrefs.Save();
        }

        public static List<string> LoadRecentProfiles()
        {
            return SplitCsv(PlayerPrefs.GetString(KeyRecentProfiles, string.Empty));
        }

        public static List<string> LoadRecentScenarios()
        {
            return SplitCsv(PlayerPrefs.GetString(KeyRecentScenarios, string.Empty));
        }

        public static void SaveRecentProfiles(List<ProfileContentData> entries, int window)
        {
            SaveRecent(KeyRecentProfiles, CollectIds(entries), window);
        }

        public static void SaveRecentScenarios(List<ScenarioContentData> entries, int window)
        {
            SaveRecent(KeyRecentScenarios, CollectIds(entries), window);
        }

        private static List<string> CollectIds(List<ProfileContentData> entries)
        {
            var ids = new List<string>();
            if (entries == null)
            {
                return ids;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && !string.IsNullOrEmpty(entries[i].id))
                {
                    ids.Add(entries[i].id);
                }
            }

            return ids;
        }

        private static List<string> CollectIds(List<ScenarioContentData> entries)
        {
            var ids = new List<string>();
            if (entries == null)
            {
                return ids;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && !string.IsNullOrEmpty(entries[i].id))
                {
                    ids.Add(entries[i].id);
                }
            }

            return ids;
        }

        private static void SaveRecent(string key, List<string> newIds, int window)
        {
            List<string> buffer = SplitCsv(PlayerPrefs.GetString(key, string.Empty));
            if (newIds != null)
            {
                for (int i = 0; i < newIds.Count; i++)
                {
                    string id = newIds[i];
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    buffer.Remove(id);
                    buffer.Insert(0, id);
                }
            }

            if (window > 0 && buffer.Count > window)
            {
                buffer.RemoveRange(window, buffer.Count - window);
            }

            PlayerPrefs.SetString(key, string.Join(",", buffer.ToArray()));
            PlayerPrefs.Save();
        }

        private static List<string> SplitCsv(string csv)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return list;
            }

            string[] parts = csv.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string value = parts[i].Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    list.Add(value);
                }
            }

            return list;
        }
    }

    public static class IdentificaveisMatchBuilder
    {
        public static SessionState Build(IdentificaveisContentDatabase database, AppPreferences preferences)
        {
            var session = new SessionState();
            session.immediateFeedback = preferences == null || !preferences.hardMode;
            session.phase = PhaseType.Tutorial;
            session.phaseIndex = 0;

            List<string> recentProfiles = IdentificaveisPreferencesStore.LoadRecentProfiles();
            List<string> recentScenarios = IdentificaveisPreferencesStore.LoadRecentScenarios();

            List<ProfileContentData> profiles = PickProfiles(database.profiles, database.profilesPerRun, recentProfiles);
            List<ScenarioContentData> scenarios = PickScenarios(database.scenarios, database.scenariosPerRun, recentScenarios);

            session.activeProfiles.AddRange(profiles);
            session.activeScenarios.AddRange(scenarios);

            IdentificaveisPreferencesStore.SaveRecentProfiles(session.activeProfiles, database.recentWindowProfiles);
            IdentificaveisPreferencesStore.SaveRecentScenarios(session.activeScenarios, database.recentWindowScenarios);

            return session;
        }

        private static List<ProfileContentData> PickProfiles(List<ProfileContentData> source, int amount, List<string> recentIds)
        {
            var preferredHumans = new List<ProfileContentData>();
            var preferredAlgorithms = new List<ProfileContentData>();
            var fallbackHumans = new List<ProfileContentData>();
            var fallbackAlgorithms = new List<ProfileContentData>();

            for (int i = 0; i < source.Count; i++)
            {
                ProfileContentData item = source[i];
                if (item == null)
                {
                    continue;
                }

                bool isRecent = recentIds.Contains(item.id);
                bool isHuman = string.Equals(item.correctType, "humano", StringComparison.OrdinalIgnoreCase);

                if (!isRecent && isHuman)
                {
                    preferredHumans.Add(item);
                }
                else if (!isRecent)
                {
                    preferredAlgorithms.Add(item);
                }
                else if (isHuman)
                {
                    fallbackHumans.Add(item);
                }
                else
                {
                    fallbackAlgorithms.Add(item);
                }
            }

            Shuffle(preferredHumans);
            Shuffle(preferredAlgorithms);
            Shuffle(fallbackHumans);
            Shuffle(fallbackAlgorithms);

            int half = Mathf.Max(1, amount / 2);
            var result = new List<ProfileContentData>();

            TakeInto(result, preferredHumans, half);
            TakeInto(result, preferredAlgorithms, half);

            while (result.Count < amount && fallbackHumans.Count > 0)
            {
                result.Add(PopFirst(fallbackHumans));
            }

            while (result.Count < amount && fallbackAlgorithms.Count > 0)
            {
                result.Add(PopFirst(fallbackAlgorithms));
            }

            while (result.Count < amount && preferredHumans.Count > 0)
            {
                result.Add(PopFirst(preferredHumans));
            }

            while (result.Count < amount && preferredAlgorithms.Count > 0)
            {
                result.Add(PopFirst(preferredAlgorithms));
            }

            Shuffle(result);
            return result;
        }

        private static List<ScenarioContentData> PickScenarios(List<ScenarioContentData> source, int amount, List<string> recentIds)
        {
            var preferred = new List<ScenarioContentData>();
            var fallback = new List<ScenarioContentData>();

            for (int i = 0; i < source.Count; i++)
            {
                ScenarioContentData item = source[i];
                if (item == null)
                {
                    continue;
                }

                if (recentIds.Contains(item.id))
                {
                    fallback.Add(item);
                }
                else
                {
                    preferred.Add(item);
                }
            }

            Shuffle(preferred);
            Shuffle(fallback);

            var result = new List<ScenarioContentData>();
            TakeInto(result, preferred, amount);

            while (result.Count < amount && fallback.Count > 0)
            {
                result.Add(PopFirst(fallback));
            }

            return result;
        }

        private static void TakeInto<T>(List<T> target, List<T> source, int count)
        {
            int amount = Mathf.Min(count, source.Count);
            for (int i = 0; i < amount; i++)
            {
                target.Add(source[0]);
                source.RemoveAt(0);
            }
        }

        private static T PopFirst<T>(List<T> source)
        {
            T value = source[0];
            source.RemoveAt(0);
            return value;
        }

        private static void Shuffle<T>(List<T> list)
        {
            System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swap = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[swap];
                list[swap] = temp;
            }
        }
    }

    public static class IdentificaveisResultAnalyzer
    {
        public static SessionAnalysis Analyze(SessionState session)
        {
            var analysis = new SessionAnalysis();
            int total = session.TotalHits;

            if (total <= 3)
            {
                analysis.headline = "Você leu sinais muito otimizados como se fossem humanidade.";
                analysis.body = "Seu desempenho sugere que estética coerente, frases limpas e respostas organizadas pareceram mais confiáveis do que contradição, ruído e afeto desajeitado.";
            }
            else if (total <= 6)
            {
                analysis.headline = "Sua leitura foi intermediária, mas ainda vulnerável a armadilhas de forma.";
                analysis.body = "Você percebeu parte das fissuras humanas, mas ainda premiou respostas excessivamente legíveis ou perfis limpos demais em alguns momentos decisivos.";
            }
            else
            {
                analysis.headline = "Você teve uma leitura forte de humanidade digital.";
                analysis.body = "Mesmo assim, o jogo foi construído para lembrar que autenticidade online continua sujeita a estética, contexto e ao desconforto que sentimos diante do que não parece bem editado.";
            }

            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < session.answers.Count; i++)
            {
                SessionAnswerRecord answer = session.answers[i];
                if (answer.wasCorrect || string.IsNullOrEmpty(answer.patternKey))
                {
                    continue;
                }

                if (counts.ContainsKey(answer.patternKey))
                {
                    counts[answer.patternKey]++;
                }
                else
                {
                    counts[answer.patternKey] = 1;
                }
            }

            foreach (KeyValuePair<string, int> entry in SortByValueDescending(counts))
            {
                string sentence = PatternToSentence(entry.Key);
                if (!string.IsNullOrEmpty(sentence) && !analysis.dominantPatterns.Contains(sentence))
                {
                    analysis.dominantPatterns.Add(sentence);
                }

                if (analysis.dominantPatterns.Count >= 3)
                {
                    break;
                }
            }

            int commented = 0;
            for (int i = 0; i < session.answers.Count; i++)
            {
                SessionAnswerRecord answer = session.answers[i];
                if (answer.wasCorrect)
                {
                    continue;
                }

                analysis.commentedMistakes.Add(answer.promptTitle + " — " + answer.feedback);
                commented++;
                if (commented >= 2)
                {
                    break;
                }
            }

            return analysis;
        }

        public static string BuildStatsLine(SessionState session)
        {
            return "Fase 1: " + session.ProfileHits + "/" + session.activeProfiles.Count
                + "  •  Fase 2: " + session.ScenarioHits + "/" + session.activeScenarios.Count
                + "  •  Total: " + session.TotalHits + "/" + (session.activeProfiles.Count + session.activeScenarios.Count);
        }

        public static string BuildClosingText()
        {
            return "Confundimos o que é humano porque aprendemos a performar para sistemas que nos leem. E esses sistemas aprenderam a falar como nós. No fim, a pergunta talvez não seja quem parece humano, mas que tipo de humanidade ainda reconhecemos quando ela não está otimizada.";
        }

        private static List<KeyValuePair<string, int>> SortByValueDescending(Dictionary<string, int> source)
        {
            var list = new List<KeyValuePair<string, int>>(source);
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            return list;
        }

        private static string PatternToSentence(string patternKey)
        {
            switch (patternKey)
            {
                case "estetica_coerente":
                    return "Você confiou demais em estética coerente e bem editada.";
                case "frase_universal":
                    return "Você tratou frases universais como profundidade.";
                case "positividade_generica":
                    return "Você associou positividade genérica a maturidade emocional.";
                case "engajamento_social":
                    return "Você confundiu pedido de engajamento com partilha honesta.";
                case "gestao_de_imagem":
                    return "Você leu gestão de imagem como sinceridade.";
                case "branding_emocional":
                    return "Você aceitou branding emocional como se fosse afeto cru.";
                case "abstracao_poetica":
                    return "Você interpretou abstração poética como humanidade concreta.";
                case "vulnerabilidade_desorganizada":
                    return "Você estranhou vulnerabilidade desorganizada demais.";
                case "falha_especifica":
                    return "Você subestimou falhas específicas e banais, que costumam ser profundamente humanas.";
                case "otimizacao_pessoal":
                    return "Você premiou linguagem de autoaperfeiçoamento como se fosse traço pessoal.";
                case "narrativa_pronta":
                    return "Você tratou narrativas prontas de superação como emoção genuína.";
                case "espiritualizacao":
                    return "Você leu espiritualização imediata como se ela resolvesse o afeto.";
                case "apelo_emocional":
                    return "Você reagiu mais a apelos de solidariedade do que a sensação concreta da cena.";
                case "humor_defensivo":
                    return "Você deixou passar humor defensivo, um sinal humano bastante recorrente.";
                default:
                    return string.Empty;
            }
        }
    }
}
