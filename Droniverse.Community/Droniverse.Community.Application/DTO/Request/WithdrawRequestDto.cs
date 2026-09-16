
public record WithdrawRequestDto
{
    public decimal Amount { get; init; }
    public string Note { get; init; }
}