namespace BlazorCodeBase.Server.Database.Interface
{
    public interface IModified
    {
        public string? UserModified { get; set; }

        public DateTime? DateModified { get; set; }
    }
}
