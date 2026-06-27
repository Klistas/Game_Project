#if UNITY_EDITOR
using GamePrototype.Shared;
using UnityEditor;

namespace GamePrototype.IntendedFeature
{
    public static class IntendedFeaturePrototypeMenu
    {
        [MenuItem("Game Prototypes/Active Prototype/Intended Feature")]
        private static void SetActive()
        {
            PrototypeRuntime.SetActive(IntendedFeaturePrototype.PrototypeId);
        }
    }
}
#endif
