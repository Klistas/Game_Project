using System.Collections.Generic;

namespace GamePrototype.StickerWorld.Gameplay
{
    public static class StickerWorldG0Rules
    {
        public const string Player = "player";
        public const string Guard = "guard";
        public const string Cctv = "cctv";
        public const string VaultDoor = "vault_door";
        public const string ThinWall = "thin_wall";

        public static bool IsBankGoalComplete(IReadOnlyDictionary<string, IReadOnlyCollection<string>> targetTags)
        {
            if (targetTags == null)
            {
                return false;
            }

            bool playerCanEnter = HasTag(targetTags, Player, "Tiny") || HasTag(targetTags, Player, "Ghost");
            bool guardNeutralized = HasAny(targetTags, Guard, "Disabled", "Asleep", "Distracted", "Bowing");
            bool routeOpen = HasAny(targetTags, Cctv, "Disabled", "Destroyed") ||
                HasAny(targetTags, VaultDoor, "Open", "Destroyed") ||
                HasAny(targetTags, ThinWall, "Destroyed", "Open");

            return playerCanEnter && guardNeutralized && routeOpen;
        }

        public static bool HasAny(
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> targetTags,
            string targetId,
            params string[] tags)
        {
            foreach (var tag in tags)
            {
                if (HasTag(targetTags, targetId, tag))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasTag(
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> targetTags,
            string targetId,
            string tag)
        {
            if (!targetTags.TryGetValue(targetId, out var tags) || tags == null)
            {
                return false;
            }

            foreach (var current in tags)
            {
                if (string.Equals(current, tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
