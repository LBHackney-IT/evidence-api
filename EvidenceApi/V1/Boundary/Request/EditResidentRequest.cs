#nullable enable annotations
namespace EvidenceApi.V1.Boundary.Request
{
    public class EditResidentRequest
    {
        public string? Name { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? PhoneNumber { get; set; } = null;
    }
}
