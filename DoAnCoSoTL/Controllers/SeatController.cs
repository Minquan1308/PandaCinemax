using DoAnCoSoTL.Extensions;
using DoAnCoSoTL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DoAnCoSoTL.Controllers
{
    public class SeatController : Controller
    {
        private readonly MovieContext _context;

        public SeatController(MovieContext context)
        {
            _context = context;
        }

        // GET: Seat/Choose

        public IActionResult Choose(int roomId, int screeningId, string timeSlot, Guid movieId)
        {
            if (screeningId == 0 || roomId == 0 || movieId == Guid.Empty)
            {
                // Log hoặc xử lý lỗi ở đây
                return BadRequest("Invalid parameters.");
            }
            var room = _context.Rooms.FirstOrDefault(x => x.Id == roomId);
            ViewBag.Row = room.soHang;
            ViewBag.Number = room.soCot;

            var seats = GenerateSeatsForScreening(screeningId, room.soHang, room.soCot, roomId);

            var cart = HttpContext.Session.GetObjectFromJson<SeatBookingCart>("Cart") ?? new SeatBookingCart();
            var selectedSeatIds = cart.Items.Select(item => item.SeatId).ToList();
            var movie = _context.Movies.SingleOrDefault(x => x.Id == movieId);

            ViewData["RoomId"] = roomId;
            ViewData["ScreeningId"] = screeningId;
            ViewData["TimeSlot"] = timeSlot;
            ViewData["MovieName"] = movie.Name;
            ViewData["MovieId"] = movieId;
            ViewData["SelectedSeatIds"] = selectedSeatIds;
            ViewBag.TicketPrice = movie.Price;



            return View(seats); // Pass the seats list to the view
        }



        // Phương thức để tạo danh sách các ghế cho một khung giờ chiếu và lưu chúng vào cơ sở dữ liệu
        private List<Seat> GenerateSeatsForScreening(int screeningId, int hang, int cot, int roomId)
        {
            // Lấy thông tin về khung giờ chiếu để tạo các ghế
            var screening = _context.Screenings
                .Include(s => s.Room)
                .FirstOrDefault(s => s.Id == screeningId);

            if (screening == null)
            {
                return new List<Seat>(); // Trả về danh sách trống nếu không tìm thấy khung giờ chiếu
            }

            // Kiểm tra xem đã tạo ghế cho khung giờ chiếu này trước đó chưa
            var existingSeats = _context.Seats.Any(s => s.ScreeningId == screeningId);

            // Nếu đã tồn tại ghế cho khung giờ chiếu này, chỉ cần trả về danh sách các ghế từ cơ sở dữ liệu
            if (existingSeats)
            {
                var seatsFromDatabase = _context.Seats.Where(s => s.ScreeningId == screeningId).ToList();
                return seatsFromDatabase;
            }
            // Nếu chưa có ghế được tạo cho khung giờ chiếu này, tạo mới danh sách ghế và lưu vào cơ sở dữ liệu
            var seats = new List<Seat>();
            for (int row = 1; row <= hang; row++)
            {
                for (int seatNumber = 1; seatNumber <= cot; seatNumber++)
                {
                    // Tạo mã ghế
                    string seatCode = $"{(char)('A' + row - 1)}{seatNumber}";

                    // Tạo đối tượng ghế mới và thêm vào danh sách
                    var seat = new Seat
                    {
                        SeatCode = seatCode,
                        Row = row.ToString(),
                        Number = seatNumber,
                        IsAvailable = true, // Mặc định là ghế trống
                        //CinemaName = screening.Room.Cinema.Name,
                        ScreeningId = screeningId,
                        RoomId = roomId

                    };

                    seats.Add(seat);
                }
            }

            // Lưu danh sách các ghế vào cơ sở dữ liệu
            _context.Seats.AddRange(seats);
            _context.SaveChanges();

            return seats;
        }


        // POST: Seat/BookTicket
        [HttpPost]
        public async Task<IActionResult> BookTicket(int roomId, int screeningId, string timeSlot, List<string> selectedSeatIds, Guid movieId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null)
            {
                return NotFound();
            }
            double ticketPrice = screening.Movie.Price;
            double totalPrice = ticketPrice * selectedSeatIds.Count;

            return RedirectToAction("Index", "SeatBookingCart", new { roomId = roomId, screeningId = screeningId, timeSlot = timeSlot, selectedSeatIds = string.Join(",", selectedSeatIds), movieId = movieId });
        }





        // GET: Seat/TicketDetails
        public IActionResult TicketDetails(int screeningId, string timeSlot, string selectedSeatIds, double totalPrice)
        {
            var screening = _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .FirstOrDefault(s => s.Id == screeningId);

            if (screening == null)
            {
                return NotFound();
            }

            ViewBag.MovieName = screening.Movie.Name;
            ViewBag.TimeSlot = timeSlot;
            ViewBag.CinemaName = screening.Cinema.Name;
            ViewBag.CinemaLocation = screening.Cinema.Location;

            // Kiểm tra nếu selectedSeatIds không phải null trước khi sử dụng Split(',')
            ViewBag.SelectedSeatIds = selectedSeatIds != null ? selectedSeatIds.Split(',') : new string[0];

            ViewBag.TotalPrice = totalPrice;
            return View();
        }


    }
}