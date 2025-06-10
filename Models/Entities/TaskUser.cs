namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class TaskUser
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid TaskId { get; set; }
        public Task Task { get; set; }

        public short Role { get; set; }
    }
}