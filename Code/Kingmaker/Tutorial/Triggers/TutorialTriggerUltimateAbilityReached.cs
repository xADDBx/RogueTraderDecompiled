using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Units;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("9c48fda7038d47629e3488db4ee7bc86")]
public class TutorialTriggerUltimateAbilityReached : TutorialTriggerRulebookEvent<RulePerformMomentumChange>, IHashable
{
	private enum UltimateAbilityType
	{
		HeroicAct,
		DesperateMeasure
	}

	[SerializeField]
	private UltimateAbilityType m_UltimateAbilityType;

	private BlueprintMomentumRoot MomentumRoot => Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;

	protected override bool ShouldTrigger(RulePerformMomentumChange rule)
	{
		int desperateMeasureThreshold = rule.ConcreteInitiator.GetDesperateMeasureThreshold();
		UltimateAbilityType ultimateAbilityType = m_UltimateAbilityType;
		if (ultimateAbilityType != 0)
		{
			if (ultimateAbilityType == UltimateAbilityType.DesperateMeasure && rule.ResultPrevValue > desperateMeasureThreshold && rule.ResultCurrentValue <= desperateMeasureThreshold)
			{
				goto IL_0054;
			}
		}
		else if (rule.ResultPrevValue < MomentumRoot.HeroicActThreshold && rule.ResultCurrentValue >= MomentumRoot.HeroicActThreshold)
		{
			goto IL_0054;
		}
		return false;
		IL_0054:
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
