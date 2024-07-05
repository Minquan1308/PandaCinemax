namespace DoAnCoSoTL.ViewModels
{
    public class OrderHistoryViewModel
    {
        public Guid OrderId { get; set; }
        public string CinemaName { get; set; }
        public string CinemaImage { get; set; }
        public string MovieName { get; set; }
        public DateTime OrderDate { get; set; }
        public string TimeSlot { get; set; }
        public decimal Price { get; set; }
    }

}
