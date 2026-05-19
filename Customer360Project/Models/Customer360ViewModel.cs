namespace Customer360Clean.Models
{
    public class Customer360ViewModel
    {
        public Customer? Customer { get; set; }
        public List<CustomerActivity> Activities { get; set; } = new();
        public List<Ticket> Tickets { get; set; } = new();
        public bool IsVIP { get; set; }
    }
}
