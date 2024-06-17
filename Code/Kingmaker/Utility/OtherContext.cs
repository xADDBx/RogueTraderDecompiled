using System;
using System.Collections.Generic;

namespace Kingmaker.Utility;

[Serializable]
public class OtherContext : ReportContextBase
{
	public OtherContext(List<BugContext> contextVariants, BugContext currentContext)
	{
		foreach (BugContext contextVariant in contextVariants)
		{
			if (contextVariant != currentContext)
			{
				try
				{
					AddContext("OtherContext", new ContextParameter("Context", contextVariant.Type.ToString("G")), new ContextParameter("Aspect", contextVariant.Aspect.ToString("G")), new ContextParameter("Blueprint", contextVariant.GetContextObjectBlueprintName()), new ContextParameter("CurrentDialog", contextVariant.GetDialogGuid()));
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
	}
}
