#if UNITY_EDITOR
using GamePrototype.Shared;
using UnityEditor;

namespace GamePrototype.BodyRebels
{
    public static class BodyRebelsPrototypeMenu
    {
        [MenuItem("Game Prototypes/Active Prototype/Body Rebels")]
        private static void SetActive()
        {
            PrototypeRuntime.SetActive(BodyRebelsPrototype.PrototypeId);
        }
    }
}
#endif
