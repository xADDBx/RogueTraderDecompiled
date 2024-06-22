using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b69ed1bb15454c68886db5c9cee93703")]
public class BuffApplyTrigger : UnitFactComponentDelegate, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[FormerlySerializedAs("ActionForAdd")]
	public ActionList ActionForApply;

	public ActionList ActionForRankAdd;

	public ActionList ActionForRankReduce;

	public ActionList ActionForBuffRemove;

	[SerializeField]
	private BlueprintBuffReference m_ApplyChildBuff;

	[FormerlySerializedAs("ForOneAbility")]
	public bool ForOneBuff;

	[ShowIf("ForOneBuff")]
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public bool ForMultipleBuffs;

	[FormerlySerializedAs("Buffs")]
	[ShowIf("ForMultipleBuffs")]
	[SerializeField]
	private List<BlueprintBuffReference> m_Buffs;

	public bool ForAbilityGroup;

	[ShowIf("ForAbilityGroup")]
	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	public bool AnyTarget;

	public bool OnlyIfOwnerIsCaster;

	public bool OnlyPsychicPowerBuffs;

	public bool OnlyIfBuffFromCaster;

	public BlueprintBuff ApplyChildBuff => m_ApplyChildBuff?.Get();

	public BlueprintBuff Buff => m_Buff?.Get();

	public BlueprintAbilityGroup AbilityGroup => m_AbilityGroup?.Get();

	public void HandleBuffDidAdded(Buff buff)
	{
		using EntityFactComponentLoopGuard entityFactComponentLoopGuard = base.Runtime.RequestLoopGuard();
		if (entityFactComponentLoopGuard.Blocked)
		{
			return;
		}
		if (buff.Owner == null)
		{
			PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
			return;
		}
		if (OnlyPsychicPowerBuffs)
		{
			MechanicsContext maybeContext = buff.MaybeContext;
			if (maybeContext == null || maybeContext.SourceAbility == null || !buff.MaybeContext.SourceAbility.AbilityParamsSource.HasFlag(WarhammerAbilityParamsSource.PsychicPower))
			{
				return;
			}
			maybeContext = buff.MaybeContext;
			if (maybeContext != null && maybeContext.SourceAbility == null)
			{
				return;
			}
		}
		bool num = buff.Owner == base.Owner || AnyTarget;
		bool flag = (!OnlyIfOwnerIsCaster || buff.Context.MaybeCaster == base.Owner) && (!OnlyIfBuffFromCaster || buff.Context.MaybeCaster == base.Fact.MaybeContext?.MaybeCaster);
		if (!(num && flag) || buff.Blueprint == ApplyChildBuff || !CheckBuff(buff))
		{
			return;
		}
		if (ActionForApply != null && ActionForApply.HasActions)
		{
			base.Fact.RunActionInContext(ActionForApply, buff.Owner.ToITargetWrapper());
		}
		if (ApplyChildBuff != null)
		{
			Buff buff2 = buff.Owner.Buffs.Add(ApplyChildBuff, base.Context);
			if (buff2 != null)
			{
				buff.StoreFact(buff2);
			}
		}
	}

	private bool CheckBuff(Buff buff)
	{
		if (ForOneBuff)
		{
			return buff.Blueprint == Buff;
		}
		if (ForMultipleBuffs)
		{
			return m_Buffs.HasItem((BlueprintBuffReference r) => r.Is(buff.Blueprint));
		}
		if (ForAbilityGroup)
		{
			return buff.Blueprint.AbilityGroups.HasReference(m_AbilityGroup);
		}
		return true;
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (buff.Owner == null)
		{
			PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
			return;
		}
		if (OnlyPsychicPowerBuffs)
		{
			MechanicsContext maybeContext = buff.MaybeContext;
			if (maybeContext == null || maybeContext.SourceAbility == null || !buff.MaybeContext.SourceAbility.AbilityParamsSource.HasFlag(WarhammerAbilityParamsSource.PsychicPower))
			{
				return;
			}
			maybeContext = buff.MaybeContext;
			if (maybeContext != null && maybeContext.SourceAbility == null)
			{
				return;
			}
		}
		bool num = buff.Owner == base.Owner || AnyTarget;
		bool flag = !OnlyIfOwnerIsCaster || buff.Context.MaybeCaster == base.Owner;
		if (num && flag && buff.Blueprint != ApplyChildBuff && CheckBuff(buff) && ActionForBuffRemove != null && ActionForBuffRemove.HasActions)
		{
			base.Fact.RunActionInContext(ActionForBuffRemove, buff.Owner.ToITargetWrapper());
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
		if (buff.Owner == null)
		{
			PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
			return;
		}
		if (OnlyPsychicPowerBuffs)
		{
			MechanicsContext maybeContext = buff.MaybeContext;
			if (maybeContext == null || maybeContext.SourceAbility == null || !buff.MaybeContext.SourceAbility.AbilityParamsSource.HasFlag(WarhammerAbilityParamsSource.PsychicPower))
			{
				return;
			}
			maybeContext = buff.MaybeContext;
			if (maybeContext != null && maybeContext.SourceAbility == null)
			{
				return;
			}
		}
		if (buff.Owner != base.Context.MaybeOwner || buff.Blueprint == ApplyChildBuff || !CheckBuff(buff))
		{
			return;
		}
		if (ActionForRankAdd != null && ActionForRankAdd.HasActions)
		{
			base.Fact.RunActionInContext(ActionForRankAdd, buff.Owner.ToITargetWrapper());
		}
		if (ApplyChildBuff != null)
		{
			Buff buff2 = buff.Owner.Buffs.Add(ApplyChildBuff, base.Context);
			if (buff2 != null)
			{
				buff.StoreFact(buff2);
			}
		}
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
		if (buff.Owner == null)
		{
			PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
			return;
		}
		if (OnlyPsychicPowerBuffs)
		{
			MechanicsContext maybeContext = buff.MaybeContext;
			if (maybeContext == null || maybeContext.SourceAbility == null || !buff.MaybeContext.SourceAbility.AbilityParamsSource.HasFlag(WarhammerAbilityParamsSource.PsychicPower))
			{
				return;
			}
			maybeContext = buff.MaybeContext;
			if (maybeContext != null && maybeContext.SourceAbility == null)
			{
				return;
			}
		}
		if (buff.Owner != base.Context.MaybeOwner || buff.Blueprint == ApplyChildBuff || !CheckBuff(buff))
		{
			return;
		}
		if (ActionForRankReduce != null && ActionForRankAdd.HasActions)
		{
			base.Fact.RunActionInContext(ActionForRankReduce, buff.Owner.ToITargetWrapper());
		}
		if (ApplyChildBuff != null)
		{
			Buff buff2 = buff.Owner.Buffs.Add(ApplyChildBuff, base.Context);
			if (buff2 != null)
			{
				buff.StoreFact(buff2);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
