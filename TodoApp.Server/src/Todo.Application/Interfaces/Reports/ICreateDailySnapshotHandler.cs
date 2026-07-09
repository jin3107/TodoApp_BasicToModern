using MayNghien.Infrastructures.Models.Responses;

namespace Todo.Application.Interfaces.Reports
{
    public interface ICreateDailySnapshotHandler
    {
        Task<AppResponse<Guid>> HandleAsync();
    }
}
