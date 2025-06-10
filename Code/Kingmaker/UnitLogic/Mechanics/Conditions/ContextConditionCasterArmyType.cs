using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Serializable]
[TypeId("b6945e1e2b1242e29d6aa5394b6af2f1")]
public class ContextConditionCasterArmyType : ContextCondition
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

	protected override bool CheckCondition()
	{
		if (!(base.Context.MaybeCaster is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Default.Error("Caster is missing");
			return false;
		}
		if (SpecificArmyType)
		{
			return HasSpecificArmyType(baseUnitEntity);
		}
		bool flag = baseUnitEntity.Blueprint.Army?.IsHuman ?? false;
		bool flag2 = baseUnitEntity.Blueprint.Army?.IsXenos ?? false;
		bool flag3 = baseUnitEntity.Blueprint.Army?.IsDaemon ?? false;
		if (!(Human && flag) && !(Xenos && flag2))
		{
			return Daemon && flag3;
		}
		return true;
	}

	private bool HasSpecificArmyType(BaseUnitEntity caster)
	{
		BlueprintArmyDescriptionReference[] armies = m_Armies;
		foreach (BlueprintArmyDescriptionReference blueprintArmyDescriptionReference in armies)
		{
			if (caster.Blueprint.Army == blueprintArmyDescriptionReference.Get())
			{
				return true;
			}
		}
		return false;
	}

	protected override string GetConditionCaption()
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
		return "Caster is from any of army types (" + string.Join("|", list) + ")?";
	}
}
