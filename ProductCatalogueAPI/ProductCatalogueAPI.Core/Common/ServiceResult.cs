namespace ProductCatalogueAPI.Core.Common
{
    public class ServiceResult
    {
        public bool IsSuccess { get; protected set; }

        public string ErrorMessage { get; protected set; } = string.Empty;

        public ErrorCode ErrorCode { get; protected set; }

        public static ServiceResult Success() => new()
        {
            IsSuccess = true
        };

        public static ServiceResult Failure(
            string message,
            ErrorCode code = ErrorCode.General) => new()
            {
                IsSuccess = false,
                ErrorMessage = message,
                ErrorCode = code
            };
    }
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; private set; }

        public static ServiceResult<T> Success(T data) => new()
        {
            IsSuccess = true,
            Data = data
        };

        public new static ServiceResult<T> Failure(
            string message,
            ErrorCode code = ErrorCode.General) => new()
            {
                IsSuccess = false,
                ErrorMessage = message,
                ErrorCode = code
            };
    }
    public enum ErrorCode
    {
        General = 0,
        NotFound = 1,
        Validation = 2,
        Conflict = 3,
        Unauthorised = 4,
        Forbidden = 5
    }
}
