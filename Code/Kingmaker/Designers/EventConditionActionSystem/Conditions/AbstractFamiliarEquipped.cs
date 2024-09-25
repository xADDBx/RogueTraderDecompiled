using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("2e7638ded4e441b6b5bbf4c340b78f2a")]
public abstract class AbstractFamiliarEquipped : Condition
{
	[SerializeField]
	private BlueprintUnit.Reference m_Blueprint;

	public BlueprintUnit Unit => m_Blueprint?.Get();

	protected abstract BaseUnitEntity Leader { get; }

	protected override bool CheckCondition()
	{
		return (Leader?.GetFamiliarLeaderOptional())?.HasEquippedFamiliar(Unit) ?? (Unit == null);
	}
}
