namespace YoutubeClone.Application.Models.Requests.User
{
    public class FilterUserRequest : BaseRequest
    {
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Country { get; set; }
    }
}
