namespace Todo.Application.Interfaces.Background
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
