using System;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.BugReport;

public class BugDuplicateItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string NumberTask;

	public readonly string Title;

	public readonly string Creator;

	public readonly bool IsTask;

	public readonly int PriorityType;

	public readonly string Status;

	public readonly string BuildStatus;

	public readonly int Distance;

	public readonly bool IsFixed;

	public readonly string Url;

	public BugDuplicateItemVM(Ticket ticket)
	{
		NumberTask = ticket.JiraTaskId;
		Title = ticket.Summary;
		Creator = ticket.Assignee ?? "Aeon";
		IsTask = false;
		PriorityType = GetPriorityByString(ticket.Priority);
		Status = ticket.Status;
		BuildStatus = ticket.BuildStatus;
		Distance = ticket.Distance;
		IsFixed = ticket.Fixed;
		Url = "https://jira.owlcat.local/browse/" + ticket.JiraTaskId;
	}

	private int GetPriorityByString(string value)
	{
		switch (value)
		{
		case "Blocker":
			return 1;
		case "Crit":
			return 2;
		case "Normal":
			return 3;
		case "Minor":
			return 4;
		case "Trivial":
			return 5;
		default:
			UberDebug.LogError("Error: Priority type " + value + " not defined");
			return 1;
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
