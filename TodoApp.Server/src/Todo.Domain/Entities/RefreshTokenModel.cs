namespace Todo.Domain.Entities
{
    public class RefreshTokenModel : BaseEntity
    {
        public Guid? UserId { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool IsRevoked { get; set; }
    }
}
