using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePrototype.NightShift.Core
{
    /// <summary>타입 기반 정적 이벤트 버스. 이벤트는 struct로 정의.</summary>
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
            foreach (var d in list.ToArray())
            {
                try { ((Action<T>)d)?.Invoke(evt); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        public static void Clear() => Handlers.Clear();
    }

    // ---- Phase 0/1 기본 이벤트 ----

    public readonly struct CustomerStateChangedEvent
    {
        public readonly string CustomerId;
        public readonly string From;
        public readonly string To;
        public CustomerStateChangedEvent(string id, string from, string to)
        { CustomerId = id; From = from; To = to; }
    }

    /// <summary>응대 판정 결과. verdict: 0=무사, 1=경고, 2=사망</summary>
    public readonly struct ServeVerdictEvent
    {
        public readonly string CustomerId;
        public readonly int Verdict;
        public readonly string Reason;
        public ServeVerdictEvent(string id, int verdict, string reason)
        { CustomerId = id; Verdict = verdict; Reason = reason; }
    }
}
