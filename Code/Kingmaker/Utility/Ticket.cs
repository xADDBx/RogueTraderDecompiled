namespace Kingmaker.Utility;

public class Ticket
{
	public string JiraTaskId { get; set; }

	public string Summary { get; set; }

	public string Assignee { get; set; }

	public object AssigneeAvatar { get; set; }

	public string Priority { get; set; }

	public int Distance { get; set; }

	public bool Fixed { get; set; }

	public string Status { get; set; }

	public string BuildStatus { get; set; }
}
