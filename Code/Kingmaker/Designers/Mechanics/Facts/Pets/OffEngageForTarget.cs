using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Pets;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("5e8f0ce5f55742d2937e900050cd3690")]
public class OffEngageForTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateOtherTargetForOffEngage>, IRulebookHandler<RuleCalculateOtherTargetForOffEngage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	[SerializeField]
	private bool m_IsRandomizeAttackLine;

	[SerializeField]
	[ShowIf("m_IsRandomizeAttackLine")]
	private float m_AttackLineAngle = 30f;

	[SerializeField]
	[ShowIf("m_IsRandomizeAttackLine")]
	private int m_RangeBetweenAttackerAndTarget = 1;

	protected override void OnActivate()
	{
		base.OnActivate();
		if (base.Fact is Buff buff && buff.Context.ParentContext is AbilityExecutionContext abilityExecutionContext && abilityExecutionContext.MainTarget.HasEntity && abilityExecutionContext.MainTarget.Entity is BaseUnitEntity baseUnitEntity && base.Owner.IsEnemy(baseUnitEntity))
		{
			base.Owner.GetOrCreate<PartOffEngageForTarget>().SetTarget(baseUnitEntity);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartOffEngageForTarget>()?.SetTarget(null);
		base.OnDeactivate();
	}

	void IRulebookHandler<RuleCalculateOtherTargetForOffEngage>.OnEventAboutToTrigger(RuleCalculateOtherTargetForOffEngage evt)
	{
		BaseUnitEntity obj = evt.MaybeTarget as BaseUnitEntity;
		if (obj != null && obj.IsOffEngageForTarget(evt.InitiatorUnit))
		{
			evt.SetupRandomizeAttackLineSettings(m_IsRandomizeAttackLine, m_AttackLineAngle, m_RangeBetweenAttackerAndTarget);
		}
	}

	void IRulebookHandler<RuleCalculateOtherTargetForOffEngage>.OnEventDidTrigger(RuleCalculateOtherTargetForOffEngage evt)
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
