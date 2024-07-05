namespace DoAnCoSoTL.Models
{
    public class SeatBookingCart
    {
        public List<SeatCartItem> Items { get; set; } = new List<SeatCartItem>();
        public void AddItem(SeatCartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.SeatId ==
            item.SeatId);

            Items.Add(item);
        }
        public void RemoveItem(int seatId)
        {
            Items.RemoveAll(i => i.SeatId == seatId);
        }
    }
}
