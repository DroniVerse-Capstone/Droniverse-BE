using System.Runtime.Serialization;

namespace Droniverse.Identity.Domain.Enums;

public enum NotificationType
{
    [EnumMember(Value = "EMAIL")]
    EMAIL,
    [EnumMember(Value = "SMS")]
    SMS,
    [EnumMember(Value = "IN_APP")]
    IN_APP,
    [EnumMember(Value = "PUSH")]
    PUSH
}
