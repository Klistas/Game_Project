using UnityEngine;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 긁기 존 1개: 포일 + ScratchSurface + 심볼 루트.
    /// 티어 룰(multi_area/multiplier/chain)에 따라 TicketController가 존을 켜고 끄며 내용물을 채운다.
    /// </summary>
    public class ScratchZone : MonoBehaviour
    {
        public ScratchSurface surface;
        public Renderer foilRenderer;
        public Renderer backgroundRenderer;
        public Transform symbolRoot;
        [Tooltip("UV 매핑 기준 평면 (포일 쿼드, 로컬 ±0.5)")]
        public Transform plane;

        public bool IsRevealed => surface != null && surface.IsCompleted;

        /// <summary>존 콘텐츠 폭/높이 (포일 쿼드 스케일 기준).</summary>
        public Vector2 Size => plane != null
            ? new Vector2(plane.localScale.x, plane.localScale.y)
            : Vector2.one;

        public void SetVisible(bool on) => gameObject.SetActive(on);

        public void ResetZone()
        {
            ClearSymbols();
            if (surface != null) surface.ResetSurface();
        }

        public void ClearSymbols()
        {
            if (symbolRoot == null) return;
            for (int i = symbolRoot.childCount - 1; i >= 0; i--)
                Destroy(symbolRoot.GetChild(i).gameObject);
        }

        public void SetBackgroundColor(Color color)
        {
            if (backgroundRenderer != null)
                backgroundRenderer.material.SetColor("_BaseColor", color);
        }
    }
}
