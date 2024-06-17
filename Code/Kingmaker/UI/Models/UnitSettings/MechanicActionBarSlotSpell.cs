using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.Units;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Owlcat.Runtime.UI.Tooltips;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public abstract class MechanicActionBarSlotSpell : MechanicActionBarSlot, IHashable
{
	[CanBeNull]
	public abstract AbilityData Spell { get; }

	public override string KeyName => Spell?.Blueprint?.name;

	protected override bool IsNotAvailable
	{
		get
		{
			if (Spell == null || Spell.IsVariable)
			{
				return true;
			}
			return !Spell.IsAvailable;
		}
	}

	public override bool IsDisabled(int resourceCount)
	{
		if (base.IsDisabled(resourceCount))
		{
			return true;
		}
		return !Spell.IsAvailable;
	}

	public override void OnClick()
	{
		base.OnClick();
		if (IsPossibleActive && !(Spell == null))
		{
			if (Spell.TargetAnchor != 0)
			{
				Game.Instance.SelectedAbilityHandler.SetAbility(Spell);
			}
			else
			{
				UnitCommandsRunner.TryUnitUseAbility(Spell, base.Unit);
			}
		}
	}

	public override void OnHover(bool state)
	{
		EventBus.RaiseEvent(delegate(IAbilityTargetHoverUIHandler h)
		{
			h.HandleAbilityTargetHover(Spell, state);
		});
		if (!(Spell != null))
		{
			return;
		}
		if (state)
		{
			if (Spell.TargetAnchor == AbilityTargetAnchor.Owner)
			{
				EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
				{
					h.HandleAoEMove(base.Unit.Position, Spell);
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

	public override bool IsBad()
	{
		if (!(Spell == null))
		{
			return Spell.Blueprint == null;
		}
		return true;
	}

	public override int GetResource()
	{
		return Spell?.GetAvailableForCastCount() ?? 0;
	}

	public override int GetResourceCost()
	{
		return Spell?.GetResourceCost() ?? (-1);
	}

	public override int GetResourceAmount()
	{
		return Spell?.GetResourceAmount() ?? (-1);
	}

	public override bool IsWeaponAttackThatRequiresAmmo()
	{
		return Spell?.IsWeaponAttackThatRequiresAmmo ?? false;
	}

	public override int MaxAmmo()
	{
		return Spell?.Weapon?.Blueprint.WarhammerMaxAmmo ?? (-1);
	}

	public override int CurrentAmmo()
	{
		return Spell?.AmmoRequired ?? (-1);
	}

	public override Sprite GetIcon()
	{
		return Spell?.Icon;
	}

	public override string GetTitle()
	{
		return Spell?.Name ?? "";
	}

	public override string GetDescription()
	{
		return Spell?.ShortenedDescription ?? "";
	}

	public override bool IsCasting()
	{
		if (IsBad())
		{
			return false;
		}
		if (base.Unit.Commands.Current is UnitUseAbility unitUseAbility)
		{
			return unitUseAbility.Ability.Blueprint == Spell?.Blueprint;
		}
		return false;
	}

	protected override string WarningMessage(Vector3 castPosition)
	{
		return Spell?.GetUnavailableReason(castPosition);
	}

	public override object GetContentData()
	{
		return Spell;
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		return new TooltipTemplateAbility(Spell);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
