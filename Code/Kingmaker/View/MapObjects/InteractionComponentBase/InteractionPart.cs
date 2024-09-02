using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

public abstract class InteractionPart<TSettings> : InteractionPart, IHashable where TSettings : InteractionSettings, new()
{
	public new TSettings Settings { get; private set; } = new TSettings();


	public virtual bool InteractThroughVariants { get; protected set; }

	protected override InteractionSettings GetSettings()
	{
		return Settings;
	}

	public override void SetSource(IAbstractEntityPartComponent source)
	{
		IAbstractEntityPartComponent source2 = base.Source;
		base.SetSource(source);
		Settings = source.GetSettings() as TSettings;
		OnSettingsDidSet(source2 != source);
		ConfigureRestrictions();
	}

	public void SetSettings(TSettings settings)
	{
		if (settings != Settings)
		{
			Settings = settings;
			OnSettingsDidSet(isNewSettings: true);
			ConfigureRestrictions();
		}
	}

	protected virtual void OnSettingsDidSet(bool isNewSettings)
	{
	}

	protected virtual void ConfigureRestrictions()
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
public abstract class InteractionPart : ViewBasedPart, IDestructionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IPartyCombatHandler, IHashable
{
	[JsonProperty]
	public readonly HashSet<UnitReference> PlayersClose = new HashSet<UnitReference>();

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool AlreadyUnlocked;

	[JsonProperty]
	private bool m_Enabled = true;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private int m_LastCombatRoundInteractionAttempt = -1;

	public InteractionType Type => Settings.Type;

	public InteractionSettings Settings => GetSettings();

	public new MapObjectView View => (MapObjectView)base.View;

	public new MapObjectEntity Owner => (MapObjectEntity)base.Owner;

	public UIInteractionType UIInteractionType
	{
		get
		{
			if (Settings.UIType != 0)
			{
				return Settings.UIType;
			}
			return GetDefaultUIType();
		}
	}

	public bool AlreadyInteractedInThisCombatRound
	{
		get
		{
			TurnController turnController = Game.Instance.TurnController;
			if (turnController != null && turnController.TbActive)
			{
				return turnController.CombatRound == m_LastCombatRoundInteractionAttempt;
			}
			return false;
		}
	}

	public int ApproachRadius
	{
		get
		{
			if (Settings.ProximityRadius <= 0)
			{
				return 2;
			}
			return Settings.ProximityRadius;
		}
	}

	public List<BaseUnitEntity> UnitsCanInteract { get; private set; }

	public bool Enabled
	{
		get
		{
			if (!Settings.AlwaysDisabled)
			{
				return m_Enabled;
			}
			return false;
		}
		set
		{
			if (m_Enabled != value)
			{
				m_Enabled = value;
				m_Enabled = Enabled;
				View.Or(null)?.UpdateHighlight();
				OnEnabledChanged();
				EventBus.RaiseEvent((IMapObjectEntity)Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
				{
					h.HandleObjectInteractChanged();
				}, isCheckRuntime: true);
			}
		}
	}

	public int ActionPointsCost
	{
		get
		{
			if (!Settings.OverrideActionPointsCost)
			{
				return 2;
			}
			return Settings.ActionPointsCost;
		}
	}

	public UnitAnimationInteractionType UseAnimationState
	{
		get
		{
			if (HasVisibleTrap())
			{
				return UnitAnimationInteractionType.DisarmTrap;
			}
			if (Settings.Type == InteractionType.Direct)
			{
				return UnitAnimationInteractionType.None;
			}
			return Settings.UseAnimationState;
		}
	}

	protected abstract InteractionSettings GetSettings();

	protected virtual UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Move;
	}

	protected virtual void OnEnabledChanged()
	{
	}

	protected override void OnViewDidAttach()
	{
		if (Settings.Trap != null)
		{
			Settings.Trap.TrappedObject = this;
		}
		UnitsCanInteract = GetMaxUnitsToInteract();
	}

	public void Interact(BaseUnitEntity user)
	{
		if (!CanInteract())
		{
			PFLog.Default.Error("{0} can't interact with {1}", user, this);
			return;
		}
		TurnController turnController = Game.Instance.TurnController;
		if (turnController.TbActive)
		{
			m_LastCombatRoundInteractionAttempt = turnController.CombatRound;
		}
		using (ContextData<MechanicEntityData>.Request().Setup(Owner))
		{
			if (Settings.Trap != null && Settings.Trap.Data.TrapActive)
			{
				Settings.Trap.Data.Interact(user);
				View.Or(null)?.UpdateHighlight();
				return;
			}
			List<InteractionRestrictionPart> restrictions = GetRestrictions();
			bool flag = ContextData<InteractionVariantData>.Current?.VariantActor != null;
			if (CanInteract(restrictions, user))
			{
				if (flag)
				{
					ContextData<InteractionVariantData>.Current.VariantActor.OnInteract(user);
					EventBus.RaiseEvent(delegate(IInteractWithVariantActorHandler h)
					{
						h.HandleInteractWithVariantActor(this, ContextData<InteractionVariantData>.Current.VariantActor);
					});
				}
				if (flag)
				{
					Game.Instance.CoroutinesController.InvokeInTime(delegate
					{
						OnInteract(user);
					}, 0.2f.Seconds());
				}
				else
				{
					OnInteract(user);
				}
				foreach (InteractionRestrictionPart item in restrictions)
				{
					item.OnDidInteract(user);
				}
				if (flag && !(ContextData<InteractionVariantData>.Current.VariantActor is InteractionRestrictionPart))
				{
					ContextData<InteractionVariantData>.Current.VariantActor.OnDidInteract(user);
				}
				if (UseAnimationState == UnitAnimationInteractionType.None && Settings.InteractionSound != null && Settings.InteractionSound != "")
				{
					SoundEventsManager.PostEvent(Settings.InteractionSound, View.Or(null)?.gameObject);
				}
				EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IInteractionHandler>)delegate(IInteractionHandler h)
				{
					h.OnInteract(this);
				}, isCheckRuntime: true);
				OnDidInteract(user);
			}
			else
			{
				foreach (InteractionRestrictionPart item2 in restrictions)
				{
					item2.OnFailedInteract(user);
				}
				if (flag && !(ContextData<InteractionVariantData>.Current.VariantActor is InteractionRestrictionPart))
				{
					ContextData<InteractionVariantData>.Current.VariantActor.OnFailedInteract(user);
				}
				if (Settings?.InteractionDisabledSound != null && Settings.InteractionDisabledSound != "")
				{
					SoundEventsManager.PostEvent(Settings.InteractionDisabledSound, View.Or(null)?.gameObject);
				}
				EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IInteractionHandler>)delegate(IInteractionHandler h)
				{
					h.OnInteractionRestricted(this);
				}, isCheckRuntime: true);
			}
			EventBus.RaiseEvent((IMapObjectEntity)Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectInteractChanged();
			}, isCheckRuntime: true);
		}
	}

	private List<BaseUnitEntity> GetMaxUnitsToInteract()
	{
		List<BaseUnitEntity> list = null;
		IEnumerable<ISkillUseRestrictionWithoutItem> enumerable = (from i in GetRestrictions()
			where i is ISkillUseRestrictionWithoutItem
			select i).OfType<ISkillUseRestrictionWithoutItem>();
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			foreach (ISkillUseRestrictionWithoutItem item2 in enumerable)
			{
				if (InteractionHelper.GetInteractionSkillCheckChance(item, item2.Skill, item2.DCOverrideValue) > 0)
				{
					if (list == null)
					{
						list = new List<BaseUnitEntity>();
					}
					list.Add(item);
				}
			}
		}
		return list;
	}

	private List<InteractionRestrictionPart> GetRestrictions()
	{
		if (!(this is IHasInteractionVariantActors) || !((IHasInteractionVariantActors)this).InteractThroughVariants || ContextData<InteractionVariantData>.Current?.VariantActor == null)
		{
			return Owner.Parts.GetAll<InteractionRestrictionPart>().ToList();
		}
		if (ContextData<InteractionVariantData>.Current?.VariantActor is InteractionRestrictionPart item)
		{
			return new List<InteractionRestrictionPart> { item };
		}
		return new List<InteractionRestrictionPart>();
	}

	private bool CanInteract(List<InteractionRestrictionPart> restrictions, BaseUnitEntity user)
	{
		if (this is IHasInteractionVariantActors && ((IHasInteractionVariantActors)this).InteractThroughVariants)
		{
			if (ContextData<InteractionVariantData>.Current?.VariantActor == null)
			{
				AlreadyUnlocked = true;
				return true;
			}
			if (ContextData<InteractionVariantData>.Current.VariantActor.CanInteract(user))
			{
				ContextData<InteractionVariantData>.Current.VariantActor.ShowSuccessBark(user);
				AlreadyUnlocked = true;
				return true;
			}
			if (Settings.Dialog != null && View != null)
			{
				DialogData data = DialogController.SetupDialogWithMapObject(Settings.Dialog, View, null, user);
				Game.Instance.DialogController.StartDialog(data);
			}
			ContextData<InteractionVariantData>.Current.VariantActor.ShowRestrictionBark(user);
			return false;
		}
		if (AlreadyUnlocked)
		{
			return true;
		}
		InteractionRestrictionPart interactionRestrictionPart = Enumerable.FirstOrDefault(restrictions);
		if (interactionRestrictionPart == null)
		{
			AlreadyUnlocked = true;
			return true;
		}
		foreach (InteractionRestrictionPart restriction in restrictions)
		{
			if (restriction.CheckRestriction(user))
			{
				restriction.ShowSuccessBark(user);
				return true;
			}
		}
		if (Settings.Dialog != null && View != null)
		{
			DialogData data2 = DialogController.SetupDialogWithMapObject(Settings.Dialog, View, null, user);
			Game.Instance.DialogController.StartDialog(data2);
		}
		interactionRestrictionPart.ShowRestrictionBark(user);
		return false;
	}

	public virtual bool CheckTechUse()
	{
		return Owner.GetOptional<TechUseRestrictionPart>() != null;
	}

	public virtual bool CanInteract()
	{
		if (Enabled && (!Settings.NotInCombat || !Game.Instance.TurnController.TbActive))
		{
			if (!Settings.UnlimitedInteractionsPerRound)
			{
				return !AlreadyInteractedInThisCombatRound;
			}
			return true;
		}
		return false;
	}

	public virtual void OnUnitLeftProximity(BaseUnitEntity unit)
	{
	}

	protected abstract void OnInteract(BaseUnitEntity user);

	protected virtual void OnDidInteract(BaseUnitEntity user)
	{
	}

	public bool HasVisibleTrap()
	{
		TrapObjectView trap = Settings.Trap;
		if (trap != null && trap.Data != null && trap.Data.IsAwarenessCheckPassed)
		{
			return trap.Data.TrapActive;
		}
		return false;
	}

	[CanBeNull]
	public virtual BaseUnitEntity SelectUnit(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, StatType? skillFromVariant = null)
	{
		return SelectUnitInternal(units.Where((BaseUnitEntity u) => CanBeSelected(u)).ToTempList(), muteEvents, skillFromVariant);
	}

	public virtual List<BaseUnitEntity> SelectAllUnits(ReadonlyList<BaseUnitEntity> units)
	{
		return units.Where((BaseUnitEntity u) => CanBeSelected(u)).ToTempList();
	}

	private bool CanBeSelected(BaseUnitEntity unit)
	{
		if (unit.CanAct)
		{
			if (!unit.CanMove)
			{
				return IsEnoughCloseForInteraction(unit);
			}
			return true;
		}
		return false;
	}

	public BaseUnitEntity SelectUnitInternal(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, StatType? skill = null)
	{
		if (units.Count <= 0)
		{
			return null;
		}
		if (Settings.Trap != null && HasVisibleTrap())
		{
			BaseUnitEntity baseUnitEntity = Settings.Trap.Data.SelectUnit(units, muteEvents);
			if (baseUnitEntity != null)
			{
				return baseUnitEntity;
			}
		}
		List<InteractionRestrictionPart> list = GetRestrictions();
		if (skill.HasValue)
		{
			list = list.Where((InteractionRestrictionPart r) => (r as IInteractionVariantActor)?.Skill == skill.Value).ToList();
		}
		foreach (InteractionRestrictionPart item in list)
		{
			BaseUnitEntity result = null;
			int num = -1;
			foreach (BaseUnitEntity item2 in units)
			{
				if (units.Count <= 1 || !item2.IsPet)
				{
					int userPriority = item.GetUserPriority(item2);
					if (userPriority > num)
					{
						num = userPriority;
						result = item2;
					}
				}
			}
			if (num >= 0)
			{
				if (num == 0)
				{
					break;
				}
				return result;
			}
		}
		Vector3 p = View.Or(null)?.ViewTransform.position ?? Vector3.zero;
		if (units.Count <= 1)
		{
			return units.FirstOrDefault();
		}
		return units.Where((BaseUnitEntity u) => !u.IsPet).Aggregate((BaseUnitEntity u1, BaseUnitEntity u2) => (!(u1.SqrDistanceTo(p) <= u2.SqrDistanceTo(p))) ? u2 : u1);
	}

	public void PlayStartSound(BaseUnitEntity user)
	{
		if (UseAnimationState != 0)
		{
			SoundEventsManager.PostEvent(Settings.InteractionSound, View.Or(null)?.gameObject);
		}
	}

	public virtual bool IsEnoughCloseForInteraction(BaseUnitEntity unit, Vector3? position = null)
	{
		return IsEnoughCloseForInteraction(position ?? unit.Position);
	}

	public bool IsEnoughCloseForInteraction(Vector3 unitPosition)
	{
		return Mathf.RoundToInt(GeometryUtils.MechanicsDistance(ObstacleAnalyzer.GetNearestNode(unitPosition).node.Vector3Position, Owner.Position) / GraphParamsMechanicsCache.GridCellSize) <= ApproachRadius;
	}

	public bool IsEnoughCloseForInteractionFromDesiredPosition(BaseUnitEntity unit)
	{
		Vector3 desiredPosition = Game.Instance.VirtualPositionController.GetDesiredPosition(unit);
		return IsEnoughCloseForInteraction(unit, desiredPosition);
	}

	public bool HasEnoughActionPoints(BaseUnitEntity unit)
	{
		if (Game.Instance.TurnController.TbActive)
		{
			return unit.CombatState.ActionPointsYellow >= ActionPointsCost;
		}
		return true;
	}

	public virtual void HandleDestructionSuccess(MapObjectView mapObjectView)
	{
		if (mapObjectView == View)
		{
			AlreadyUnlocked = true;
		}
	}

	public virtual void HandleDestructionFail(MapObjectView mapObjectView)
	{
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		m_LastCombatRoundInteractionAttempt = -1;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		HashSet<UnitReference> playersClose = PlayersClose;
		if (playersClose != null)
		{
			int num = 0;
			foreach (UnitReference item in playersClose)
			{
				UnitReference obj = item;
				num ^= UnitReferenceHasher.GetHash128(ref obj).GetHashCode();
			}
			result.Append(num);
		}
		result.Append(ref AlreadyUnlocked);
		result.Append(ref m_Enabled);
		result.Append(ref m_LastCombatRoundInteractionAttempt);
		return result;
	}
}
