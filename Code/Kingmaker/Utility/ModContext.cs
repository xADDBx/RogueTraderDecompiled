using System;
using System.Collections.Generic;
using Kingmaker.Utility.ModsInfo;

namespace Kingmaker.Utility;

[Serializable]
public class ModContext : ReportContextBase
{
	public ModContext()
	{
		List<ContextRow> list = new List<ContextRow>();
		foreach (ModInfo usedMod in UserModsData.Instance.UsedMods)
		{
			list.Add(new ContextRow
			{
				Parameters = 
				{
					new ContextParameter("Modification Name", usedMod.Id),
					new ContextParameter("Version", usedMod.Version)
				}
			});
		}
		base.Contexts.Add("Modifications", list);
	}
}
