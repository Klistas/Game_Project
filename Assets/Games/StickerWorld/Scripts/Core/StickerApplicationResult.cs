using System.Collections.Generic;

namespace GamePrototype.StickerWorld.Core
{
    public sealed class StickerApplicationResult
    {
        public StickerApplicationResult(IReadOnlyCollection<string> tagIds, RuleResolution resolution)
        {
            TagIds = tagIds;
            Resolution = resolution;
        }

        public IReadOnlyCollection<string> TagIds { get; }
        public RuleResolution Resolution { get; }
    }
}
