namespace Todo.Domain.Entities
{
    public class BlacklistedToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
