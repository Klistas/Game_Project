using System.Collections.Generic;
using GamePrototype.StickerWorld.Core;
using UnityEngine;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DTarget : MonoBehaviour
    {
        [SerializeField] private string targetId;
        [SerializeField] private string displayName;
        [SerializeField] private string[] baseTags;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private TextMesh label;
        [SerializeField] private TextMesh stateLabel;

        private readonly List<string> runtimeTags = new List<string>();
        private readonly List<Color> baseColors = new List<Color>();
        private Vector3 baseScale;
        private bool highlighted;

        public string TargetId => targetId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public IReadOnlyList<string> Tags => runtimeTags;

        public void Configure(string id, string labelText, string[] tags, Renderer[] targetRenderers, TextMesh nameLabel, TextMesh state)
        {
            targetId = id;
            displayName = labelText;
            baseTags = tags;
            renderers = targetRenderers;
            label = nameLabel;
            stateLabel = state;
        }

        private void Awake()
        {
            CacheBaseState();
            ResetTarget();
        }

        public void CacheBaseState()
        {
            baseScale = transform.localScale;
            baseColors.Clear();

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }

            foreach (var targetRenderer in renderers)
            {
                var material = ResolveMaterial(targetRenderer);
                if (material != null)
                {
                    baseColors.Add(material.color);
                }
            }
        }

        public void ResetTarget()
        {
            runtimeTags.Clear();
            if (baseTags != null)
            {
                foreach (var tag in baseTags)
                {
                    AddTag(tag);
                }
            }

            transform.localScale = baseScale == Vector3.zero ? Vector3.one : baseScale;
            SetState("대기");
            SetHighlight(false);

            if (label != null)
            {
                label.text = DisplayName;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || i >= baseColors.Count)
                {
                    continue;
                }

                var material = ResolveMaterial(renderers[i]);
                if (material != null)
                {
                    material.color = baseColors[i];
                }
            }
        }

        public void ReplaceTags(IEnumerable<string> tags)
        {
            runtimeTags.Clear();
            if (tags == null)
            {
                return;
            }

            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        public void AddTag(string tag)
        {
            var normalized = TagIdUtility.Normalize(tag);
            if (string.IsNullOrEmpty(normalized))
            {
                return;
            }

            foreach (var current in runtimeTags)
            {
                if (TagIdUtility.Equals(current, normalized))
                {
                    return;
                }
            }

            runtimeTags.Add(normalized);
        }

        public bool HasTag(string tag)
        {
            foreach (var current in runtimeTags)
            {
                if (TagIdUtility.Equals(current, tag))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetState(string value)
        {
            if (stateLabel != null)
            {
                stateLabel.text = value;
            }
        }

        public void Tint(Color color)
        {
            if (renderers == null)
            {
                return;
            }

            foreach (var targetRenderer in renderers)
            {
                var material = ResolveMaterial(targetRenderer);
                if (material != null)
                {
                    material.color = color;
                }
            }
        }

        public void SetTargetScale(float multiplier)
        {
            transform.localScale = (baseScale == Vector3.zero ? Vector3.one : baseScale) * multiplier;
        }

        public void SetHighlight(bool value)
        {
            highlighted = value;
            if (label != null)
            {
                label.color = highlighted ? new Color(1f, 0.86f, 0.25f) : Color.white;
            }
        }

        private static Material ResolveMaterial(Renderer targetRenderer)
        {
            if (targetRenderer == null)
            {
                return null;
            }

            return Application.isPlaying ? targetRenderer.material : targetRenderer.sharedMaterial;
        }
    }
}
