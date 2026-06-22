namespace Todo.Domain
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public void SetCreatedInfo(string? CreatedBy)
        {
            this.CreatedBy = CreatedBy;
            CreatedOn = DateTime.UtcNow;
        }

        public void SetModifiedInfo(string? ModifiedBy)
        {
            this.ModifiedBy = ModifiedBy;
            ModifiedOn = DateTime.UtcNow;
        }
    }
}
