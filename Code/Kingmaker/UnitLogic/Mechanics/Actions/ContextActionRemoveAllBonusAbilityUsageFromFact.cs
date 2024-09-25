using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("095658cd010d4987bd9990a88bc0f7e5")]
public class ContextActionRemoveAllBonusAbilityUsageFromFact : ContextAction
{
	[SerializeField]
	private BlueprintUnitFactReference m_SourceFact;

	public bool ToTarget;

	public BlueprintUnitFact SourceFact => m_SourceFact.Get();

	public override string GetCaption()
	{
		if (!ToTarget)
		{
			return "Remove all bonus ability usage given by a fact from caster";
		}
		return "Remove all bonus ability usage given by a fact from target";
	}

	protected override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext != null)
		{
			MechanicEntity mechanicEntity = (ToTarget ? base.Target.Entity : mechanicsContext.MaybeCaster);
			if (mechanicEntity != null && mechanicEntity is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.GetOptional<UnitPartBonusAbility>()?.RemoveBonusAbility(SourceFact);
			}
		}
	}
}
