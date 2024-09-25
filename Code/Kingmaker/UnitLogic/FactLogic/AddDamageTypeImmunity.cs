using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Damage;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("245250259ef6425787b71c6fa1b493d5")]
public class AddDamageTypeImmunity : MechanicEntityFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	[EnumFlagsAsButtons]
	public DamageTypeMask Types;

	[SerializeField]
	private ActionList m_ActionsOnImmunity;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (Types.Contains(evt.DamageType))
		{
			evt.ValueModifiers.Add(ModifierType.PctMul_Extra, 0, base.Fact, ModifierDescriptor.Immunity);
			base.Fact.RunActionInContext(m_ActionsOnImmunity, base.Owner.ToITargetWrapper());
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
