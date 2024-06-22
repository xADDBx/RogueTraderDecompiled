using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowMultipleComponents]
[TypeId("f2dd09e9b92aa574097a32a41b7e140e")]
public class StarshipDamageModifier : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleStarshipPerformAttack>, IRulebookHandler<RuleStarshipPerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleStarshipPerformAttack>, ITargetRulebookSubscriber, IHashable
{
	private enum TriggerType
	{
		AsInitiator,
		AsTarget
	}

	[SerializeField]
	private TriggerType triggerType;

	public bool CheckWeaponType;

	[ShowIf("CheckWeaponType")]
	public StarshipWeaponType WeaponType;

	[SerializeField]
	private int m_BonusDamage;

	[SerializeField]
	private float m_ExtraDamageMod;

	[SerializeField]
	private bool m_MultiplyByBuffRank;

	[SerializeField]
	[ShowIf("m_MultiplyByBuffRank")]
	private BlueprintBuffReference m_StackingBuff;

	public BlueprintBuff StackingBuff => m_StackingBuff?.Get();

	public void OnEventAboutToTrigger(RuleStarshipPerformAttack evt)
	{
		if ((triggerType != 0 || evt.Initiator == base.Owner) && (triggerType != TriggerType.AsTarget || evt.Target == base.Owner) && (!CheckWeaponType || evt.Weapon.Blueprint.WeaponType == WeaponType))
		{
			float num = ((!m_MultiplyByBuffRank) ? 1 : (base.Owner.Buffs.GetBuff(StackingBuff)?.GetRank() ?? 0));
			evt.BonusDamage += Mathf.RoundToInt((float)m_BonusDamage * num);
			evt.ExtraDamageMod += m_ExtraDamageMod * num;
		}
	}

	public void OnEventDidTrigger(RuleStarshipPerformAttack evt)
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
