namespace BOTS.Services.Common.Results
{
    public class ErrorResult : Result
    {
        internal ErrorResult(string errorMessage, params object[] parameters)
        {
            this.ErrorMessage = errorMessage;
            this.Parameters = parameters;
        }

        public string ErrorMessage { get; }

        public object[] Parameters { get; }

        public override string ToString()
            => string.Format(this.ErrorMessage, this.Parameters);
    }

    public class ErrorResult<T> : Result<T>
    {
        internal ErrorResult(string errorMessage, params object[] parameters)
        {
            this.ErrorMessage = errorMessage;
            this.Parameters = parameters;
        }

        public string ErrorMessage { get; }

        public object[] Parameters { get; }

        public override string ToString()
            => string.Format(this.ErrorMessage, this.Parameters);
    }
}
