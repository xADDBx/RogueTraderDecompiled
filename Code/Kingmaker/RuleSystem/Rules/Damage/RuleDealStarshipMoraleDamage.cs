using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleDealStarshipMoraleDamage : RulebookTargetEvent
{
	[CanBeNull]
	public readonly PartStarshipMorale TargetMorale;

	public readonly int Damage;

	public int Result { get; private set; }

	public RuleDealStarshipMoraleDamage([NotNull] IMechanicEntity initiator, [NotNull] StarshipEntity target, int moraleDamage)
		: this((MechanicEntity)initiator, target, moraleDamage)
	{
	}

	public RuleDealStarshipMoraleDamage([NotNull] MechanicEntity initiator, [NotNull] StarshipEntity target, int moraleDamage)
		: base(initiator, target)
	{
		TargetMorale = target.Morale;
		Damage = moraleDamage;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetMorale != null)
		{
			Result = Damage;
			TargetMorale.MoraleDamage += Result;
		}
	}
}
