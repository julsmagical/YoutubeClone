namespace YoutubeClone.Application.Models.Request.Users
{
    public class GetAllUserRequest
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string? DisplayName { get; set; }
    }
}