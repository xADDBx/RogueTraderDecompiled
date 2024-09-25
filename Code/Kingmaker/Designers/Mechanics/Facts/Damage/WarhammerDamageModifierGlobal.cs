using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[AllowMultipleComponents]
[TypeId("751154c0214f445d8b3062786992503d")]
public class WarhammerDamageModifierGlobal : WarhammerDamageModifier, IGlobalRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public bool OnlyAgainstAllies;

	public bool NotAgainstOwner;

	public bool OnlyIfTargetHasBuff;

	[ShowIf("OnlyIfTargetHasBuff")]
	[SerializeField]
	private BlueprintBuffReference[] m_Buffs;

	[ShowIf("OnlyIfTargetHasBuff")]
	public bool BuffOnlyFromCaster;

	public ReferenceArrayProxy<BlueprintBuff> Buffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffs = m_Buffs;
			return buffs;
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		MechanicEntity mechanicEntity = (MechanicEntity)evt.Initiator;
		MechanicEntity maybeTarget = evt.MaybeTarget;
		if (maybeTarget != null && mechanicEntity != null && (!OnlyAgainstAllies || (mechanicEntity.IsAlly(maybeTarget) && maybeTarget.IsAlly(base.Owner))) && (!NotAgainstOwner || maybeTarget != base.Owner) && (!OnlyIfTargetHasBuff || maybeTarget.Buffs.Enumerable.Any((Buff p) => Buffs.Contains(p.Blueprint) && (!BuffOnlyFromCaster || p.Context.MaybeCaster == base.Owner))))
		{
			TryApply(evt);
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
