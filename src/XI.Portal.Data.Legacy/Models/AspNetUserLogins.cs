namespace XI.Portal.Data.Legacy.Models
{
    public class AspNetUserLogins
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string UserId { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}