using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.UI.Tooltips;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotAbility : MechanicActionBarSlot, IHashable
{
	[JsonProperty]
	private EntityFactRef<Ability> m_AbilityRef;

	public AbilityData Ability
	{
		get
		{
			return m_AbilityRef.Fact?.Data;
		}
		set
		{
			m_AbilityRef = value.Fact;
		}
	}

	private bool IsVariantAbility => Ability?.IsVariable ?? false;

	public override string KeyName => Ability?.Blueprint?.name;

	protected override bool IsNotAvailable
	{
		get
		{
			if (Ability == null)
			{
				return true;
			}
			if (Ability.Caster == null)
			{
				return true;
			}
			if (IsVariantAbility)
			{
				return Ability.IsOnCooldown;
			}
			return !Ability.IsAvailable;
		}
	}

	public bool IsSameAbility(Ability other)
	{
		return m_AbilityRef == other;
	}

	public override bool IsBad()
	{
		if (!base.IsBad() && Ability?.Fact != null)
		{
			return Ability?.Caster == null;
		}
		return true;
	}

	public override bool IsDisabled(int resourceCount)
	{
		if (base.Unit.LifeState.IsConscious)
		{
			if (base.Unit.IsInCombat)
			{
				return base.Unit.View.IsMoving();
			}
			return false;
		}
		return true;
	}

	public override void OnClick()
	{
		base.OnClick();
		if (!IsPossibleActive)
		{
			return;
		}
		TrySetAbilityToPrediction(state: true);
		if (Ability.TargetAnchor != 0)
		{
			Game.Instance.SelectedAbilityHandler.SetAbility(Ability);
			return;
		}
		if (TurnController.IsInTurnBasedCombat())
		{
			UnitCommandsRunner.CancelMoveCommand();
		}
		UnitCommandsRunner.TryUnitUseAbility(Ability, base.Unit);
		EventBus.RaiseEvent(delegate(IAbilityOwnerTargetSelectionHandler h)
		{
			h.HandleOwnerAbilitySelected(Ability);
		});
	}

	public override void OnHover(bool state)
	{
		base.OnHover(state);
		if (Ability == null)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IAbilityTargetHoverUIHandler h)
		{
			h.HandleAbilityTargetHover(Ability, state);
		});
		if (state)
		{
			if (Ability.TargetAnchor == AbilityTargetAnchor.Owner)
			{
				EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
				{
					h.HandleAoEMove(base.Unit.Position, Ability);
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

	public override int GetResource()
	{
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability?.GetAvailableForCastCount() ?? 0;
	}

	public override int GetResourceCost()
	{
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability?.GetResourceCost() ?? 0;
	}

	public override int GetResourceAmount()
	{
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability?.GetResourceAmount() ?? 0;
	}

	public override int ActionPointCost()
	{
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability?.CalculateActionPointCost() ?? 0;
	}

	public override bool IsWeaponAttackThatRequiresAmmo()
	{
		return Ability?.IsWeaponAttackThatRequiresAmmo ?? false;
	}

	public override int MaxAmmo()
	{
		return Ability?.Weapon?.Blueprint.WarhammerMaxAmmo ?? (-1);
	}

	public override int CurrentAmmo()
	{
		return Ability?.AmmoRequired ?? (-1);
	}

	public override Sprite GetIcon()
	{
		return Ability?.Icon;
	}

	public override string GetTitle()
	{
		return Ability?.Name;
	}

	public override string GetDescription()
	{
		return Ability?.ShortenedDescription;
	}

	public override bool HasWeaponAbilityGroup()
	{
		if (Ability == null)
		{
			return false;
		}
		foreach (BlueprintAbilityGroup abilityGroup in Ability.Blueprint.AbilityGroups)
		{
			if (abilityGroup.NameSafe() == "WeaponAttackAbilityGroup")
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsCasting()
	{
		if (base.Unit?.Commands?.Current is UnitUseAbility unitUseAbility)
		{
			if (unitUseAbility.Ability.Blueprint == Ability.Blueprint || unitUseAbility.Ability.Blueprint.Parent == Ability.Blueprint)
			{
				return unitUseAbility.Result == AbstractUnitCommand.ResultType.None;
			}
			return false;
		}
		return false;
	}

	public override object GetContentData()
	{
		return Ability;
	}

	protected override string WarningMessage(Vector3 castPosition)
	{
		string text = base.WarningMessage(castPosition);
		if (string.IsNullOrEmpty(text))
		{
			return Ability?.GetUnavailableReason(castPosition);
		}
		return text;
	}

	public override IEnumerable<AbilityData> GetConvertedAbilityData()
	{
		if (!(Ability != null))
		{
			return base.GetConvertedAbilityData();
		}
		return Ability.GetConversions();
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		if (!(Ability != null))
		{
			return null;
		}
		return new TooltipTemplateAbility(Ability);
	}

	public override int AmmoCost()
	{
		return Ability?.AmmoRequired ?? 0;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityFactRef<Ability> obj = m_AbilityRef;
		Hash128 val2 = StructHasher<EntityFactRef<Kingmaker.UnitLogic.Abilities.Ability>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
