using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateDamagePush : RulebookTargetEvent
{
	private readonly float m_DeflectionCoef;

	private readonly float m_AbsorptionCoef;

	private readonly float m_DealtCoef;

	private readonly RuleDealDamage m_DamageRule;

	public float StrengthMultiplier = 1f;

	public float StrengthAddition;

	public int ResultPush;

	public RuleCalculateDamagePush([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] RuleDealDamage damageRule)
		: this((MechanicEntity)initiator, (MechanicEntity)target, damageRule)
	{
	}

	public RuleCalculateDamagePush([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] RuleDealDamage damageRule)
		: base(initiator, target)
	{
		m_DeflectionCoef = BlueprintWarhammerRoot.Instance.PushSettingRoot.DeflectionCoefficient;
		m_AbsorptionCoef = BlueprintWarhammerRoot.Instance.PushSettingRoot.AbsorptionCoefficient;
		m_DealtCoef = BlueprintWarhammerRoot.Instance.PushSettingRoot.DealtCoefficient;
		m_DamageRule = damageRule;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
	}
}
