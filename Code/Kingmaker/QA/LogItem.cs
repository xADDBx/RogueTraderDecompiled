namespace Kingmaker.QA;

public class LogItem : RegistrationService<LogItem>.DatedItem
{
	public string Message { get; set; }

	public string Callstack { get; set; }
}
