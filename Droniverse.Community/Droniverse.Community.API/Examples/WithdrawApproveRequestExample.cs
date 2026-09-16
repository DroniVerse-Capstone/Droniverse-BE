namespace Droniverse.Community.API.Examples;

using Droniverse.Community.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

public class WithdrawApproveRequestExample : IMultipleExamplesProvider<WithdrawApproveRequestDto>
{
    public IEnumerable<SwaggerExample<WithdrawApproveRequestDto>> GetExamples()
    {
        yield return new SwaggerExample<WithdrawApproveRequestDto>
        {
            Name = "Approve (duyệt)",
            Value = new WithdrawApproveRequestDto
            {
                Status = WithdrawStatus.APPROVED,
                RejectReason = null
            }
        };

        yield return new SwaggerExample<WithdrawApproveRequestDto>
        {
            Name = "Reject (từ chối)",
            Value = new WithdrawApproveRequestDto
            {
                Status = WithdrawStatus.REJECTED,
                RejectReason = "hiện tại hệ thống vẫn chưa đủ tiền. vui lòng gửi yêu cầu rút tiền vào lần sau."
            }
        };

        yield return new SwaggerExample<WithdrawApproveRequestDto>
        {
            Name = "Cancel (Hủy)",
            Value = new WithdrawApproveRequestDto
            {
                Status = WithdrawStatus.CANCELLED,
                RejectReason = null
            }
        };
    }
}
