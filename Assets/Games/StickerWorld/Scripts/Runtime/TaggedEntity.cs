using System.Collections.Generic;
using GamePrototype.StickerWorld.Core;
using GamePrototype.StickerWorld.Data;
using UnityEngine;

namespace GamePrototype.StickerWorld.Runtime
{
    public sealed class TaggedEntity : MonoBehaviour
    {
        [SerializeField] private TagSO[] baseTags;

        private readonly List<string> runtimeTagIds = new List<string>();
        private readonly StickerApplicationService stickerService = new StickerApplicationService();

        public IReadOnlyList<string> RuntimeTagIds => runtimeTagIds;

        private void Awake()
        {
            ResetTags();
        }

        public void ResetTags()
        {
            runtimeTagIds.Clear();
            if (baseTags == null)
            {
                return;
            }

            foreach (var tag in baseTags)
            {
                var tagId = TagIdUtility.FromTag(tag);
                if (!string.IsNullOrEmpty(tagId) && !runtimeTagIds.Contains(tagId))
                {
                    runtimeTagIds.Add(tagId);
                }
            }
        }

        public StickerApplicationResult ApplySticker(StickerSO sticker, IEnumerable<TagRuleSO> rules)
        {
            var result = stickerService.Apply(sticker, runtimeTagIds, rules);
            runtimeTagIds.Clear();
            runtimeTagIds.AddRange(result.TagIds);
            return result;
        }

        public bool HasTag(string tagId)
        {
            foreach (var current in runtimeTagIds)
            {
                if (TagIdUtility.Equals(current, tagId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
