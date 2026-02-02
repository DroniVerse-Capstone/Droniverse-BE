namespace Droniverse.Shared.Messages.User;

public record UserNameUpdateMessage(Guid UserId, string? NewUserName)
{
}

