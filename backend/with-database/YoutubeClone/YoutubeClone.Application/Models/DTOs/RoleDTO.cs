namespace YoutubeClone.Application.Models.DTOs
{
    public class RoleDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
