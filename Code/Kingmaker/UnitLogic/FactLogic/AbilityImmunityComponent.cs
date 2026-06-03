using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("5053a294d2f3aa64d864f8e1a32aafb9")]
public abstract class AbilityImmunityComponent : UnitBuffComponentDelegate, IHashable
{
	[SerializeField]
	private ActionList m_ActionsOnImmunity;

	[SerializeField]
	private bool m_DisableGameLog;

	[SerializeField]
	[Tooltip("If true, the unit will be immune to all abilities, except the specified ones.")]
	protected bool m_InvertCondition;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityImmunity>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOrCreate<PartAbilityImmunity>().Unregister(base.Fact, this);
	}

	public abstract bool HasImmunityTo(BlueprintAbility ability);

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
