using System.ComponentModel.DataAnnotations;
using YoutubeClone.Shared.Constants;

namespace YoutubeClone.Application.Models.Requests.Auth.Register
{
    public class RegisterInitAuthRequest
    {
        [Required(ErrorMessage = ValidationConstants.REQUIRED)]
        [EmailAddress(ErrorMessage = ValidationConstants.EMAIL)]
        [MaxLength(255, ErrorMessage = ValidationConstants.REQUIRED)]
        [MinLength(10, ErrorMessage = ValidationConstants.REQUIRED)]
        public string EmailAdress { get; set; }
    }
}
