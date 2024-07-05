using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSoTL.Models
{
    public class Movie
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Image { get; set; }

        public double Price { get; set; }

        public string Description { get; set; }
        public string? Age { get; set; }

        public string Trailer { get; set; }

        public int DurationMinutes { get; set; }

        [ForeignKey("Category")]
        public int Cat_Id { get; set; }

        [ForeignKey("Producer")]
        public int Producer_Id { get; set; }
        public int TotalQuantitySold { get; set; }

        public virtual Category Category { get; set; }

        public virtual Producer Producer { get; set; }

        public ICollection<MoviesInCinema> MoviesInCinema { get; set; }
        public ICollection<Screening> Screenings { get; set; }
        public ICollection<MovieActor> MovieActors { get; set; }
    }

    public class DateValidationHelper
    {
        public static ValidationResult ValidateMovieDates(DateTime startDate, DateTime endDate)
        {
            DateTime currentDate = DateTime.Now;

            // Kiểm tra ngày khởi chiếu phim có lớn hơn hoặc bằng ngày hiện tại không
            if (startDate < currentDate)
            {
                return new ValidationResult("Ngày khởi chiếu phim phải lớn hơn hoặc bằng ngày hiện tại.");
            }

            // Kiểm tra ngày kết thúc phim có lớn hơn ngày khởi chiếu ít nhất 10 ngày không
            TimeSpan duration = endDate - startDate;
            if (duration.TotalDays < 10)
            {
                return new ValidationResult("Ngày kết thúc chiếu phải lớn hơn ngày khởi chiếu ít nhất 10 ngày.");
            }

            return ValidationResult.Success;
        }
    }

    public class DateRangeValidationAttribute : ValidationAttribute
    {
        private readonly string _startPropertyName;
        private readonly string _endPropertyName;

        public DateRangeValidationAttribute(string startDatePropertyName, string endDatePropertyName)
        {
            _startPropertyName = startDatePropertyName;
            _endPropertyName = endDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDateProperty = validationContext.ObjectType.GetProperty(_startPropertyName);
            var endDateProperty = validationContext.ObjectType.GetProperty(_endPropertyName);

            if (startDateProperty == null || endDateProperty == null)
            {
                return new ValidationResult($"Invalid property names: {_startPropertyName} or {_endPropertyName}");
            }

            var startDateValue = (DateTime?)startDateProperty.GetValue(validationContext.ObjectInstance);
            var endDateValue = (DateTime?)endDateProperty.GetValue(validationContext.ObjectInstance);

            if (startDateValue == null || endDateValue == null)
            {
                return ValidationResult.Success; // Không kiểm tra nếu giá trị không tồn tại
            }

            var dateValue = (DateTime)value;

            // Kiểm tra ngày của thuộc tính được áp dụng
            if (dateValue < startDateValue || dateValue > endDateValue)
            {
                return new ValidationResult($"The {validationContext.DisplayName} must be within the Movie's start and end dates.");
            }

            return ValidationResult.Success;
        }
    }
}