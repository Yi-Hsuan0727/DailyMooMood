using System.ComponentModel.DataAnnotations;

namespace DailyMooMood.Models
{
    public class LandingPageViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Pick the Mood!")]
        public int? Type { get; set; }

        [Display(Name = "Your comment")]
        public string? Comment { get; set; }

        public string? Date { get; set; }
    }
}
