namespace Customer360Clean.Models
{
    public class CustomerActivity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string AppName { get; set; } = "";
        public string ActivityType { get; set; } = "";
        public DateTime LoginTime { get; set; } = DateTime.Now;
    }
}
