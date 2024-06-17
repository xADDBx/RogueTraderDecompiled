using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingmaker.Utility;

[Serializable]
public class ExtendedContext : ReportContextBase
{
	private List<ContextRow> LastVisitedAreas = new List<ContextRow>();

	private List<ContextRow> LastDialogs = new List<ContextRow>();

	private List<ContextRow> LastAreaCreatures = new List<ContextRow>();

	private HashSet<string> creaturesSet = new HashSet<string>();

	private bool isLastAreaCreaturesCollected;

	private bool isDialogCollected;

	public ExtendedContext(List<string> historyLog)
	{
		for (int num = historyLog.Count - 1; num > 0; num--)
		{
			string text = historyLog[num];
			if (LastVisitedAreas.Count < 3 && text.Contains("loading area") && !text.Contains("unloading area"))
			{
				ContextRow contextRow = LineToExtendedContextRow(text, BugContext.InnerContextType.Area);
				if (IsValidContextRow(contextRow) && LastVisitedAreas.LastOrDefault()?.GetKey("Guid") != contextRow.GetKey("Guid"))
				{
					LastVisitedAreas.Add(contextRow);
					isLastAreaCreaturesCollected = LastVisitedAreas.Count >= 2;
				}
				if (IsAllCollected())
				{
					break;
				}
			}
			else if (!isDialogCollected && text.Contains("Started dialog"))
			{
				ContextRow contextRow2 = LineToExtendedContextRow(text, BugContext.InnerContextType.Dialog);
				if (IsValidContextRow(contextRow2))
				{
					LastDialogs.Add(contextRow2);
					isDialogCollected = true;
				}
				if (IsAllCollected())
				{
					break;
				}
			}
			else if (!isLastAreaCreaturesCollected && text.Contains(": unit"))
			{
				ContextRow contextRow3 = LineToExtendedContextRow(text, BugContext.InnerContextType.Unit);
				if (IsValidContextRow(contextRow3))
				{
					if (!creaturesSet.Contains(contextRow3.GetKey("Name")))
					{
						creaturesSet.Add(contextRow3.GetKey("Name"));
						LastAreaCreatures.Add(contextRow3);
					}
					if (IsAllCollected())
					{
						break;
					}
				}
			}
		}
		base.Contexts.Add("Last Visited Areas", LastVisitedAreas);
		base.Contexts.Add("Last Dialog", LastDialogs);
		base.Contexts.Add("Creatures", LastAreaCreatures);
		base.Contexts.Add("All Visible Interfaces", new InterfaceContextHelper().ContextRows);
	}

	private bool IsAllCollected()
	{
		if (LastVisitedAreas.Count >= 3 && isDialogCollected)
		{
			return isLastAreaCreaturesCollected;
		}
		return false;
	}

	private ContextRow LineToExtendedContextRow(string line, BugContext.InnerContextType contextType)
	{
		try
		{
			line = line.Substring(line.IndexOf(']') + 1, line.LastIndexOf(':') - (line.IndexOf(']') + 1)).Trim();
			string[] array = line.Split(' ');
			ContextParameter item = new ContextParameter("Context", contextType.ToString());
			ContextParameter contextParameter = new ContextParameter("Name", array[0].Trim());
			ContextParameter contextParameter2 = new ContextParameter("Guid", array[1].Trim().Replace("(", "").Replace(")", ""));
			if (!IsValidContextParameter(contextParameter) || !IsValidContextParameter(contextParameter2))
			{
				return null;
			}
			return new ContextRow(new List<ContextParameter> { item, contextParameter, contextParameter2 });
		}
		catch
		{
			return null;
		}
	}

	private bool IsValidContextParameter(ContextParameter check)
	{
		if (check != null && !string.IsNullOrEmpty(check.Name))
		{
			return !string.IsNullOrEmpty(check.Value);
		}
		return false;
	}

	private bool IsValidContextRow(ContextRow chek)
	{
		if (chek == null)
		{
			return false;
		}
		foreach (ContextParameter parameter in chek.Parameters)
		{
			if (!IsValidContextParameter(parameter))
			{
				return false;
			}
		}
		return true;
	}
}
