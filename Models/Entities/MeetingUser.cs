public class MeetingUser
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; }

    public short Role { get; set; }
}