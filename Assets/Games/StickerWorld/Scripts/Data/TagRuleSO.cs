using System.Collections.Generic;
using GamePrototype.StickerWorld.Core;
using UnityEngine;

namespace GamePrototype.StickerWorld.Data
{
    [CreateAssetMenu(menuName = "Sticker World/Tag Rule", fileName = "Rule_")]
    public sealed class TagRuleSO : ScriptableObject
    {
        public string id;
        public RuleEvent eventType;
        public int priority;
        public TagSO[] requiredTags;
        public TagSO[] blockedTags;
        public RuleEffect[] effects;
        public bool stopAfterMatch;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id.Trim();

        public bool Matches(RuleEvent ruleEvent, ISet<string> activeTagIds)
        {
            if (ruleEvent != eventType || activeTagIds == null)
            {
                return false;
            }

            if (!ContainsAll(activeTagIds, requiredTags))
            {
                return false;
            }

            return !ContainsAny(activeTagIds, blockedTags);
        }

        private static bool ContainsAll(ISet<string> activeTagIds, IEnumerable<TagSO> required)
        {
            if (required == null)
            {
                return true;
            }

            foreach (var tag in required)
            {
                var tagId = TagIdUtility.FromTag(tag);
                if (!string.IsNullOrEmpty(tagId) && !activeTagIds.Contains(tagId))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsAny(ISet<string> activeTagIds, IEnumerable<TagSO> blocked)
        {
            if (blocked == null)
            {
                return false;
            }

            foreach (var tag in blocked)
            {
                var tagId = TagIdUtility.FromTag(tag);
                if (!string.IsNullOrEmpty(tagId) && activeTagIds.Contains(tagId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
