namespace DoAnCoSoTL.Models
{
    public class SeatOrderDetail
    {
        public Guid Id { get; set; }
        public Guid SeatOrderId { get; set; }
        public Guid MovieId { get; set; }
        public int RoomId { get; set; }
        public int ScreeningId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string TimeSlot { get; set; }
        public string CinemaName { get; set; }
        public string CinemaLocation { get; set; }
        public string SeatCode { get; set; }
        public int SeatId { get; set; }
        public SeatOrder SeatOrder { get; set; }
        //public Seat Seat { get; set; }
    }
}
