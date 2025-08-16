namespace SAT.BE.src.SAT.BE.Application.Common
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public int StatusCode { get; set; }

        public static ServiceResult<T> Success(T data, string message = "Success")
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }

        public static ServiceResult<T> Failure(string message, int statusCode = 400)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ServiceResult<T> Failure(List<string> errors, string message = "Validation failed", int statusCode = 400)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }
    }
}