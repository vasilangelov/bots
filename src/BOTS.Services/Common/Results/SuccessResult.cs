namespace BOTS.Services.Common.Results
{
    public class SuccessResult : Result
    {
        internal SuccessResult() { }
    }

    public class SuccessResult<T> : Result<T>
    {
        internal SuccessResult(T value)
        {
            this.Value = value;
        }

        public T Value { get; }
    }
}
