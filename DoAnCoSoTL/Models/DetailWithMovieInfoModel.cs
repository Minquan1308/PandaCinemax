namespace DoAnCoSoTL.Models
{
    public class DetailWithMovieInfoModel
    {
        public Guid DetailId { get; set; }
        public string MovieName { get; set; }
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserId { get; set; }
        public decimal Price { get; set; }
       
    }
}
