using System.Collections.Generic;
using GamePrototype.StickerWorld.Data;

namespace GamePrototype.StickerWorld.Core
{
    public sealed class StickerApplicationService
    {
        private readonly RuleResolver resolver;

        public StickerApplicationService()
            : this(new RuleResolver())
        {
        }

        public StickerApplicationService(RuleResolver resolver)
        {
            this.resolver = resolver ?? new RuleResolver();
        }

        public StickerApplicationResult Apply(
            StickerSO sticker,
            IEnumerable<string> currentTagIds,
            IEnumerable<TagRuleSO> rules,
            RuleEvent ruleEvent = RuleEvent.StickerApplied)
        {
            var nextTags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (currentTagIds != null)
            {
                foreach (var tagId in currentTagIds)
                {
                    var normalized = TagIdUtility.Normalize(tagId);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        nextTags.Add(normalized);
                    }
                }
            }

            if (sticker != null)
            {
                foreach (var tagId in sticker.RemovedTagIds)
                {
                    nextTags.Remove(tagId);
                }

                foreach (var tagId in sticker.AddedTagIds)
                {
                    nextTags.Add(tagId);
                }
            }

            var resolution = resolver.Resolve(rules, ruleEvent, nextTags);
            return new StickerApplicationResult(nextTags, resolution);
        }
    }
}
