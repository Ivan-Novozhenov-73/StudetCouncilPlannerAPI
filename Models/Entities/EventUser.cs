namespace StudetCouncilPlannerAPI.Models.Entities
{

    public class EventUser
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = new();
        public Guid EventId { get; set; }
        public Event Event { get; set; } = new();
        public short Role { get; set; }
    }
}