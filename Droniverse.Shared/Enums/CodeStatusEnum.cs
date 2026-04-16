namespace Droniverse.Shared.Enums
{
    public enum CodeStatusEnum
    {
        Active = 1,   // code hợp lệ, chưa dùng
        Used = 2,     // đã sử dụng thành công
        Expired = 3,  // hết hạn
        Disabled = 4  // bị vô hiệu hóa (admin khóa)
    }
}
