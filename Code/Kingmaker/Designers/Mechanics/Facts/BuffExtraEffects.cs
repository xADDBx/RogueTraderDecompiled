using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[ComponentName("Add extra buff to buff")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("eb30e3ab60d893e4189e1be10a29a9e2")]
public class BuffExtraEffects : UnitFactComponentDelegate, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("CheckedBuff")]
	private BlueprintBuffReference m_CheckedBuff;

	[SerializeField]
	[FormerlySerializedAs("ExtraEffectBuff")]
	private BlueprintBuffReference m_ExtraEffectBuff;

	public BlueprintBuff CheckedBuff => m_CheckedBuff?.Get();

	public BlueprintBuff ExtraEffectBuff => m_ExtraEffectBuff?.Get();

	protected override void OnActivate()
	{
		if (base.Owner.Facts.Contains(CheckedBuff) && base.Owner.Facts.FindBySource(ExtraEffectBuff, base.Fact, this) == null)
		{
			base.Owner.Buffs.Add(ExtraEffectBuff, base.Context).AddSource(base.Fact, this);
		}
	}

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (buff.Blueprint == CheckedBuff && buff.Owner == base.Owner && base.Owner.Facts.FindBySource(ExtraEffectBuff, base.Fact, this) == null)
		{
			base.Owner.Buffs.Add(ExtraEffectBuff, base.Context).AddSource(base.Fact, this);
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (buff.Blueprint == CheckedBuff && buff.Owner == base.Owner && !base.Owner.Facts.Contains(CheckedBuff))
		{
			RemoveAllFactsOriginatedFromThisComponent(base.Owner);
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
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
