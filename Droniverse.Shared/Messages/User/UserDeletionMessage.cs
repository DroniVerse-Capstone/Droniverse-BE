namespace Droniverse.Shared.Messages.User;

public record UserDeletionMessage(Guid UserId, string? Username)
{
}

