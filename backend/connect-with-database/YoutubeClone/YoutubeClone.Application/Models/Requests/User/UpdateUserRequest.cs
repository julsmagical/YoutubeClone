using System.ComponentModel.DataAnnotations;
using YoutubeClone.Shared.Constants;

namespace YoutubeClone.Application.Models.Requests.User
{
    public class UpdateUserRequest
    {
        [MaxLength(20, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(5, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        public string? UserName { get; set; }

        [MaxLength(50, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(3, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        public string? DisplayName { get; set; }

        [MaxLength(255, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(10, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        [EmailAddress(ErrorMessage = ValidationConstants.EMAIL)]
        public string? Email { get; set; }

        public DateTime? Birthday { get; set; }

        [MaxLength(30, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(3, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        public string? Location { get; set; }
    }
}
