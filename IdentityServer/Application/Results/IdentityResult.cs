namespace IdentityServer.Application.Results
{
    public class IdentityResult<T>
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; } 
        public string? Error { get; set; }  

        public static IdentityResult<T> Success(T data) => new IdentityResult<T> { IsSuccess = true, Data = data };
        public static IdentityResult<T> Failure(string error) => new IdentityResult<T> { IsSuccess = false, Error = error }; 
        public IdentityResult<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            if (!IsSuccess)
                return IdentityResult<TResult>.Failure(Error);

            if (Data == null)
                return IdentityResult<TResult>.Failure("No data.");

            var mappedData = mapper(Data);
            return IdentityResult<TResult>.Success(mappedData);
        } 
    }
}
