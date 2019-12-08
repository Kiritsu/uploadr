namespace UploadR.Services
{
    /// <summary>
    /// Files:
    ///     1: Not Found
    ///     2: Removed
    ///     3: Not Found then Removed
    ///     4: Unauthorized
    ///     
    ///     11: Null
    ///     12: Too big
    ///     13: Too small
    ///     14: Unsupported file extension
    /// </summary>
    public class ServiceResult<T> 
    {
        public bool IsSuccess { get; }
        public int Code { get; }
        public T Value { get; }
        
        public ServiceResult(bool success, int code)
        {
            IsSuccess = success;
            Code = code;
        }

        public ServiceResult(bool success, int code, T value)
        {
            IsSuccess = success;
            Code = code;
            Value = value;
        }

        public static ServiceResult<T> Fail(int code)
        {
            return new ServiceResult<T>(false, code);
        }

        public static ServiceResult<T> Success(T value)
        {
            return new ServiceResult<T>(true, 0, value);
        }
    }
}
