
namespace MayNghien.Infrastructures.Models.Responses
{
    public class AppResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public AppResponse<T> BuildResult(T data)
        {
            Data = data;
            IsSuccess = true;
            
            return this;
        }
        public AppResponse<T> BuildResult(T data, string message)
        {
            Data = data;
            IsSuccess = true;
            Message = message;
            return this;
        }

        public AppResponse<T> BuildError(string message)
        {
            
            IsSuccess = false;
            Message = message;
            return this;
        }
    }
}
