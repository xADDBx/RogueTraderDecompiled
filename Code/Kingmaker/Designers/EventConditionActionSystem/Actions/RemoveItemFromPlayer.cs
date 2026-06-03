using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Group("Actions")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("ef95139bce5938c48b2997497ab811af")]
public class RemoveItemFromPlayer : GameAction
{
	[ValidateNotNull]
	[HideIf("RemoveAll")]
	public bool Money;

	[HideIf("Money")]
	public bool RemoveAll;

	[HideIf("Money")]
	[SerializeField]
	[FormerlySerializedAs("ItemToRemove")]
	private BlueprintItemReference m_ItemToRemove;

	[SerializeField]
	private bool m_Silent;

	[HideIf("RemoveAll")]
	[ValidatePositiveOrZeroNumber]
	public int Quantity = 1;

	[HideIf("RemoveAll")]
	[Range(0f, 100f)]
	public float Percentage;

	public BlueprintItem ItemToRemove => m_ItemToRemove?.Get();

	public bool Silent => m_Silent;

	public override string GetDescription()
	{
		return "Отнимает у игрока указанные предметы.\nМожно отнять все такие предметы или заданное кол-во.\nМожно отнять деньги";
	}

	public override string GetCaption()
	{
		if (Quantity == 0 && Mathf.Approximately(Percentage, 0f))
		{
			if (!Money)
			{
				return $"Remove no Item ({ItemToRemove}) from player";
			}
			return "Remove no Coins from player";
		}
		if (Quantity == 1 && Mathf.Approximately(Percentage, 0f))
		{
			if (!Money)
			{
				return $"Remove Item ({ItemToRemove}) from player";
			}
			return "Remove Coins x1 from player";
		}
		if (!Money)
		{
			return $"Remove Item ({ItemToRemove} {GetAmount()}) from player";
		}
		return "Remove Coins " + GetAmount() + " from player";
	}

	private string GetAmount()
	{
		return ((Quantity != 0) ? $"x{Quantity}" : "") + ((Quantity != 0 && !Mathf.Approximately(Percentage, 0f)) ? " + " : "") + ((Percentage != 0f) ? $"{Percentage}%" : "");
	}

	protected override void RunAction()
	{
		using (ContextData<GameLogDisabled>.RequestIf(m_Silent))
		{
			Element.LogInfo("{0}: Want remove {1} {2} from the player.", new object[3]
			{
				"RemoveItemFromPlayer",
				GetAmount(),
				Money ? "Coins" : $"Items ({ItemToRemove}"
			});
			BlueprintItem itemToRemove = ItemToRemove;
			int num;
			long num2;
			if (!Money)
			{
				num = (((bool)ItemToRemove.GetComponent<MoneyReplacement>()) ? 1 : 0);
				if (num == 0)
				{
					num2 = GameHelper.GetPlayerCharacter().Inventory.Count((ItemEntity i) => i.Blueprint == itemToRemove);
					goto IL_00ac;
				}
			}
			else
			{
				num = 1;
			}
			num2 = Game.Instance.Player.Money;
			goto IL_00ac;
			IL_00ac:
			long num3 = num2;
			long num4 = ((num == 0) ? CountItemsInCargo(itemToRemove) : 0);
			long num5 = (RemoveAll ? (num3 + num4) : (Quantity + (long)((decimal)num3 / 100.0m * (decimal)Percentage + 0.5m)));
			long num6 = num3 + num4;
			if (num6 < 0)
			{
				Element.LogError(this, "{0}: Player has {1} {2}, that's a negative amount. Will remove nothing.", "RemoveItemFromPlayer", num6, Money ? ((object)"Coins") : ((object)itemToRemove));
				return;
			}
			if (num5 < 0)
			{
				Element.LogError(this, "{0}: Trying to remove {1} {2}, that's a negative amount. Will remove nothing.", "RemoveItemFromPlayer", num5, Money ? ((object)"Coins") : ((object)itemToRemove));
				return;
			}
			if (num5 > num6)
			{
				Element.LogInfo(this, "{0}: Trying to remove {1} {2}, but player has only {3}. Will remove only the amount the player has.", "RemoveItemFromPlayer", num5, Money ? ((object)"Coins") : ((object)itemToRemove), num6);
				num5 = num6;
			}
			if (num5 == 0L)
			{
				Element.LogInfo(this, "{0}: Will remove no {1} from the player.", "RemoveItemFromPlayer", Money ? ((object)"Coins") : ((object)itemToRemove));
			}
			else
			{
				int num7 = (int)Mathf.Min(num3, num5);
				RemoveFromInventory(itemToRemove, num7);
				RemoveFromCargo(itemToRemove, (int)(num5 - num7));
			}
		}
	}

	private int CountItemsInCargo(BlueprintItem item)
	{
		int num = 0;
		foreach (CargoEntity cargoEntity in Game.Instance.Player.CargoState.CargoEntities)
		{
			foreach (ItemEntity item2 in cargoEntity.Inventory)
			{
				if (item2.Blueprint == item)
				{
					num++;
				}
			}
		}
		return num;
	}

	private void RemoveFromInventory(BlueprintItem item, int quantity)
	{
		if (quantity > 0)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(item, quantity);
		}
	}

	private void RemoveFromCargo(BlueprintItem item, int quantity)
	{
		if (quantity <= 0)
		{
			return;
		}
		foreach (CargoEntity cargoEntity in Game.Instance.Player.CargoState.CargoEntities)
		{
			foreach (ItemEntity item2 in cargoEntity.Inventory)
			{
				if (item2.Blueprint == item)
				{
					cargoEntity.Inventory.Remove(item2);
					quantity--;
					if (quantity <= 0)
					{
						return;
					}
				}
			}
		}
	}
}
