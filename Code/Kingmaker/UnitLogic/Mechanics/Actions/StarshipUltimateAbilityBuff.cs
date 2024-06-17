using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("cae5fcf97ef49da4eb7a70a0e181d81c")]
public class StarshipUltimateAbilityBuff : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetCaption()
	{
		return "Apply " + Buff.NameSafe() + " with post officer skill dependant duration";
	}

	public override void RunAction()
	{
		if (!base.Caster.Buffs.Contains(Buff))
		{
			StarshipCompanionsOnPostLogic starshipCompanionsOnPostLogic = base.Caster.Facts.GetComponents<StarshipCompanionsOnPostLogic>().FirstOrDefault();
			BuffDuration duration;
			if (starshipCompanionsOnPostLogic != null)
			{
				int ultimateBuffDuration = starshipCompanionsOnPostLogic.GetUltimateBuffDuration(base.Caster as StarshipEntity, base.Context.SourceAbility);
				duration = new BuffDuration(new Rounds(ultimateBuffDuration), BuffEndCondition.SpaceCombatExit);
			}
			else
			{
				duration = new BuffDuration(null, BuffEndCondition.SpaceCombatExit);
			}
			base.Caster.Buffs.Add(Buff, duration);
		}
	}
}
