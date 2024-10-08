using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("2101f4c178a0418bafc746c0e33bccd2")]
public class AddPremiumReward : GameAction
{
	[SerializeField]
	private BlueprintDlcRewardReference m_DlcReward;

	public List<BlueprintItemReference> Items;

	public List<BlueprintFeatureReference> PlayerFeatures;

	public ActionList AdditionalActions;

	public BlueprintDlcReward DlcReward => m_DlcReward;

	private bool MaybeAdd(ItemsCollection inventory = null)
	{
		Player player = Game.Instance.Player;
		if (player.ClaimedDlcRewards.HasItem(DlcReward))
		{
			Element.LogInfo("Premium reward already received: {0}", DlcReward);
			return false;
		}
		if (!DlcReward.IsAvailable)
		{
			Element.LogInfo("Premium reward is not enabled: {0}", DlcReward);
			return false;
		}
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		if (inventory == null)
		{
			inventory = playerCharacter.Inventory.Collection;
		}
		foreach (BlueprintItemReference item in Items)
		{
			if (!inventory.Contains(item.Get()) && !playerCharacter.Inventory.Contains(item.Get()))
			{
				inventory.Add(item.Get());
			}
		}
		foreach (BlueprintFeatureReference playerFeature in PlayerFeatures)
		{
			if (!playerCharacter.Facts.Contains(playerFeature.Get()))
			{
				playerCharacter.AddFact(playerFeature.Get())?.TryAddSource(this);
			}
		}
		Element.LogInfo("Receive premium reward: {0}", DlcReward);
		player.ClaimedDlcRewards.Add(DlcReward);
		return true;
	}

	public override string GetDescription()
	{
		return "Выдает реварды за DLC.";
	}

	public override string GetCaption()
	{
		return $"Add reward: {DlcReward}";
	}

	protected override void RunAction()
	{
		ItemsCollection sharedStash = Game.Instance.Player.SharedStash;
		if (MaybeAdd(sharedStash))
		{
			AdditionalActions.Run();
		}
	}
}
