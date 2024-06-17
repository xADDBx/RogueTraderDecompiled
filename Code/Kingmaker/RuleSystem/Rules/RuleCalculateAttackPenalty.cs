using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateAttackPenalty : RulebookEvent
{
	private readonly AbilityData m_Ability;

	public int ResultBallisticSkillPenalty { get; private set; }

	public int ResultWeaponSkillPenalty { get; private set; }

	public RuleCalculateAttackPenalty([NotNull] MechanicEntity initiator, AbilityData ability)
		: base(initiator)
	{
		m_Ability = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!m_Ability.IsAttackOfOpportunity && IsInitiatorAttackedThisTurn())
		{
			ResultWeaponSkillPenalty = 20;
			ResultBallisticSkillPenalty = 20;
		}
	}

	private bool IsInitiatorAttackedThisTurn()
	{
		return base.ConcreteInitiator.GetTwoWeaponFightingOptional()?.IsAttackedThisTurn ?? false;
	}
}
