namespace Droniverse.Community.API.Examples;

using Swashbuckle.AspNetCore.Filters;

public class WalletRequestExample : IMultipleExamplesProvider<WalletRequestDto>
{
    public IEnumerable<SwaggerExample<WalletRequestDto>> GetExamples()
    {
        yield return new SwaggerExample<WalletRequestDto>
        {
            Name = "Vietcombank",
            Value = new WalletRequestDto
            {
                Bank = "Vietcombank",
                BankNumber = "0123456789"
            }
        };

        yield return new SwaggerExample<WalletRequestDto>
        {
            Name = "Techcombank",
            Value = new WalletRequestDto
            {
                Bank = "Techcombank",
                BankNumber = "0987654321"
            }
        };

        yield return new SwaggerExample<WalletRequestDto>
        {
            Name = "BIDV",
            Value = new WalletRequestDto
            {
                Bank = "BIDV",
                BankNumber = "1234567890"
            }
        };
    }
}
