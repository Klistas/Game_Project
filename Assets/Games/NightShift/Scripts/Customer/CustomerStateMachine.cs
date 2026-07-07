using System;
using GamePrototype.NightShift.Core;
using GamePrototype.NightShift.Data;
using UnityEngine;

namespace GamePrototype.NightShift.Customer
{
    public enum CustomerState
    {
        Idle,       // 미등장
        Entering,   // 입장 (자동문 → 진열대)
        Browsing,   // 진열대 배회
        AtCounter,  // 카운터 도착, 응대 대기
        Serving,    // 플레이어가 절차 수행 중
        Leaving,    // 정상 퇴장
        Turning     // 돌변 (오판 시)
    }

    /// <summary>
    /// 손님 1명의 상태머신 (GDD Phase 1). 행동/판정은 전부 SO 데이터 참조.
    /// 시간 기반 자동 전이 + 플레이어 절차 입력(Serve)에 반응.
    /// </summary>
    public class CustomerStateMachine : MonoBehaviour
    {
        public CustomerSO data;

        [Header("타이밍 (초)")]
        public float enterDuration = 1.2f;
        public float browseDuration = 2.0f;
        public float leaveDuration = 1.2f;
        public float turnDuration = 1.0f;

        public CustomerState State { get; private set; } = CustomerState.Idle;
        public Verdict LastVerdict { get; private set; }

        public event Action<CustomerState, CustomerState> StateChanged;
        public event Action<Verdict, string> Served;
        /// <summary>퇴장/돌변 완료 → 다음 손님 진행 신호.</summary>
        public event Action<CustomerStateMachine> Finished;

        private float _timer;
        private bool _served;

        public void Begin(CustomerSO customer)
        {
            data = customer;
            _served = false;
            SetState(CustomerState.Entering);
        }

        private void Update()
        {
            if (State == CustomerState.Idle) return;
            _timer -= Time.deltaTime;
            if (_timer > 0) return;

            switch (State)
            {
                case CustomerState.Entering: SetState(CustomerState.Browsing); break;
                case CustomerState.Browsing: SetState(CustomerState.AtCounter); break;
                case CustomerState.AtCounter: break; // 플레이어 입력 대기 (자동 전이 없음)
                case CustomerState.Serving: break;   // Serve()가 결과로 전이
                case CustomerState.Leaving:
                case CustomerState.Turning:
                    SetState(CustomerState.Idle);
                    Finished?.Invoke(this);
                    break;
            }
        }

        /// <summary>플레이어가 절차를 선택. AtCounter 상태에서만 유효.</summary>
        public Verdict Serve(ServeProcedure chosen)
        {
            if (State != CustomerState.AtCounter || _served) return Verdict.Warning;
            _served = true;
            SetState(CustomerState.Serving);

            var verdict = ServeJudge.Evaluate(data, chosen, out var reason);
            LastVerdict = verdict;
            Served?.Invoke(verdict, reason);
            EventBus.Publish(new ServeVerdictEvent(data != null ? data.id : "?", (int)verdict, reason));

            SetState(verdict == Verdict.Safe ? CustomerState.Leaving : CustomerState.Turning);
            return verdict;
        }

        private void SetState(CustomerState next)
        {
            var prev = State;
            State = next;
            _timer = next switch
            {
                CustomerState.Entering => enterDuration,
                CustomerState.Browsing => browseDuration,
                CustomerState.Leaving => leaveDuration,
                CustomerState.Turning => turnDuration,
                _ => 0f
            };

            if (prev != next)
            {
                StateChanged?.Invoke(prev, next);
                EventBus.Publish(new CustomerStateChangedEvent(
                    data != null ? data.id : "?", prev.ToString(), next.ToString()));
            }
        }
    }
}
