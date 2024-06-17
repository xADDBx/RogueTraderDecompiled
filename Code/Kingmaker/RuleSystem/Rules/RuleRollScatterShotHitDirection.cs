using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollScatterShotHitDirection : RulebookEvent
{
	public enum Ray
	{
		LeftFar,
		LeftClose,
		Main,
		RightClose,
		RightFar
	}

	private int m_Weight;

	public readonly AbilityData Ability;

	public readonly int ShotIndex;

	public Ray Result { get; private set; }

	public RuleRollD100 ResultD100 { get; private set; }

	public int ResultMainLineChance { get; private set; }

	public int ResultFarLineChance { get; private set; }

	public RuleRollScatterShotHitDirection([NotNull] MechanicEntity initiator, [NotNull] AbilityData ability, int shotIndex)
		: base(initiator)
	{
		Ability = ability;
		ShotIndex = shotIndex;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (AttackHitPolicyContextData.Current == AttackHitPolicyType.AutoHit)
		{
			Result = Ray.Main;
			return;
		}
		RuleCalculateScatterShotHitDirectionProbability ruleCalculateScatterShotHitDirectionProbability = Rulebook.Trigger(new RuleCalculateScatterShotHitDirectionProbability(base.ConcreteInitiator, Ability, ShotIndex));
		ResultD100 = Dice.D100;
		int result = ResultD100.Result;
		ResultMainLineChance = ruleCalculateScatterShotHitDirectionProbability.ResultMainLine;
		ResultFarLineChance = ruleCalculateScatterShotHitDirectionProbability.ResultMainLine + ruleCalculateScatterShotHitDirectionProbability.ResultScatterNear;
		m_Weight = ruleCalculateScatterShotHitDirectionProbability.ResultMainLine;
		if (result <= m_Weight)
		{
			Result = Ray.Main;
			return;
		}
		m_Weight += ruleCalculateScatterShotHitDirectionProbability.ResultScatterNear;
		if (result <= m_Weight)
		{
			Result = ((result <= m_Weight - ruleCalculateScatterShotHitDirectionProbability.ResultScatterNear / 2) ? Ray.LeftClose : Ray.RightClose);
		}
		else
		{
			Result = ((result > m_Weight + ruleCalculateScatterShotHitDirectionProbability.ResultScatterFar / 2) ? Ray.RightFar : Ray.LeftFar);
		}
	}
}
