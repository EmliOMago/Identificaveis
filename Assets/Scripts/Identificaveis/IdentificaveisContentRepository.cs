using UnityEngine;

namespace Identificaveis
{
    public static class IdentificaveisContentRepository
    {
        private const string ResourcePath = "Identificaveis/default_content";
        private static IdentificaveisContentDatabase _cached;

        public static IdentificaveisContentDatabase Load()
        {
            if (_cached != null)
            {
                return _cached;
            }

            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                Debug.LogError("[Identificáveis] Arquivo de conteúdo não encontrado em Resources/" + ResourcePath + ".json");
                _cached = new IdentificaveisContentDatabase();
                return _cached;
            }

            _cached = JsonUtility.FromJson<IdentificaveisContentDatabase>(asset.text);
            if (_cached == null)
            {
                Debug.LogError("[Identificáveis] Falha ao desserializar o conteúdo do jogo.");
                _cached = new IdentificaveisContentDatabase();
            }

            if (_cached.profiles == null)
            {
                _cached.profiles = new System.Collections.Generic.List<ProfileContentData>();
            }

            if (_cached.scenarios == null)
            {
                _cached.scenarios = new System.Collections.Generic.List<ScenarioContentData>();
            }
            return _cached;
        }
    }
}
