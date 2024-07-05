using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSoTL.Models
{
    public class Screening
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Date field is required.")]
        [Display(Name = "Screening Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DateRange("Movie.StartDate", "Movie.EndDate", ErrorMessage = "The Screening Date must be within the Movie's start and end dates.")]
        public DateTime Date { get; set; }

        public string Time { get; set; }

        [Display(Name = "End Time")]
        public string EndTime { get; set; }

        //[ForeignKey("Cinema")]
        public int CinemaId { get; set; }
        [ForeignKey("Room")]
        public int RoomId { get; set; }

        [ForeignKey("Movie")]
        public Guid MovieId { get; set; }

        public virtual Movie Movie { get; set; }
        public virtual Room Room { get; set; }

        public virtual Cinema Cinema { get; set; }
    }

    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;
        private readonly string _endDatePropertyName;

        public DateRangeAttribute(string startDatePropertyName, string endDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
            _endDatePropertyName = endDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var screening = (Screening)validationContext.ObjectInstance;
            if (screening.Movie != null)
            {
                var startDateValue = screening.Movie.StartDate;
                var endDateValue = screening.Movie.EndDate;

                var dateValue = (DateTime)value;

                if (dateValue < startDateValue || dateValue > endDateValue)
                {
                    return new ValidationResult($"The {validationContext.DisplayName} must be within the Movie's start and end dates.");
                }
            }

            return ValidationResult.Success;
        }
    }
}

