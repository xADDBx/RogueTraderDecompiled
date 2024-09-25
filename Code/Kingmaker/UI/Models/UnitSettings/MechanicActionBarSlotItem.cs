using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotItem : MechanicActionBarSlot, IHashable
{
	[JsonProperty]
	public ItemEntityUsable Item;

	public Ability Ability;

	public override string KeyName => Item?.Blueprint?.name;

	protected override bool IsNotAvailable => !(Ability?.Data.IsAvailable ?? false);

	public override bool IsBad()
	{
		if (!base.IsBad() && Item?.Wielder != null)
		{
			return Item?.Wielder != base.Unit;
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
		if (Ability == null)
		{
			return;
		}
		AbilityData itemAbility = Ability.Data;
		if (itemAbility.TargetAnchor != 0)
		{
			Game.Instance.SelectedAbilityHandler.SetAbility(itemAbility);
			return;
		}
		if (TurnController.IsInTurnBasedCombat())
		{
			UnitCommandsRunner.CancelMoveCommand();
		}
		UnitCommandsRunner.TryUnitUseAbility(itemAbility, base.Unit);
		EventBus.RaiseEvent(delegate(IAbilityOwnerTargetSelectionHandler h)
		{
			h.HandleOwnerAbilitySelected(itemAbility);
		});
	}

	public override bool IsDisabled(int resourceCount)
	{
		if (resourceCount != 0)
		{
			return base.IsDisabled(resourceCount);
		}
		return true;
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
			h.HandleAbilityTargetHover(Ability.Data, state);
		});
		if (state)
		{
			if (Ability.Data.TargetAnchor == AbilityTargetAnchor.Owner)
			{
				EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
				{
					h.HandleAoEMove(base.Unit.Position, Ability.Data);
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

	public override int ActionPointCost()
	{
		if (Ability != null)
		{
			return Ability.Data?.CalculateActionPointCost() ?? (-1);
		}
		return base.ActionPointCost();
	}

	public override int GetResource()
	{
		if (!base.Unit.Body.QuickSlots.Any((UsableSlot s) => s.HasItem && s.Item == Item))
		{
			if (!Item.Blueprint.SpendCharges)
			{
				return -1;
			}
			return 0;
		}
		if (!Item.Blueprint.SpendCharges)
		{
			return -1;
		}
		if (Item.Count <= 1)
		{
			return Item.Charges;
		}
		return Item.Count;
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
		return Item.Icon;
	}

	public override string GetTitle()
	{
		return Item.Name;
	}

	public override string GetDescription()
	{
		return Ability?.Blueprint.GetShortenedDescription() ?? Item.Description;
	}

	public override bool IsCasting()
	{
		if (base.Unit.Commands.Current is UnitUseAbility unitUseAbility)
		{
			return unitUseAbility.Ability.SourceItem == Item;
		}
		return false;
	}

	public override object GetContentData()
	{
		return Item;
	}

	public override IEnumerable<AbilityData> GetConvertedAbilityData()
	{
		if (Item.Blueprint.Type == UsableItemType.Potion)
		{
			if (Ability == null)
			{
				return base.GetConvertedAbilityData();
			}
			AbilityData item = new AbilityData(Ability)
			{
				PotionForOther = true
			};
			List<AbilityData> list = TempList.Get<AbilityData>();
			list.Add(item);
			return list;
		}
		return base.GetConvertedAbilityData();
	}

	protected override bool CanUseIfTurnBased()
	{
		if (!base.CanUseIfTurnBased())
		{
			return false;
		}
		return Ability.Data != null;
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		return new TooltipTemplateItem(Item);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemEntityUsable>.GetHash128(Item);
		result.Append(ref val2);
		return result;
	}
}
