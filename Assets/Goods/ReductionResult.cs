public class ReductionResult
{
    public readonly bool IsReducing;
    public readonly float ReductionAmount;
    public ReductionResult(bool isReducing, float reductionAmount)
    {
        IsReducing = isReducing;
        ReductionAmount = reductionAmount;
    }

    public float By()=> IsReducing ? ReductionAmount : 0f;
}
