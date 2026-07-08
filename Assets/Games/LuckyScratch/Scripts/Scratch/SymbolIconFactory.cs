using System.Collections.Generic;
using GamePrototype.LuckyScratch.Core;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 심볼 도형 스프라이트 절차 생성 (텍스처 에셋 0).
    /// symbolId → (도형, DisplayNames 색) 매핑 + 스프라이트 캐시.
    /// </summary>
    public static class SymbolIconFactory
    {
        private enum Shape { Circle, Diamond, Star, Triangle, Hex, Heart, Clover, Ring, Cross }

        private const int TexSize = 96;
        private static readonly Dictionary<string, Sprite> Cache = new();

        private static readonly Dictionary<string, Shape> Shapes = new()
        {
            { "blank", Shape.Cross },
            // tier1
            { "cherry", Shape.Circle }, { "clover", Shape.Clover },
            { "seven", Shape.Diamond }, { "jackpot", Shape.Star },
            // tier2
            { "rabbit", Shape.Heart }, { "fox", Shape.Triangle },
            { "tiger", Shape.Hex }, { "dragon", Shape.Star },
            // tier3
            { "coin", Shape.Circle }, { "gem", Shape.Diamond },
            { "chest", Shape.Hex }, { "goldmap", Shape.Star },
            // tier4
            { "chip", Shape.Ring }, { "dice", Shape.Diamond },
            { "crown", Shape.Triangle }, { "royal", Shape.Star },
            // tier5
            { "star", Shape.Star }, { "planet", Shape.Ring },
            { "comet", Shape.Triangle }, { "galaxy", Shape.Clover },
        };

        /// <summary>아이콘 스프라이트 + 이름 라벨을 parent 아래 생성. size = 아이콘 월드 한 변.</summary>
        public static GameObject Create(Transform parent, string symbolId, float size, Color labelColor)
        {
            var root = new GameObject($"Symbol_{symbolId}");
            root.transform.SetParent(parent, false);

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(root.transform, false);
            var sr = iconGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite(symbolId);
            iconGo.transform.localScale = Vector3.one * size;

            var label = TextMeshFactory.Create(root.transform, "Label",
                DisplayNames.Of(symbolId), 24, labelColor, TextAnchor.MiddleCenter);
            label.transform.localPosition = new Vector3(0f, -size * 0.74f, 0f);
            label.transform.localScale = Vector3.one * 0.2f;

            return root;
        }

        private static Sprite GetSprite(string symbolId)
        {
            if (Cache.TryGetValue(symbolId, out var cached) && cached != null) return cached;

            var shape = Shapes.TryGetValue(symbolId, out var s) ? s : Shape.Circle;
            Color color = DisplayNames.ColorOf(symbolId);
            Color outline = color * 0.55f;
            outline.a = 1f;

            var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            var pixels = new Color32[TexSize * TexSize];
            for (int y = 0; y < TexSize; y++)
            {
                for (int x = 0; x < TexSize; x++)
                {
                    // 2x2 슈퍼샘플 안티에일리어싱
                    float fill = 0f, edge = 0f;
                    for (int sy = 0; sy < 2; sy++)
                    {
                        for (int sx = 0; sx < 2; sx++)
                        {
                            float px = ((x + 0.25f + sx * 0.5f) / TexSize) * 2f - 1f;
                            float py = ((y + 0.25f + sy * 0.5f) / TexSize) * 2f - 1f;
                            if (Inside(shape, px, py))
                            {
                                fill += 0.25f;
                                if (!Inside(shape, px * 1.16f, py * 1.16f)) edge += 0.25f;
                            }
                        }
                    }
                    Color c = Color.Lerp(color, outline, edge > 0f ? edge / Mathf.Max(fill, 0.01f) : 0f);
                    c.a = fill;
                    pixels[y * TexSize + x] = c;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            var sprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
                new Vector2(0.5f, 0.5f), TexSize);
            Cache[symbolId] = sprite;
            return sprite;
        }

        /// <summary>p ∈ [-1,1]² 도형 내부 판정.</summary>
        private static bool Inside(Shape shape, float x, float y)
        {
            switch (shape)
            {
                case Shape.Circle:
                    return x * x + y * y <= 0.8f * 0.8f;

                case Shape.Ring:
                {
                    float d2 = x * x + y * y;
                    return d2 <= 0.8f * 0.8f && d2 >= 0.42f * 0.42f;
                }

                case Shape.Diamond:
                    return Mathf.Abs(x) + Mathf.Abs(y) <= 0.85f;

                case Shape.Triangle:
                    return y >= -0.65f && y <= 0.78f &&
                           Mathf.Abs(x) <= (0.78f - y) * 0.58f;

                case Shape.Hex:
                    return Mathf.Max(Mathf.Abs(x) * 0.866f + Mathf.Abs(y) * 0.5f, Mathf.Abs(y)) <= 0.76f;

                case Shape.Heart:
                {
                    float hx = x * 1.25f, hy = y * 1.25f - 0.1f;
                    float q = hx * hx + hy * hy - 0.55f;
                    return q * q * q - hx * hx * hy * hy * hy <= 0f;
                }

                case Shape.Star:
                {
                    float r = Mathf.Sqrt(x * x + y * y);
                    if (r < 1e-4f) return true;
                    float angle = Mathf.Atan2(y, x) + Mathf.PI * 0.5f; // 꼭짓점 위쪽
                    float t = Mathf.Repeat(angle / (2f * Mathf.PI) * 5f, 1f);
                    float wave = Mathf.Abs(t - 0.5f) * 2f; // 꼭짓점 1 → 골 0
                    float rMax = Mathf.Lerp(0.4f, 0.9f, wave);
                    return r <= rMax;
                }

                case Shape.Clover:
                {
                    float stem = Mathf.Abs(x) <= 0.09f && y <= -0.1f && y >= -0.85f ? 1f : 0f;
                    if (stem > 0f) return true;
                    for (int i = 0; i < 3; i++)
                    {
                        float a = (90f + i * 120f) * Mathf.Deg2Rad;
                        float cx = Mathf.Cos(a) * 0.36f, cy = Mathf.Sin(a) * 0.36f + 0.08f;
                        float dx = x - cx, dy = y - cy;
                        if (dx * dx + dy * dy <= 0.36f * 0.36f) return true;
                    }
                    return false;
                }

                case Shape.Cross:
                {
                    if (x * x + y * y > 0.85f * 0.85f) return false;
                    float u = (x - y) * 0.7071f, v = (x + y) * 0.7071f;
                    return Mathf.Abs(u) <= 0.17f || Mathf.Abs(v) <= 0.17f;
                }

                default:
                    return false;
            }
        }
    }
}
