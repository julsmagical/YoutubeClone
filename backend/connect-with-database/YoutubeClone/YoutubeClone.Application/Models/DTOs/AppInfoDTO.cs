namespace YoutubeClone.Application.Models.DTOs
{
    public class AppInfoDTO
    {
        public string Version { get; set; } = null!;
        public List<RoleDTO> Roles { get; set; } = [];
    }
}
