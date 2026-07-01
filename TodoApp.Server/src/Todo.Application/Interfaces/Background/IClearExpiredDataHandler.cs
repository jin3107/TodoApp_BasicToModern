namespace Todo.Application.Interfaces.Background
{
    public interface IClearExpiredDataHandler
    {
        Task HandleAsync();
    }
}
