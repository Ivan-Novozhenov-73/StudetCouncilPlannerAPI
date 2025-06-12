namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class MeetingUser
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = new();
        public Guid MeetingId { get; set; }
        public Meeting Meeting { get; set; } = new();
        public short Role { get; set; }
    }
}