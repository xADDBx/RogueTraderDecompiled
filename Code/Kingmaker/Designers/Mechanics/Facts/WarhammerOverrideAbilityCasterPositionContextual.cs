using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("44eaefc6ca2f44899eadc30a2c46bf19")]
public class WarhammerOverrideAbilityCasterPositionContextual : BlueprintComponent, IAbilityOverrideCasterForRange
{
	[SerializeField]
	[SerializeReference]
	private MechanicEntityEvaluator m_FakeCasterEvaluator;

	[CanBeNull]
	public MechanicEntity GetCaster(MechanicEntity originalCaster, bool forUi)
	{
		if (!m_FakeCasterEvaluator.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}
}
