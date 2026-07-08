using UnityEngine;

namespace GamePrototype.StickerWorld.Data
{
    [CreateAssetMenu(menuName = "Sticker World/Tag", fileName = "Tag_")]
    public sealed class TagSO : ScriptableObject
    {
        public string id;
        public string displayName;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id.Trim();
    }
}
