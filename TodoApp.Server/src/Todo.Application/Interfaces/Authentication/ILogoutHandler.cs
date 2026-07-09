namespace Todo.Application.Interfaces.Authentication
{
    public interface ILogoutHandler
    {
        Task HandleAsync(string refreshToken, string accessToken);
    }
}
