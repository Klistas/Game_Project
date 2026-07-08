using System;
using GamePrototype.StickerWorld.Data;

namespace GamePrototype.StickerWorld.Core
{
    public static class TagIdUtility
    {
        public static string Normalize(string tagId)
        {
            return string.IsNullOrWhiteSpace(tagId) ? string.Empty : tagId.Trim();
        }

        public static string FromTag(TagSO tag)
        {
            if (tag == null)
            {
                return string.Empty;
            }

            return Normalize(tag.Id);
        }

        public static bool Equals(string a, string b)
        {
            return string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
        }
    }
}
