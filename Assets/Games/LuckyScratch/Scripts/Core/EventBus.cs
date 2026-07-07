using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Core
{
    /// <summary>
    /// 타입 기반 정적 이벤트 버스. 이벤트는 struct로 정의한다.
    /// 사용: EventBus.Subscribe&lt;GoldChangedEvent&gt;(OnGold); EventBus.Publish(new GoldChangedEvent(...));
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            var type = typeof(T);
            if (!Handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                Handlers[type] = list;
            }
            if (!list.Contains(handler)) list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            if (Handlers.TryGetValue(typeof(T), out var list)) list.Remove(handler);
        }

        public static void Publish<T>(T evt) where T : struct
        {
            if (!Handlers.TryGetValue(typeof(T), out var list)) return;
            // 핸들러 내부에서 구독/해지가 일어나도 안전하도록 스냅샷 순회
            var snapshot = list.ToArray();
            foreach (var d in snapshot)
            {
                try { ((Action<T>)d)?.Invoke(evt); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        /// <summary>씬 리로드/테스트 간 상태 누수 방지용.</summary>
        public static void Clear() => Handlers.Clear();
    }

    // ---- Phase 0 기본 이벤트 정의 (경제 루프에서 확장) ----

    public readonly struct GoldChangedEvent
    {
        public readonly double Previous;
        public readonly double Current;
        public GoldChangedEvent(double previous, double current) { Previous = previous; Current = current; }
    }

    public readonly struct TicketScratchedEvent
    {
        public readonly string TierId;
        public readonly double Payout;
        public readonly int WinGrade; // 0=꽝, 1=소액, 2=중액, 3=잭팟
        public TicketScratchedEvent(string tierId, double payout, int winGrade)
        {
            TierId = tierId; Payout = payout; WinGrade = winGrade;
        }
    }
}
