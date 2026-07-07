using UnityEngine;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>프로토용 3D TextMesh 생성 헬퍼 (캔버스 불필요).</summary>
    public static class TextMeshFactory
    {
        public static TextMesh Create(Transform parent, string name, string text,
            int fontSize, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = fontSize;
            tm.characterSize = 0.1f;
            tm.color = color;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            go.GetComponent<MeshRenderer>().material = tm.font.material;

            return tm;
        }
    }
}
