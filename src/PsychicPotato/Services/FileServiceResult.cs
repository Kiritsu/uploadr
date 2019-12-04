namespace PsychicPotato.Services
{
    public class FileServiceResult<T> where T : class
    {
        public bool IsSuccess { get; }
        public int Code { get; }
        public T Value { get; }
        
        public FileServiceResult(bool success, int code, T value)
        {
            IsSuccess = success;
            Code = code;
            Value = value;
        }

        public static FileServiceResult<T> Fail(int code)
        {
            return new FileServiceResult<T>(false, code, null);
        }

        public static FileServiceResult<T> Success(T value)
        {
            return new FileServiceResult<T>(true, 0, value);
        }
    }
}
