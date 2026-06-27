#if UNITY_EDITOR
using GamePrototype.Shared;
using UnityEditor;

namespace GamePrototype.EveryoneInnocent
{
    public static class EveryoneInnocentPrototypeMenu
    {
        [MenuItem("Game Prototypes/Active Prototype/Everyone Innocent")]
        private static void SetActive()
        {
            PrototypeRuntime.SetActive(EveryoneInnocentPrototype.PrototypeId);
        }
    }
}
#endif
