namespace SchoolApp.Models
{
    public abstract class BaseEntity
    {
        public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;   // Soft delete
        public DateTime? DeletedAt { get; set; }

        /*
         * Στο update του service layer θα πρέπει να γίνεται:
         * ModifiedAt = DateTime.UtcNow
         */
    }
}
