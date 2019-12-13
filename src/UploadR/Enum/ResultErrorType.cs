namespace UploadR.Enum
{
    public enum ResultErrorType
    {
        NotFound = 1,
        Removed = 2,
        NotFoundRemoved = 3,
        Unauthorized = 4,

        Null = 11,
        TooBig = 12,
        TooSmall = 13,
        UnsupportedFileExtension = 14,

        InvalidGuid = 20,

        Found = 30
    }
}
