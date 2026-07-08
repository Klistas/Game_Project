namespace GamePrototype.StickerWorld.Core
{
    public readonly struct ResolvedReaction
    {
        public readonly string RuleId;
        public readonly int Priority;
        public readonly RuleEffect Effect;

        public ResolvedReaction(string ruleId, int priority, RuleEffect effect)
        {
            RuleId = ruleId;
            Priority = priority;
            Effect = effect;
        }
    }
}
