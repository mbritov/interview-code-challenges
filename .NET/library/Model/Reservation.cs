namespace OneBeyondApi.Model
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BorrowerId { get; set; }
        public Guid BookId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
