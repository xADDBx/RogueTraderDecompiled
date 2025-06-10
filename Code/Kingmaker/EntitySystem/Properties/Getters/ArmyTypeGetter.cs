using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("632fcd2b80d228149945aceb30f308b7")]
public class ArmyTypeGetter : UnitPropertyGetter
{
	[HideIf("SpecificArmyType")]
	public bool Human;

	[HideIf("SpecificArmyType")]
	public bool Xenos;

	[HideIf("SpecificArmyType")]
	public bool Daemon;

	public bool SpecificArmyType;

	[SerializeField]
	[ShowIf("SpecificArmyType")]
	private BlueprintArmyDescriptionReference[] m_Armies = new BlueprintArmyDescriptionReference[0];

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		List<string> list = new List<string>();
		if (SpecificArmyType)
		{
			BlueprintArmyDescriptionReference[] armies = m_Armies;
			foreach (BlueprintArmyDescriptionReference blueprintArmyDescriptionReference in armies)
			{
				list.Add(blueprintArmyDescriptionReference.NameSafe());
			}
		}
		else
		{
			if (Human)
			{
				list.Add("Human");
			}
			if (Xenos)
			{
				list.Add("Xenos");
			}
			if (Daemon)
			{
				list.Add("Daemon");
			}
		}
		return FormulaTargetScope.Current + " is from any of army types (" + string.Join("|", list) + ")";
	}

	protected override int GetBaseValue()
	{
		if (SpecificArmyType)
		{
			return HasSpecificArmyType();
		}
		bool flag = base.CurrentEntity.Blueprint.Army?.IsHuman ?? false;
		bool flag2 = base.CurrentEntity.Blueprint.Army?.IsXenos ?? false;
		bool flag3 = base.CurrentEntity.Blueprint.Army?.IsDaemon ?? false;
		if (!(Human && flag) && !(Xenos && flag2) && !(Daemon && flag3))
		{
			return 0;
		}
		return 1;
	}

	private int HasSpecificArmyType()
	{
		BlueprintArmyDescriptionReference[] armies = m_Armies;
		foreach (BlueprintArmyDescriptionReference blueprintArmyDescriptionReference in armies)
		{
			if (base.CurrentEntity.Blueprint.Army == blueprintArmyDescriptionReference.Get())
			{
				return 1;
			}
		}
		return 0;
	}
}
