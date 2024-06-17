using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class PartUnitState : AbstractUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitState>, IEntityPartOwner
	{
		PartUnitState State { get; }
	}

	[JsonProperty]
	private Size m_Size = Size.Medium;

	[JsonProperty]
	public List<SpellSchool> SuppressedSpellSchools = new List<SpellSchool>();

	private sbyte[] m_Conditions;

	private sbyte[] m_ConditionsImmunity;

	private List<EntityFact>[] m_ConditionSources;

	private List<EntityFact>[] m_ConditionImmunitySources;

	private int m_ImpatienceModifier;

	public readonly CountableFlag ControlledByDirector = new CountableFlag();

	private EntityFact m_CachedMythicReduceResistanceFact;

	[JsonProperty]
	public bool IsPanicked { get; set; }

	[JsonProperty]
	public bool CanRemoveFromParty { get; set; }

	public ImpatienceLevel Impatience { get; private set; }

	public bool IsCharging { get; set; }

	public bool IsRegenerating { get; set; }

	public bool IsHelpless
	{
		get
		{
			if (base.Owner.LifeState.IsConscious)
			{
				return HasCondition(UnitCondition.Sleeping);
			}
			return true;
		}
	}

	public bool IsAnimating => true;

	public bool IsAble => !IsProne;

	public bool CanRotate
	{
		get
		{
			if (!IsProne && !IsHelpless && !base.Owner.Features.RotationForbidden)
			{
				AbstractUnitEntityView abstractUnitEntityView = base.Owner.View.Or(null);
				bool? obj;
				if ((object)abstractUnitEntityView == null)
				{
					obj = null;
				}
				else
				{
					UnitAnimationManager unitAnimationManager = abstractUnitEntityView.AnimationManager.Or(null);
					obj = (((object)unitAnimationManager != null) ? new bool?(!unitAnimationManager.IsPreventingRotation) : null);
				}
				return obj ?? true;
			}
			return false;
		}
	}

	public bool IsProne
	{
		get
		{
			AbstractUnitCommand current = base.Owner.Commands.Current;
			if (current == null || !current.FromCutscene)
			{
				if (base.Owner.LifeState.IsConscious && !HasCondition(UnitCondition.Prone))
				{
					return HasCondition(UnitCondition.Sleeping);
				}
				return true;
			}
			return false;
		}
	}

	public bool CanMove
	{
		get
		{
			if (!(Game.Instance.CurrentMode == GameModeType.Cutscene))
			{
				if (!IsHelpless && IsAble && !base.Owner.Features.CantMove && !base.Owner.GetOptional<UnitPartForceMove>())
				{
					UnitPartEncumbrance optional = base.Owner.GetOptional<UnitPartEncumbrance>();
					if (optional == null)
					{
						return true;
					}
					return optional.Value != Encumbrance.Overload;
				}
				return false;
			}
			return true;
		}
	}

	public bool CanAct
	{
		get
		{
			if (!(Game.Instance.CurrentMode == GameModeType.Cutscene))
			{
				if (!IsHelpless && IsAble && !base.Owner.Features.CantAct)
				{
					AbstractUnitEntityView abstractUnitEntityView = base.Owner.View.Or(null);
					bool? obj;
					if ((object)abstractUnitEntityView == null)
					{
						obj = null;
					}
					else
					{
						UnitAnimationManager unitAnimationManager = abstractUnitEntityView.AnimationManager.Or(null);
						obj = (((object)unitAnimationManager != null) ? new bool?(!unitAnimationManager.IsInExclusiveState) : null);
					}
					return obj ?? true;
				}
				return false;
			}
			return true;
		}
	}

	public bool CanActInTurnBased
	{
		get
		{
			AbstractUnitEntityView abstractUnitEntityView = base.Owner.View.Or(null);
			UnitAnimationManager unitAnimationManager = (((object)abstractUnitEntityView != null) ? abstractUnitEntityView.AnimationManager.Or(null) : null);
			bool flag = unitAnimationManager != null && unitAnimationManager.IsInExclusiveState;
			bool flag2 = unitAnimationManager != null && (unitAnimationManager.IsProne || unitAnimationManager.IsStandUp);
			if (!(Game.Instance.CurrentMode == GameModeType.Cutscene))
			{
				if (!IsHelpless && IsAble && !base.Owner.Features.CantAct && !flag)
				{
					return !flag2;
				}
				return false;
			}
			return true;
		}
	}

	public bool CanDodge
	{
		get
		{
			if (!IsHelpless)
			{
				return !base.Owner.Features.CantAct;
			}
			return false;
		}
	}

	public bool CanDodgeWithMove
	{
		get
		{
			if (CanDodge && !base.Owner.Features.CantMove && !IsProne)
			{
				return !base.Owner.Features.CantJumpAside;
			}
			return false;
		}
	}

	public Size Size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size = value;
			base.Owner.UpdateSizeModifiers();
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSizeHandler>)delegate(IUnitSizeHandler h)
			{
				h.HandleUnitSizeChanged();
			}, isCheckRuntime: true);
		}
	}

	[JsonConstructor]
	public PartUnitState()
	{
		Setup();
	}

	private void Setup()
	{
		int num = EnumUtils.GetMaxValue<UnitCondition>() + 1;
		m_Conditions = new sbyte[num];
		m_ConditionsImmunity = new sbyte[num];
		m_ConditionSources = new List<EntityFact>[num];
		m_ConditionImmunitySources = new List<EntityFact>[num];
	}

	protected override void OnAttach()
	{
		Size = base.Owner.OriginalSize;
	}

	public void PrePostLoad(AbstractUnitEntity owner)
	{
		Setup();
	}

	private bool PreprocessCondition(UnitCondition condition, bool added, bool immunity)
	{
		FeatureCountableFlag featureCountableFlag = null;
		switch (condition)
		{
		case UnitCondition.CantAct:
			featureCountableFlag = base.Owner.Features.CantAct;
			break;
		case UnitCondition.CantMove:
			featureCountableFlag = base.Owner.Features.CantMove;
			break;
		case UnitCondition.DisableAttacksOfOpportunity:
			featureCountableFlag = base.Owner.Features.DisableAttacksOfOpportunity;
			break;
		}
		if (featureCountableFlag == null)
		{
			return false;
		}
		if (!immunity)
		{
			if (added)
			{
				featureCountableFlag.Retain();
			}
			else
			{
				featureCountableFlag.Release();
			}
		}
		PFLog.Default.ErrorWithReport($"{condition} isn't UnitCondition, use AddMechanicsFeature component");
		return true;
	}

	public void AddCondition(UnitCondition condition, [NotNull] EntityFact source)
	{
		if (PreprocessCondition(condition, added: true, immunity: false))
		{
			return;
		}
		bool num = HasCondition(condition);
		m_Conditions[(int)condition]++;
		List<EntityFact> list = m_ConditionSources[(int)condition];
		if (list == null)
		{
			list = (m_ConditionSources[(int)condition] = new List<EntityFact>());
		}
		list.Add(source);
		UpdateStatusEffect(condition, source);
		bool success = HasCondition(condition);
		EventBus.RaiseEvent((IAbstractUnitEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitConditionChangeAttemptHandler>)delegate(IUnitConditionChangeAttemptHandler h)
		{
			h.HandleUnitConditionAddAttempt(condition, success);
		}, isCheckRuntime: true);
		if (!num && success)
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitConditionsChanged>)delegate(IUnitConditionsChanged h)
			{
				h.HandleUnitConditionsChanged(condition);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveCondition(UnitCondition condition, [NotNull] EntityFact source)
	{
		AbstractUnitEntity owner = base.Owner;
		if ((owner != null && owner.IsDisposingNow) || PreprocessCondition(condition, added: false, immunity: false))
		{
			return;
		}
		bool num = HasCondition(condition);
		if (m_Conditions[(int)condition] > 0)
		{
			m_Conditions[(int)condition]--;
		}
		m_ConditionSources[(int)condition]?.Remove(source);
		UpdateStatusEffect(condition, source);
		if (num && !HasCondition(condition))
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitConditionsChanged>)delegate(IUnitConditionsChanged h)
			{
				h.HandleUnitConditionsChanged(condition);
			}, isCheckRuntime: true);
		}
	}

	public bool HasCondition(UnitCondition condition)
	{
		if (m_Conditions[(int)condition] > 0)
		{
			return m_ConditionsImmunity[(int)condition] <= 0;
		}
		return false;
	}

	public bool TryGetConditionSource(UnitCondition condition, out IReadOnlyList<EntityFact> result)
	{
		if (m_ConditionSources.TryGet((int)condition, out var element))
		{
			result = element;
			return true;
		}
		result = null;
		return false;
	}

	public void AddConditionImmunity(UnitCondition condition, [NotNull] EntityFact source)
	{
		if (PreprocessCondition(condition, added: true, immunity: true))
		{
			return;
		}
		bool num = HasCondition(condition);
		m_ConditionsImmunity[(int)condition]++;
		List<EntityFact> list = m_ConditionImmunitySources[(int)condition];
		if (list == null)
		{
			list = (m_ConditionImmunitySources[(int)condition] = new List<EntityFact>());
		}
		list.Add(source);
		UpdateStatusEffect(condition, source);
		if (num)
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitConditionsChanged>)delegate(IUnitConditionsChanged h)
			{
				h.HandleUnitConditionsChanged(condition);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveConditionImmunity(UnitCondition condition, [NotNull] EntityFact source)
	{
		AbstractUnitEntity owner = base.Owner;
		if ((owner != null && owner.IsDisposingNow) || PreprocessCondition(condition, added: false, immunity: true))
		{
			return;
		}
		bool num = HasCondition(condition);
		m_ConditionsImmunity[(int)condition]--;
		m_ConditionImmunitySources[(int)condition]?.Remove(source);
		UpdateStatusEffect(condition, source);
		if (!num && HasCondition(condition))
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitConditionsChanged>)delegate(IUnitConditionsChanged h)
			{
				h.HandleUnitConditionsChanged(condition);
			}, isCheckRuntime: true);
		}
	}

	public bool HasConditionImmunity(UnitCondition condition)
	{
		return m_ConditionsImmunity[(int)condition] > 0;
	}

	private void UpdateStatusEffect(UnitCondition condition, [NotNull] EntityFact source)
	{
		bool flag = m_Conditions[(int)condition] > 0;
		bool flag2 = m_ConditionsImmunity[(int)condition] > 0;
		BlueprintBuff blueprintBuff = BlueprintRoot.Instance.WarhammerRoot.UnitConditionBuffs?.GetMarker(condition);
		if (blueprintBuff != null)
		{
			EntityFact entityFact = base.Owner.Facts.FindBySource(blueprintBuff, condition);
			if (entityFact != null && (!flag || flag2))
			{
				base.Owner.Facts.Remove(entityFact);
			}
			else if (entityFact == null && flag && !flag2)
			{
				base.Owner.Facts.Add(blueprintBuff.CreateFact(null, base.Owner, null))?.AddSource(condition);
			}
		}
	}

	public void ChangeImpatience(int delta)
	{
		m_ImpatienceModifier += delta;
		int impatience = Math.Max(-1, Math.Min(1, m_ImpatienceModifier));
		Impatience = (ImpatienceLevel)impatience;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Size);
		bool val2 = IsPanicked;
		result.Append(ref val2);
		bool val3 = CanRemoveFromParty;
		result.Append(ref val3);
		List<SpellSchool> suppressedSpellSchools = SuppressedSpellSchools;
		if (suppressedSpellSchools != null)
		{
			for (int i = 0; i < suppressedSpellSchools.Count; i++)
			{
				SpellSchool obj = suppressedSpellSchools[i];
				Hash128 val4 = UnmanagedHasher<SpellSchool>.GetHash128(ref obj);
				result.Append(ref val4);
			}
		}
		return result;
	}
}
