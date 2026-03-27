namespace Droniverse.Academy.Application.IService;

public interface IUserDisplayNameService
{
    Task<string?> ResolveUserDisplayNameAsync(Guid userId);
    Task<(string? Creator, string? Updater)> ResolveCreatorUpdaterAsync(Guid createBy, Guid updateBy);
}
