using System.Runtime.Serialization;

namespace Droniverse.Identity.Domain.Enums;

public enum NotificationStatus
{
    [EnumMember(Value = "PENDING")]
    PENDING,
    [EnumMember(Value = "SENT")]
    SENT,
    [EnumMember(Value = "FAILED")]
    FAILED,
    [EnumMember(Value = "READ")]
    READ
}
