using System.Collections.Generic;
using Kingmaker.Achievements;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f00ea8f728694b8f8b2e195d44de196d")]
public class AddAchievementReward : GameAction
{
	[SerializeField]
	private AchievementDataReference m_AchievementDataReference;

	[SerializeField]
	private List<BlueprintItemReference> m_Items;

	[SerializeField]
	private List<BlueprintFeatureReference> m_PlayerFeatures;

	[SerializeField]
	private ActionList m_AdditionalActions;

	private AchievementData AchievementData => m_AchievementDataReference;

	public override string GetCaption()
	{
		return $"Add reward: {AchievementData}";
	}

	public override string GetDescription()
	{
		return "Get rewards for achievement.";
	}

	protected override void RunAction()
	{
		ItemsCollection sharedStash = Game.Instance.Player.SharedStash;
		if (TryAddRewards(sharedStash))
		{
			m_AdditionalActions?.Run();
		}
	}

	private bool TryAddRewards(ItemsCollection inventory = null)
	{
		Player player = Game.Instance.Player;
		if (player == null || AchievementData == null)
		{
			return false;
		}
		if (player.AchievementRewardIsClaimed(AchievementData))
		{
			Element.LogInfo("Achievement reward already received: {0}", AchievementData);
			return false;
		}
		if (!player.Achievements.IsAchievementUnlocked(AchievementData))
		{
			Element.LogInfo("Achievement reward is not unlocked: {0}", AchievementData);
			return false;
		}
		AddItems(inventory);
		AddFeatures();
		Element.LogInfo("Receive achievement reward: {0}", AchievementData);
		player.ClaimAchievementReward(AchievementData);
		return true;
	}

	private void AddItems(ItemsCollection inventory)
	{
		List<BlueprintItemReference> items = m_Items;
		if (items == null || items.Count <= 0)
		{
			return;
		}
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		if (inventory == null)
		{
			inventory = playerCharacter.Inventory.Collection;
		}
		foreach (BlueprintItemReference item in m_Items)
		{
			if (!inventory.Contains(item.Get()) && !playerCharacter.Inventory.Contains(item.Get()))
			{
				inventory.Add(item.Get());
			}
		}
	}

	private void AddFeatures()
	{
		List<BlueprintFeatureReference> playerFeatures = m_PlayerFeatures;
		if (playerFeatures == null || playerFeatures.Count <= 0)
		{
			return;
		}
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		foreach (BlueprintFeatureReference playerFeature in m_PlayerFeatures)
		{
			if (!playerCharacter.Facts.Contains(playerFeature.Get()))
			{
				playerCharacter.AddFact(playerFeature.Get())?.TryAddSource(this);
			}
		}
	}
}
