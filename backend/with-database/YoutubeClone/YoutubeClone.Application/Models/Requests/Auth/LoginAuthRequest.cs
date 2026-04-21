using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YoutubeClone.Shared.Constants;

namespace YoutubeClone.Application.Models.Requests.Auth
{
    public class LoginAuthRequest
    {
        [Required(ErrorMessage = ValidationConstants.REQUIRED)]
        [EmailAddress(ErrorMessage = ValidationConstants.EMAIL)]
        [Description("Correo del usuario")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = ValidationConstants.REQUIRED)]
        [MaxLength(255, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(3, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        [Description("Contraseña del usuario")]
        public string Password { get; set; } = null!;
    }
}
