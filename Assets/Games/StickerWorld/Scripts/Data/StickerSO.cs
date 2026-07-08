using System.Collections.Generic;
using GamePrototype.StickerWorld.Core;
using UnityEngine;

namespace GamePrototype.StickerWorld.Data
{
    [CreateAssetMenu(menuName = "Sticker World/Sticker", fileName = "Sticker_")]
    public sealed class StickerSO : ScriptableObject
    {
        public string id;
        public string displayName;
        public TagSO[] addedTags;
        public TagSO[] removedTags;
        public int maxUsesInStage = 1;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id.Trim();

        public IEnumerable<string> AddedTagIds => EnumerateTagIds(addedTags);
        public IEnumerable<string> RemovedTagIds => EnumerateTagIds(removedTags);

        private static IEnumerable<string> EnumerateTagIds(IEnumerable<TagSO> tags)
        {
            if (tags == null)
            {
                yield break;
            }

            foreach (var tag in tags)
            {
                var tagId = TagIdUtility.FromTag(tag);
                if (!string.IsNullOrEmpty(tagId))
                {
                    yield return tagId;
                }
            }
        }
    }
}
