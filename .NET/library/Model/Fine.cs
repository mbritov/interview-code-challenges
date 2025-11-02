namespace OneBeyondApi.Model
{
    public class Fine
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BorrowerId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = false;
    }
}