namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class TaskUser
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = new();
        public Guid TaskId { get; set; }
        public Task Task { get; set; } = new() { Title = string.Empty, };
        public short Role { get; set; }
    }
}