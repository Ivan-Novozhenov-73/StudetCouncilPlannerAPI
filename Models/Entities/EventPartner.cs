namespace StudetCouncilPlannerAPI.Models.Entities
{

    public class EventPartner
    {
        public Guid PartnerId { get; set; }
        public Partner Partner { get; set; }

        public Guid EventId { get; set; }
        public Event Event { get; set; }
    }
}