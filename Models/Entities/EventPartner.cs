namespace StudetCouncilPlannerAPI.Models.Entities
{

    public class EventPartner
    {
        public Guid PartnerId { get; set; }
        public Partner Partner { get; set; } = new();
        public Guid EventId { get; set; }
        public Event Event { get; set; } = new();
    }
}