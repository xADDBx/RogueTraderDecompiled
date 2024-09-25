using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Newtonsoft.Json;
using Owlcat.Runtime.UI.Tooltips;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotActivableAbility : MechanicActionBarSlot, IHashable
{
	[JsonProperty]
	public ActivatableAbility ActivatableAbility;

	private ActivatableAbilityResourceLogic m_CachedResourceLogic;

	protected override bool IsNotAvailable => !ActivatableAbility.IsAvailable;

	public override string KeyName => ActivatableAbility?.Blueprint?.name;

	public override bool IsBad()
	{
		if (!base.IsBad() && ActivatableAbility != null)
		{
			return !ActivatableAbility.Active;
		}
		return true;
	}

	public override bool IsDisabled(int resourceCount)
	{
		return base.IsDisabled(resourceCount);
	}

	public override void OnClick()
	{
		base.OnClick();
		if (IsPossibleActive)
		{
			TrySetAbilityToPrediction(state: true);
			ActivatableAbility.IsOn = !ActivatableAbility.IsOn;
		}
	}

	public override void OnHover(bool state)
	{
		base.OnHover(state);
	}

	public override int GetResource()
	{
		return ActivatableAbility.ResourceCount ?? (-1);
	}

	public override int GetResourceCost()
	{
		return -1;
	}

	public override int GetResourceAmount()
	{
		return -1;
	}

	public override Sprite GetIcon()
	{
		return ActivatableAbility.Icon;
	}

	public override string GetTitle()
	{
		return ActivatableAbility.Name;
	}

	public override string GetDescription()
	{
		return ActivatableAbility.Description;
	}

	public override bool IsActive()
	{
		return ActivatableAbility.IsOn;
	}

	public override bool IsCasting()
	{
		if (ActivatableAbility.IsOn)
		{
			return false;
		}
		if (base.Unit.Commands.Current is UnitActivateAbility unitActivateAbility)
		{
			return unitActivateAbility.Ability == ActivatableAbility;
		}
		return false;
	}

	protected override bool CanUseIfTurnBased()
	{
		if (base.CanUseIfTurnBased())
		{
			return ActivatableAbility.IsOn;
		}
		return false;
	}

	public override object GetContentData()
	{
		return ActivatableAbility;
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		return new TooltipTemplateActivatableAbility(ActivatableAbility);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ActivatableAbility>.GetHash128(ActivatableAbility);
		result.Append(ref val2);
		return result;
	}
}
