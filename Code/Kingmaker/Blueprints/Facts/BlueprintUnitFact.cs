using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using UnityEngine;

namespace Kingmaker.Blueprints.Facts;

[TypeId("7e85fcaa0f664874f8d214d73cff305b")]
public class BlueprintUnitFact : BlueprintMechanicEntityFact
{
	[SerializeField]
	private bool m_AllowNonContextActions;

	public override bool AllowContextActionsOnly => !m_AllowNonContextActions;

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, MechanicEntity owner, BuffDuration duration)
	{
		return new UnitFact(this);
	}
}
