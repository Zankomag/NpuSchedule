namespace NpuSchedule.Core.Options; 

public class GoogleSheetScheduleOptions {

	public const string SectionName = "GoogleSheetSchedule";
	
	public string GroupName { get; set; } = null!;

	public string GraduationLevel { get; set; } = null!;

	public string GoogleSheetId { get; set; } = null!;

	public string GoogleSheetGroupColumnLetter { get; set; } = null!;
	
	public string GoogleApiKey { get; set; } = null!;

}