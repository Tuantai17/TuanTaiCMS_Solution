namespace CMS.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public virtual ICollection<ProductReviewReply> ProductReviewReplies { get; set; } = new List<ProductReviewReply>();
    }
}
