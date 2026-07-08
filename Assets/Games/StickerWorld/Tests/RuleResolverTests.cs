using System.Linq;
using GamePrototype.StickerWorld.Core;
using GamePrototype.StickerWorld.Data;
using GamePrototype.StickerWorld.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GamePrototype.StickerWorld.Tests
{
    public class RuleResolverTests
    {
        private TagSO human;
        private TagSO machine;
        private TagSO sleepy;
        private TagSO broken;
        private TagSO royal;

        [SetUp]
        public void SetUp()
        {
            human = CreateTag("Human");
            machine = CreateTag("Machine");
            sleepy = CreateTag("Sleepy");
            broken = CreateTag("Broken");
            royal = CreateTag("Royal");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(human);
            Object.DestroyImmediate(machine);
            Object.DestroyImmediate(sleepy);
            Object.DestroyImmediate(broken);
            Object.DestroyImmediate(royal);
        }

        [Test]
        public void ApplySticker_AddsTagsAndTriggersMatchingRule()
        {
            var sticker = CreateSticker("sleepy_sticker", sleepy);
            var rule = CreateRule(
                "machine_sleep",
                10,
                new[] { machine, sleepy },
                null,
                ReactionId.PowerOff);

            var result = new StickerApplicationService().Apply(
                sticker,
                new[] { "Machine" },
                new[] { rule });

            CollectionAssert.Contains(result.TagIds.ToArray(), "Sleepy");
            Assert.AreEqual(1, result.Resolution.Reactions.Count);
            Assert.AreEqual(ReactionId.PowerOff, result.Resolution.Reactions[0].Effect.reaction);

            Object.DestroyImmediate(sticker);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void BlockedTag_PreventsRule()
        {
            var sticker = CreateSticker("royal_sticker", royal);
            var rule = CreateRule(
                "human_bows",
                10,
                new[] { human, royal },
                new[] { broken },
                ReactionId.Bow);

            var result = new StickerApplicationService().Apply(
                sticker,
                new[] { "Human", "Broken" },
                new[] { rule });

            Assert.IsFalse(result.Resolution.HasReactions);

            Object.DestroyImmediate(sticker);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void HigherPriorityStopRule_SuppressesLowerRules()
        {
            var stopRule = CreateRule("stop_rule", 100, new[] { human, royal }, null, ReactionId.Bow);
            stopRule.stopAfterMatch = true;

            var lowerRule = CreateRule("lower_rule", 1, new[] { human, royal }, null, ReactionId.Follow);
            var sticker = CreateSticker("royal_sticker", royal);

            var result = new StickerApplicationService().Apply(
                sticker,
                new[] { "Human" },
                new[] { lowerRule, stopRule });

            Assert.IsTrue(result.Resolution.StopRequested);
            Assert.AreEqual(1, result.Resolution.Reactions.Count);
            Assert.AreEqual("stop_rule", result.Resolution.Reactions[0].RuleId);
            Assert.AreEqual(ReactionId.Bow, result.Resolution.Reactions[0].Effect.reaction);

            Object.DestroyImmediate(stopRule);
            Object.DestroyImmediate(lowerRule);
            Object.DestroyImmediate(sticker);
        }

        [Test]
        public void NoMatchingRule_ReturnsNoReactionButKeepsStickerTags()
        {
            var sticker = CreateSticker("sleepy_sticker", sleepy);
            var rule = CreateRule("royal_human", 10, new[] { human, royal }, null, ReactionId.Bow);

            var result = new StickerApplicationService().Apply(
                sticker,
                new[] { "Machine" },
                new[] { rule });

            CollectionAssert.Contains(result.TagIds.ToArray(), "Sleepy");
            Assert.IsFalse(result.Resolution.HasReactions);

            Object.DestroyImmediate(sticker);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void BankGoal_CompletesWhenPlayerSmallGuardNeutralizedAndRouteOpen()
        {
            var state = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IReadOnlyCollection<string>>
            {
                { StickerWorldG0Rules.Player, new[] { "Player", "Tiny" } },
                { StickerWorldG0Rules.Guard, new[] { "Guard", "Distracted" } },
                { StickerWorldG0Rules.Cctv, new[] { "Machine", "Disabled" } },
                { StickerWorldG0Rules.VaultDoor, new[] { "Door" } },
                { StickerWorldG0Rules.ThinWall, new[] { "Wall" } }
            };

            Assert.IsTrue(StickerWorldG0Rules.IsBankGoalComplete(state));
        }

        [Test]
        public void BankGoal_DoesNotCompleteWithoutPlayerEntryTrick()
        {
            var state = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IReadOnlyCollection<string>>
            {
                { StickerWorldG0Rules.Player, new[] { "Player" } },
                { StickerWorldG0Rules.Guard, new[] { "Guard", "Distracted" } },
                { StickerWorldG0Rules.Cctv, new[] { "Machine", "Disabled" } }
            };

            Assert.IsFalse(StickerWorldG0Rules.IsBankGoalComplete(state));
        }

        [Test]
        public void BankGoal_CompletesWhenVaultDoorIsOpenedInsteadOfCctvDisabled()
        {
            var state = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IReadOnlyCollection<string>>
            {
                { StickerWorldG0Rules.Player, new[] { "Player", "Tiny" } },
                { StickerWorldG0Rules.Guard, new[] { "Guard", "Bowing" } },
                { StickerWorldG0Rules.Cctv, new[] { "Machine", "Watcher" } },
                { StickerWorldG0Rules.VaultDoor, new[] { "Door", "Open" } },
                { StickerWorldG0Rules.ThinWall, new[] { "Wall" } }
            };

            Assert.IsTrue(StickerWorldG0Rules.IsBankGoalComplete(state));
        }

        private static TagSO CreateTag(string id)
        {
            var tag = ScriptableObject.CreateInstance<TagSO>();
            tag.id = id;
            tag.displayName = id;
            return tag;
        }

        private static StickerSO CreateSticker(string id, params TagSO[] addedTags)
        {
            var sticker = ScriptableObject.CreateInstance<StickerSO>();
            sticker.id = id;
            sticker.addedTags = addedTags;
            return sticker;
        }

        private static TagRuleSO CreateRule(
            string id,
            int priority,
            TagSO[] requiredTags,
            TagSO[] blockedTags,
            ReactionId reaction)
        {
            var rule = ScriptableObject.CreateInstance<TagRuleSO>();
            rule.id = id;
            rule.eventType = RuleEvent.StickerApplied;
            rule.priority = priority;
            rule.requiredTags = requiredTags;
            rule.blockedTags = blockedTags;
            rule.effects = new[]
            {
                new RuleEffect
                {
                    reaction = reaction,
                    targetTagId = string.Empty,
                    value = 1f,
                    message = id
                }
            };
            return rule;
        }
    }
}
