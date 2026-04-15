namespace Droniverse.Shared.DTOs.Request
{
    public class GetAllCodesByClubSearchRequest : SearchRequest
    {
        public CodeState? CodeUseState { get; set; } = Request.CodeState.Used;
        public CodeOwnState? CodeOwnState { get; set; } = Request.CodeOwnState.UserOwned;
    }

    public enum CodeState
    {
        UnUse,
        Used
    }

    public enum CodeOwnState
    {
        UnUserOwned,
        UserOwned
    }
}
