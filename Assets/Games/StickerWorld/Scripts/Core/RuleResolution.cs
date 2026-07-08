using System.Collections.Generic;

namespace GamePrototype.StickerWorld.Core
{
    public sealed class RuleResolution
    {
        public RuleResolution(IReadOnlyList<ResolvedReaction> reactions, bool stopRequested)
        {
            Reactions = reactions;
            StopRequested = stopRequested;
        }

        public IReadOnlyList<ResolvedReaction> Reactions { get; }
        public bool StopRequested { get; }
        public bool HasReactions => Reactions.Count > 0;
    }
}
