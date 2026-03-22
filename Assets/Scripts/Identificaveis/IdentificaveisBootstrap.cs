using UnityEngine;

namespace Identificaveis
{
    public sealed class IdentificaveisBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntime()
        {
            if (FindAnyObjectByType<IdentificaveisApp>() != null)
            {
                return;
            }

            GameObject root = new GameObject("IdentificaveisApp");
            DontDestroyOnLoad(root);
            root.AddComponent<IdentificaveisApp>();
        }
    }
}
