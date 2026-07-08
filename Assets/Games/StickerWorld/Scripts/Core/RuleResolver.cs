using System.Collections.Generic;
using System.Linq;
using GamePrototype.StickerWorld.Data;

namespace GamePrototype.StickerWorld.Core
{
    public sealed class RuleResolver
    {
        public RuleResolution Resolve(IEnumerable<TagRuleSO> rules, RuleEvent ruleEvent, IEnumerable<string> tagIds)
        {
            var activeTags = BuildTagSet(tagIds);
            var reactions = new List<ResolvedReaction>();
            var stopRequested = false;

            if (rules == null)
            {
                return new RuleResolution(reactions, false);
            }

            foreach (var rule in rules.Where(rule => rule != null)
                .OrderByDescending(rule => rule.priority)
                .ThenBy(rule => rule.Id))
            {
                if (!rule.Matches(ruleEvent, activeTags))
                {
                    continue;
                }

                var effects = rule.effects;
                if (effects != null)
                {
                    foreach (var effect in effects)
                    {
                        reactions.Add(new ResolvedReaction(rule.Id, rule.priority, effect));
                    }
                }

                if (rule.stopAfterMatch)
                {
                    stopRequested = true;
                    break;
                }
            }

            return new RuleResolution(reactions, stopRequested);
        }

        private static HashSet<string> BuildTagSet(IEnumerable<string> tagIds)
        {
            var tags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (tagIds == null)
            {
                return tags;
            }

            foreach (var tagId in tagIds)
            {
                var normalized = TagIdUtility.Normalize(tagId);
                if (!string.IsNullOrEmpty(normalized))
                {
                    tags.Add(normalized);
                }
            }

            return tags;
        }
    }
}
