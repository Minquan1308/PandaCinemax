namespace DoAnCoSoTL.Models
{
    public class SeatCartItem
    {
        public int SeatId { get; set; }
        public Guid MovieId { get; set; }
        public int RoomId { get; set; }
        public int ScreeningId { get; set; }
        public string MovieName { get; set; }
        public string CinemaName { get; set; }
        public string CinemaLocation { get; set; }
        public string TimeSlot { get; set; }
        public string SeatCode { get; set; }
        public decimal Price { get; set; }
        public int Quantity = 1;
    }
}
