namespace GamePrototype.NightShift.Data
{
    /// <summary>응대 절차 단계 (GDD 2.3). 괴이별로 특정 단계를 다르게 해야 함.</summary>
    public enum ServeProcedure
    {
        NormalServe,   // 정상 응대 (스캔→계산→봉투)
        CashByTray,    // 거스름돈을 트레이로
        CashByHand,    // 거스름돈을 손으로
        RequestId,     // 신분증 요구
        Refuse,        // 거부·퇴거
        Hide,          // 숨기 (창고/카운터 아래)
        Salt           // 소금 뿌리기 (특수 퇴마)
    }

    /// <summary>단서를 관찰하는 도구 (GDD 2.1 / Phase 2 확장).</summary>
    public enum ObservationTool
    {
        Counter,       // 카운터 정면
        CCTV,
        Mirror,        // 거울
        Thermometer    // 온도계
    }
}
