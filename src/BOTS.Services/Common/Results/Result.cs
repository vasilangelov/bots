namespace BOTS.Services.Common.Results
{
    public abstract class Result
    {
        public static ErrorResult Error(string errorMessage, params object[] paramters)
            => new(errorMessage, paramters);

        public static readonly SuccessResult Success
            = new();

        public bool IsSuccess
            => this is SuccessResult;
    }

    public abstract class Result<T>
    {
        public bool IsSuccess
            => this is SuccessResult<T>;

        public static implicit operator Result<T>(T value)
            => new SuccessResult<T>(value);

        public static implicit operator Result<T>(ErrorResult errorResult)
            => new ErrorResult<T>(errorResult.ErrorMessage, errorResult.Parameters);
    }
}
