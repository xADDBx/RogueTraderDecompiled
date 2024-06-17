using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartLifeState : AbstractUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartLifeState>, IEntityPartOwner
	{
		PartLifeState LifeState { get; }
	}

	[JsonProperty]
	private bool m_IsDeathRevealed;

	[JsonProperty]
	public bool IsHiddenBecauseDead { get; private set; }

	public bool IsDeathRevealed
	{
		get
		{
			return m_IsDeathRevealed;
		}
		set
		{
			if (m_IsDeathRevealed != value)
			{
				m_IsDeathRevealed = value;
				if ((bool)base.Owner.View)
				{
					base.Owner.View.UpdateViewActive();
				}
			}
		}
	}

	[JsonProperty]
	public TimeSpan DeathTime { get; private set; }

	[JsonProperty]
	public UnitLifeState State { get; private set; }

	[JsonProperty]
	public bool MarkedForDeath { get; set; }

	[JsonProperty]
	public bool IsManualDeath { get; private set; }

	[JsonProperty]
	public bool ScriptedKill { get; set; }

	[JsonProperty]
	public UnitDismemberType ForceDismember { get; set; }

	[JsonProperty]
	public DismembermentLimbsApartType? DismembermentLimbsApartType { get; set; }

	public PartHealth Health => base.Owner.GetRequired<PartHealth>();

	public bool IsConscious => State == UnitLifeState.Conscious;

	public bool IsUnconscious => State == UnitLifeState.Unconscious;

	public bool IsDead => State == UnitLifeState.Dead;

	public bool IsDeadOrUnconscious => State != UnitLifeState.Conscious;

	public bool IsFinallyDead
	{
		get
		{
			if (IsDead)
			{
				if ((bool)base.Owner.Features.Immortality)
				{
					return ScriptedKill;
				}
				return true;
			}
			return false;
		}
	}

	public void ManualDeath()
	{
		MarkedForDeath = true;
		IsManualDeath = true;
	}

	public bool Set(UnitLifeState newLifeState)
	{
		if (State == newLifeState)
		{
			return false;
		}
		State = newLifeState;
		if (newLifeState == UnitLifeState.Dead)
		{
			DeathTime = Game.Instance.TimeController.GameTime;
		}
		else
		{
			IsDeathRevealed = false;
		}
		return true;
	}

	public void Resurrect(int resultHealth = 1, bool restoreHealth = true)
	{
		Resurrect(resultHealth, restoreHealth, fullRestore: false);
	}

	public void ResurrectAndFullRestore()
	{
		Resurrect(0);
	}

	private void Resurrect(int resultHealth, bool restoreHealth, bool fullRestore)
	{
		IsHiddenBecauseDead = false;
		UnitLifeController.ForceUnitConscious(base.Owner);
		if (fullRestore)
		{
			Health.HealDamageAll();
		}
		else if (restoreHealth)
		{
			Health.SetHitPointsLeft(resultHealth);
		}
		PartStatsAttributes optional = base.Owner.GetOptional<PartStatsAttributes>();
		if (optional != null)
		{
			UpdateAttributesDamageAndDrainOnResurrect(optional, fullRestore);
		}
		UpdateUnitViewOnResurrect(base.Owner.View);
		base.Owner.Buffs.RemoveBuffsOnResurrect();
		EventBus.RaiseEvent((IAbstractUnitEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitResurrectedHandler>)delegate(IUnitResurrectedHandler h)
		{
			h.HandleUnitResurrected();
		}, isCheckRuntime: true);
	}

	private static void UpdateAttributesDamageAndDrainOnResurrect([NotNull] PartStatsAttributes attributes, bool fullRestore)
	{
		foreach (ModifiableValueAttributeStat attribute in attributes)
		{
			if (fullRestore)
			{
				attribute.Damage = 0;
				attribute.Drain = 0;
			}
			if (attribute.ModifiedValueRaw < 1)
			{
				int num = -attribute.ModifiedValueRaw + 1;
				int num2 = Math.Min(attribute.Damage, num);
				attribute.Damage -= num2;
				num -= num2;
				int num3 = Math.Min(attribute.Drain, num);
				attribute.Drain -= num3;
			}
		}
	}

	private static void UpdateUnitViewOnResurrect([NotNull] AbstractUnitEntityView view)
	{
		UnitEntityView unitEntityView = view as UnitEntityView;
		if (unitEntityView != null)
		{
			unitEntityView.RigidbodyController.Or(null)?.CancelRagdoll();
		}
		view.LeaveProneState();
		if (unitEntityView != null)
		{
			Game.Instance.HandsEquipmentController.ScheduleUpdate(unitEntityView.HandsEquipment);
		}
		view.ResetMouseHighlighted();
	}

	public void HideIfDead()
	{
		IsHiddenBecauseDead |= IsFinallyDead;
	}

	public void RevealDeath()
	{
		IsDeathRevealed = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsHiddenBecauseDead;
		result.Append(ref val2);
		result.Append(ref m_IsDeathRevealed);
		TimeSpan val3 = DeathTime;
		result.Append(ref val3);
		UnitLifeState val4 = State;
		result.Append(ref val4);
		bool val5 = MarkedForDeath;
		result.Append(ref val5);
		bool val6 = IsManualDeath;
		result.Append(ref val6);
		bool val7 = ScriptedKill;
		result.Append(ref val7);
		UnitDismemberType val8 = ForceDismember;
		result.Append(ref val8);
		if (DismembermentLimbsApartType.HasValue)
		{
			DismembermentLimbsApartType val9 = DismembermentLimbsApartType.Value;
			result.Append(ref val9);
		}
		return result;
	}
}
