using System;

namespace GamePrototype.StickerWorld.Core
{
    [Serializable]
    public struct RuleEffect
    {
        public ReactionId reaction;
        public string targetTagId;
        public float value;
        public string message;
    }
}
