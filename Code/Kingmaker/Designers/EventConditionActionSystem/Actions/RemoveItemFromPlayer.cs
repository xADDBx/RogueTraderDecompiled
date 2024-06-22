using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
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

[ComponentName("Actions/RemoveItemFromPlayer")]
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
			Element.LogError("{0}: Want remove {1} {2} from the player.", new object[3]
			{
				"RemoveItemFromPlayer",
				GetAmount(),
				Money ? "Coins" : $"Items ({ItemToRemove}"
			});
			BlueprintItem itemToRemove = ItemToRemove;
			bool flag = Money || (bool)ItemToRemove.GetComponent<MoneyReplacement>();
			long num = (flag ? Game.Instance.Player.Money : GameHelper.GetPlayerCharacter().Inventory.Count((ItemEntity i) => i.Blueprint == itemToRemove));
			long num2 = (RemoveAll ? num : (Quantity + (long)((decimal)num / 100.0m * (decimal)Percentage + 0.5m)));
			if (num < 0)
			{
				Element.LogError(this, "{0}: Player has {1} {2}, that's a negative amount. Will remove nothing.", "RemoveItemFromPlayer", num, Money ? ((object)"Coins") : ((object)itemToRemove));
				return;
			}
			if (num2 < 0)
			{
				Element.LogError(this, "{0}: Trying to remove {1} {2}, that's a negative amount. Will remove nothing.", "RemoveItemFromPlayer", num2, Money ? ((object)"Coins") : ((object)itemToRemove));
				return;
			}
			if (num2 > num)
			{
				Element.LogInfo(this, "{0}: Trying to remove {1} {2}, but player has only {3}. Will remove only the amount the player has.", "RemoveItemFromPlayer", num2, Money ? ((object)"Coins") : ((object)itemToRemove), num);
				num2 = num;
			}
			if (num2 == 0L)
			{
				Element.LogInfo(this, "{0}: Will remove no {1} from the player.", "RemoveItemFromPlayer", Money ? ((object)"Coins") : ((object)itemToRemove));
				return;
			}
			GameHelper.GetPlayerCharacter().Inventory.Remove(itemToRemove, (int)num2);
			if (flag)
			{
				Game.Instance.Statistic.HandleMoneyFlow(base.Owner?.name ?? "RemoveItemFromPlayer", "RemoveCoins", GameStatistic.MoneyFlowStatistic.ActionType.Quest, num2);
			}
		}
	}
}
