using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Newtonsoft.Json;
using Owlcat.Runtime.UI.Tooltips;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public abstract class MechanicActionBarSlot : IRoundEndHandler, ISubscriber, IHashable
{
	[JsonProperty]
	private EntityRef<BaseUnitEntity> m_UnitRef;

	private bool m_IsCastingActive;

	private bool m_IsUnlocked = true;

	public bool HoverState;

	public BaseUnitEntity Unit
	{
		get
		{
			return m_UnitRef.Entity;
		}
		set
		{
			m_UnitRef = value;
		}
	}

	public virtual string KeyName { get; }

	protected virtual bool IsNotAvailable => false;

	protected virtual bool IsPossibleToConvertWhileNotAvailable => false;

	private bool IsPlayerInputLocked => Game.Instance.PlayerInputInCombatController.IsLocked;

	protected int ResourceCount { get; private set; }

	protected int ResourceCost { get; private set; }

	protected int ResourceAmount { get; private set; }

	public virtual bool IsPossibleActive
	{
		get
		{
			bool flag = !IsDisabled(GetResource()) && !IsPlayerInputLocked && !IsNotAvailable && (!TurnController.IsInTurnBasedCombat() || CanUseIfTurnBased());
			if (flag && UINetUtility.InLobbyAndPlaying)
			{
				flag = ((Game.Instance.CurrentMode == GameModeType.SpaceCombat) ? UINetUtility.IsControlMainCharacter() : (Unit != null && Unit.IsMyNetRole()));
			}
			return flag;
		}
	}

	public bool IsPossibleActiveWithoutNetRole
	{
		get
		{
			if (!IsDisabled(GetResource()) && !IsNotAvailable)
			{
				if (TurnController.IsInTurnBasedCombat())
				{
					return CanUseIfTurnBased();
				}
				return true;
			}
			return false;
		}
	}

	public bool IsPossibleToConvert
	{
		get
		{
			if (!IsPossibleActive && IsPossibleToConvertWhileNotAvailable && m_IsUnlocked)
			{
				return true;
			}
			return IsPossibleActive;
		}
	}

	public virtual bool IsDisabled(int resourceCount)
	{
		if (!Unit.LifeState.IsConscious)
		{
			return true;
		}
		if ((bool)Unit.GetMechanicFeature(MechanicsFeatureType.ForceAIControl))
		{
			return true;
		}
		if (resourceCount == -1)
		{
			return false;
		}
		return resourceCount == 0;
	}

	private static bool CanEndTurnAndNoActing()
	{
		if (Game.Instance.TurnController.CurrentUnit is BaseUnitEntity { IsDirectlyControllable: not false } baseUnitEntity)
		{
			return baseUnitEntity.Commands.Empty;
		}
		return false;
	}

	protected virtual bool CanUseIfTurnBased()
	{
		if (Game.Instance.TurnController.CurrentUnit != Unit || Unit.State.IsProne || !CanEndTurnAndNoActing())
		{
			return false;
		}
		return true;
	}

	public virtual void OnClick()
	{
		PlaySound();
		TryShowWarning(Game.Instance.VirtualPositionController.GetDesiredPosition(Unit));
		EventBus.RaiseEvent(delegate(IClickMechanicActionBarSlotHandler h)
		{
			h.HandleClickMechanicActionBarSlot(this);
		});
		m_IsUnlocked = false;
	}

	public void PlaySound()
	{
		UISounds.Instance.Play(IsPossibleActive ? UISounds.Instance.Sounds.Combat.ActionBarSlotClick : UISounds.Instance.Sounds.Combat.ActionBarCanNotSlotClick);
	}

	public virtual void TryShowWarning(Vector3 castPosition)
	{
		if (IsPossibleActive)
		{
			return;
		}
		string message = WarningMessage(castPosition);
		if (!string.IsNullOrEmpty(message))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(message, addToLog: true, WarningNotificationFormat.Attention);
			});
		}
	}

	protected virtual string WarningMessage(Vector3 castPosition)
	{
		if (!CanUseIfTurnBased())
		{
			return UIStrings.Instance.TurnBasedTexts.NotEnoughActionsMessage;
		}
		return string.Empty;
	}

	public virtual bool IsActive()
	{
		return false;
	}

	public virtual int ActionPointCost()
	{
		return -1;
	}

	public virtual void OnHover(bool state)
	{
		HoverState = state;
	}

	public abstract int GetResource();

	public abstract int GetResourceCost();

	public abstract int GetResourceAmount();

	public virtual bool IsWeaponAttackThatRequiresAmmo()
	{
		return false;
	}

	public virtual int MaxAmmo()
	{
		return -1;
	}

	public virtual int CurrentAmmo()
	{
		return -1;
	}

	public virtual bool HasWeaponAbilityGroup()
	{
		return false;
	}

	public abstract object GetContentData();

	public virtual bool IsBad()
	{
		return false;
	}

	public abstract Sprite GetIcon();

	public virtual Sprite GetForeIcon()
	{
		return null;
	}

	public virtual bool NeedUpdate()
	{
		return true;
	}

	public abstract string GetTitle();

	public abstract string GetDescription();

	public void UpdateResourceCount()
	{
		if (Unit != null)
		{
			ResourceCount = GetResource();
		}
	}

	public void UpdateResourceCost()
	{
		if (Unit != null)
		{
			ResourceCost = GetResourceCost();
		}
	}

	public void UpdateResourceAmount()
	{
		if (Unit != null)
		{
			ResourceAmount = GetResourceAmount();
		}
	}

	public abstract bool IsCasting();

	public virtual string GetCountText(int resourceCount)
	{
		int resource = GetResource();
		if (resource != -1)
		{
			return resource.ToString();
		}
		return string.Empty;
	}

	public virtual IEnumerable<AbilityData> GetConvertedAbilityData()
	{
		return Enumerable.Empty<AbilityData>();
	}

	public virtual int GetLevel()
	{
		return -1;
	}

	public virtual SpellSchool GetSpellSchool()
	{
		return SpellSchool.None;
	}

	public void TrySetAbilityToPrediction(bool state)
	{
	}

	public virtual TooltipBaseTemplate GetTooltipTemplate()
	{
		return null;
	}

	public virtual void OnAutoUseToggle()
	{
	}

	public virtual int AmmoCost()
	{
		return -1;
	}

	protected void TriggerAbilityHoverEvents(AbilityData abilityData, bool state)
	{
		AbilityData targetingAbility = abilityData.InitialTargetAbility;
		EventBus.RaiseEvent(delegate(IAbilityTargetHoverUIHandler h)
		{
			h.HandleAbilityTargetHover(targetingAbility, state);
		});
		if (state)
		{
			if (abilityData.TargetAnchor == AbilityTargetAnchor.Owner)
			{
				EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
				{
					h.HandleAoEMove(Unit.Position, abilityData);
				});
			}
		}
		else
		{
			EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
			{
				h.HandleAoECancel();
			});
		}
	}

	public void HandleRoundEnd(bool isTurnBased, bool isFirst)
	{
		if (isTurnBased)
		{
			m_IsUnlocked = true;
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<BaseUnitEntity> obj = m_UnitRef;
		Hash128 val = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		return result;
	}
}
