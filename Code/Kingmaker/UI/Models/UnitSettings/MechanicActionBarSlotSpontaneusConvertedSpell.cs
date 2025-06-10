using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.Units;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.UnitSettings.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Newtonsoft.Json;
using Owlcat.Runtime.UI.Tooltips;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotSpontaneusConvertedSpell : MechanicActionBarSlot, IHashable
{
	[JsonProperty]
	public AbilityData Spell;

	public override string KeyName
	{
		get
		{
			object obj = Spell?.Blueprint?.GetComponent<ActionPanelLogic>()?.UseKeyNameFromFact?.Get()?.name;
			if (obj == null)
			{
				AbilityData spell = Spell;
				if ((object)spell == null)
				{
					return null;
				}
				BlueprintAbility blueprint = spell.Blueprint;
				if (blueprint == null)
				{
					return null;
				}
				obj = blueprint.name;
			}
			return (string)obj;
		}
	}

	protected override bool IsNotAvailable
	{
		get
		{
			if (Spell == null)
			{
				return true;
			}
			return !Spell.IsAvailable;
		}
	}

	public override bool IsDisabled(int resourceCount)
	{
		return !base.Unit.LifeState.IsConscious;
	}

	public override void OnClick()
	{
		base.OnClick();
		if (IsPossibleActive)
		{
			TrySetAbilityToPrediction(state: true);
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
		base.OnHover(state);
		EventBus.RaiseEvent(delegate(IAbilityTargetHoverUIHandler h)
		{
			h.HandleAbilityTargetHover(Spell.InitialTargetAbility, state);
		});
	}

	public override int GetResource()
	{
		return Spell.GetAvailableForCastCount();
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
		return Spell.Icon;
	}

	public override string GetTitle()
	{
		return Spell.Name;
	}

	public override string GetDescription()
	{
		return Spell.ShortenedDescription;
	}

	public override Sprite GetForeIcon()
	{
		return (Spell.ConvertedFrom ?? Spell).Blueprint.Icon;
	}

	public override bool IsCasting()
	{
		if (base.Unit.Commands.Current is UnitUseAbility unitUseAbility)
		{
			if (unitUseAbility.Ability.Blueprint != Spell.Blueprint)
			{
				return unitUseAbility.Ability.Blueprint.Parent == Spell.Blueprint;
			}
			return true;
		}
		return false;
	}

	public override object GetContentData()
	{
		return Spell;
	}

	protected override string WarningMessage(Vector3 castPosition)
	{
		string text = base.WarningMessage(castPosition);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return Spell?.GetUnavailableReason(castPosition);
	}

	protected override bool CanUseIfTurnBased()
	{
		if (base.CanUseIfTurnBased())
		{
			return Spell != null;
		}
		return false;
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
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(Spell);
		result.Append(ref val2);
		return result;
	}
}
