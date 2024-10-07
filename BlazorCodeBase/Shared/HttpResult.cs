namespace BlazorCodeBase.Shared
{
    public class HttpResult<T>
    {
        public bool IsSuccess => Errors.Count == 0;

        public List<string> Errors { get; set; } = [];

        public T Data { get; set; }

        public HttpResult<T> AddErrors(params string[] error)
        {
            Errors.AddRange(error);
            return this;
        }

        public HttpResult<T> AddData(T data)
        {
            Data = data;
            return this;
        }
    }

    public class HttpResult : HttpResult<object> { }
}
