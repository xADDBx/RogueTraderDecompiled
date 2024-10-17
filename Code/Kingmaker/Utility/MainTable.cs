using System.Collections.Generic;

namespace Kingmaker.Utility;

public class MainTable
{
	public Dictionary<BugContext.ContextType, Dictionary<BugContext.AspectType, AssigneeQa>> Assignees = new Dictionary<BugContext.ContextType, Dictionary<BugContext.AspectType, AssigneeQa>>();
}
