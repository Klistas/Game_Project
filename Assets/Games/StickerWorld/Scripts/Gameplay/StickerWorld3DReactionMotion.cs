using GamePrototype.StickerWorld.Core;
using UnityEngine;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DReactionMotion : MonoBehaviour
    {
        [SerializeField] private bool powerOffConfigured;
        [SerializeField] private Vector3 powerOffOffset;
        [SerializeField] private Vector3 powerOffEuler;
        [SerializeField] private Vector3 powerOffScale = Vector3.one;
        [SerializeField] private bool sleepConfigured;
        [SerializeField] private Vector3 sleepOffset;
        [SerializeField] private Vector3 sleepEuler;
        [SerializeField] private Vector3 sleepScale = Vector3.one;
        [SerializeField] private bool explodeConfigured;
        [SerializeField] private Vector3 explodeOffset;
        [SerializeField] private Vector3 explodeEuler;
        [SerializeField] private Vector3 explodeScale = Vector3.one;
        [SerializeField] private bool passThroughConfigured;
        [SerializeField] private Vector3 passThroughOffset;
        [SerializeField] private Vector3 passThroughEuler;
        [SerializeField] private Vector3 passThroughScale = Vector3.one;
        [SerializeField] private bool royalConfigured;
        [SerializeField] private Vector3 royalOffset;
        [SerializeField] private Vector3 royalEuler;
        [SerializeField] private Vector3 royalScale = Vector3.one;
        [SerializeField] private bool noiseConfigured;
        [SerializeField] private Vector3 noiseOffset;
        [SerializeField] private Vector3 noiseEuler;
        [SerializeField] private Vector3 noiseScale = Vector3.one;

        private Vector3 baseLocalPosition;
        private Vector3 baseLocalEuler;
        private Vector3 baseLocalScale;
        private bool cached;

        public void ConfigurePowerOff(Vector3 offset, Vector3 euler, Vector3 scale)
        {
            powerOffConfigured = true;
            powerOffOffset = offset;
            powerOffEuler = euler;
            powerOffScale = NonZero(scale);
            CacheBaseState();
        }

        public void ConfigureSleep(Vector3 offset, Vector3 euler, Vector3 scale)
        {
            sleepConfigured = true;
            sleepOffset = offset;
            sleepEuler = euler;
            sleepScale = NonZero(scale);
            CacheBaseState();
        }

        public void ConfigureExplode(Vector3 offset, Vector3 euler, Vector3 scale)
        {
            explodeConfigured = true;
            explodeOffset = offset;
            explodeEuler = euler;
            explodeScale = NonZero(scale);
            CacheBaseState();
        }

        public void ConfigurePassThrough(Vector3 offset, Vector3 euler, Vector3 scale)
        {
            passThroughConfigured = true;
            passThroughOffset = offset;
            passThroughEuler = euler;
            passThroughScale = NonZero(scale);
            CacheBaseState();
        }

        public void ConfigureRoyal(Vector3 offset, Vector3 euler, Vector3 scale)
        {
            royalConfigured = true;
            royalOffset = offset;
            royalEuler = euler;
            royalScale = NonZero(scale);
            CacheBaseState();
        }

        public void ConfigureNoise(Vector3 offset, Vector3 euler, Vector3 scale)
        {
            noiseConfigured = true;
            noiseOffset = offset;
            noiseEuler = euler;
            noiseScale = NonZero(scale);
            CacheBaseState();
        }

        public bool Apply(ReactionId reaction, float value)
        {
            EnsureCached();
            switch (reaction)
            {
                case ReactionId.PowerOff:
                    if (!powerOffConfigured)
                    {
                        return false;
                    }

                    ApplyPose(powerOffOffset, powerOffEuler, powerOffScale);
                    return true;
                case ReactionId.Sleep:
                    if (!sleepConfigured)
                    {
                        return false;
                    }

                    ApplyPose(sleepOffset, sleepEuler, sleepScale);
                    return true;
                case ReactionId.Explode:
                    if (!explodeConfigured)
                    {
                        return false;
                    }

                    ApplyPose(explodeOffset, explodeEuler, explodeScale);
                    return true;
                case ReactionId.Resize:
                    float multiplier = value <= 0f ? 0.55f : value;
                    ApplyPose(Vector3.zero, Vector3.zero, Vector3.one * multiplier);
                    return true;
                case ReactionId.PassThrough:
                    if (!passThroughConfigured)
                    {
                        return false;
                    }

                    ApplyPose(passThroughOffset, passThroughEuler, passThroughScale);
                    return true;
                case ReactionId.Bow:
                    if (!royalConfigured)
                    {
                        return false;
                    }

                    ApplyPose(royalOffset, royalEuler, royalScale);
                    return true;
                case ReactionId.MakeNoise:
                case ReactionId.Attract:
                    if (!noiseConfigured)
                    {
                        return false;
                    }

                    ApplyPose(noiseOffset, noiseEuler, noiseScale);
                    return true;
                default:
                    return false;
            }
        }

        private void Awake()
        {
            CacheBaseState();
        }

        private void CacheBaseState()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalEuler = transform.localEulerAngles;
            baseLocalScale = transform.localScale;
            cached = true;
        }

        private void EnsureCached()
        {
            if (!cached)
            {
                CacheBaseState();
            }
        }

        private void ApplyPose(Vector3 offset, Vector3 euler, Vector3 scaleMultiplier)
        {
            transform.localPosition = baseLocalPosition + offset;
            transform.localEulerAngles = baseLocalEuler + euler;
            transform.localScale = Vector3.Scale(baseLocalScale, NonZero(scaleMultiplier));
        }

        private static Vector3 NonZero(Vector3 value)
        {
            return new Vector3(
                Mathf.Approximately(value.x, 0f) ? 1f : value.x,
                Mathf.Approximately(value.y, 0f) ? 1f : value.y,
                Mathf.Approximately(value.z, 0f) ? 1f : value.z);
        }
    }
}
