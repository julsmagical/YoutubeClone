using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YoutubeClone.Application.Models.Requests.Auth
{
    public class RenewAuthRequest
    {
        [Required]
        [Description("Token que se usa para renovar la sesión. Se consigue al iniciar sesión en el aplicativo.")]
        public string RefreshToken { get; set; } = null!;
    }
}
