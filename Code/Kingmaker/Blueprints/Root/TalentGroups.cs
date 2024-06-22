using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Base;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class TalentGroups
{
	[Serializable]
	public class TalentGroupConfig
	{
		[SingleEnumFlagSelect(typeof(TalentGroup))]
		public TalentGroup Group;

		public Sprite Icon;

		public Color BgrColor;

		public TalentGroupConfig(TalentGroup group, Sprite icon, Color bgrColor)
		{
			Group = group;
			Icon = icon;
			BgrColor = bgrColor;
		}
	}

	public List<TalentGroupConfig> Groups;

	public TalentGroupConfig GetConfig(TalentGroup group)
	{
		return Groups.FindOrDefault((TalentGroupConfig e) => e.Group == group);
	}
}
