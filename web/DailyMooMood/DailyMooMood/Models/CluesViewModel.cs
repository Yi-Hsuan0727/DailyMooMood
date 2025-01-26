namespace DailyMooMood.Models
{
	public class CluesViewModel(string title, string description)
	{
		public string Title { get; set; } = title;
		public string Description { get; set; } = description;
	}
}
