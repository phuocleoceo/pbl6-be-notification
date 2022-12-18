namespace Monolithic.Models.DTO;

public class BookingNotificationDTO
{
    public int OriginUserId { get; set; }
    public int PostId { get; set; }
    public int BookingId { get; set; }
    public DateTime BookingTime { get; set; }
}