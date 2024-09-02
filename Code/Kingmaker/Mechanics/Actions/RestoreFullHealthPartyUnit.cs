using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("41287fc5bca34087b6ccf71bef420196")]
public class RestoreFullHealthPartyUnit : ContextAction
{
	[SerializeField]
	private BlueprintUnitReference m_TargetPartyUnit;

	public override string GetCaption()
	{
		BlueprintUnit blueprintUnit = m_TargetPartyUnit.Get();
		if (blueprintUnit != null)
		{
			return "Restore full health " + blueprintUnit.CharacterName;
		}
		return "Restore full health failed! Target unit blueprint is null!";
	}

	protected override void RunAction()
	{
		BlueprintUnit blueprintUnit = m_TargetPartyUnit.Get();
		if (blueprintUnit == null)
		{
			return;
		}
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.Blueprint == blueprintUnit)
			{
				item.GetHealthOptional()?.HealDamageAll();
				break;
			}
		}
	}
}
