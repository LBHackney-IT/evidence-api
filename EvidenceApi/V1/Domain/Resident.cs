// TODO: This model should be replaced by an external data source
namespace EvidenceApi.V1.Domain
{
    public class Resident
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
