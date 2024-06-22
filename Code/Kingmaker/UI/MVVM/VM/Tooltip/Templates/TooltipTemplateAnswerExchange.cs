using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Items;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateAnswerExchange : TooltipBaseTemplate
{
	private static class OnSelectConversionUtils
	{
		public static ITooltipBrick GetGainPFBrick(BlueprintAnswer answer, out bool isPositive)
		{
			List<GainPF> list = new List<GainPF>();
			try
			{
				AssembleActionsOfType(answer.OnSelect.Actions, list);
				float num = list.Sum((GainPF action) => action.Value);
				isPositive = num >= 0f;
				RewardProfitFactorUI rewardProfitFactorUI = new RewardProfitFactorUI(null);
				TooltipBrickIconStatValueType type = (isPositive ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
				object result;
				if (num == 0f)
				{
					result = null;
				}
				else
				{
					string name = rewardProfitFactorUI.Name;
					string value = num.ToString();
					Sprite icon = rewardProfitFactorUI.Icon;
					Color? iconColor = rewardProfitFactorUI.IconColor;
					TooltipBaseTemplate tooltip = new TooltipTemplateSimple(UIStrings.Instance.ProfitFactorTexts.Title.Text, UIStrings.Instance.ProfitFactorTexts.Description.Text);
					result = new TooltipBrickIconStatValue(name, value, null, icon, type, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, iconColor);
				}
				return (ITooltipBrick)result;
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect GainPF bricks from OnSelect for {answer.name} \n{arg}");
				isPositive = false;
				return null;
			}
		}

		public static List<ITooltipBrick> GetGainItemsBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<AddItemToPlayer> list2 = new List<AddItemToPlayer>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (AddItemToPlayer item in list2)
				{
					BlueprintItem itemToGive = item.ItemToGive;
					using (ContextData<DisableStatefulRandomContext>.Request())
					{
						TooltipTemplateItem tooltip = new TooltipTemplateItem(itemToGive.CreateEntity());
						list.Add(new TooltipBrickIconStatValue(itemToGive.Name, null, null, itemToGive.Icon, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, Color.white, null, null, hasValue: false));
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetLooseItemsBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<RemoveItemFromPlayer> list2 = new List<RemoveItemFromPlayer>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (RemoveItemFromPlayer item in list2)
				{
					BlueprintItem itemToRemove = item.ItemToRemove;
					using (ContextData<DisableStatefulRandomContext>.Request())
					{
						TooltipTemplateItem tooltip = new TooltipTemplateItem(itemToRemove.CreateEntity());
						list.Add(new TooltipBrickIconStatValue(itemToRemove.Name, null, null, itemToRemove.Icon, TooltipBrickIconStatValueType.Negative, TooltipBrickIconStatValueType.Negative, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, Color.white, null, null, hasValue: false));
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect RemoveItemFromPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetGainFactionReputationBricks(BlueprintAnswer answer, bool isPositive)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<GainFactionReputation> list2 = new List<GainFactionReputation>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (GainFactionReputation item in list2)
				{
					if ((item.Reputation > 0 && isPositive) || (item.Reputation < 0 && !isPositive))
					{
						TooltipBrickIconStatValueType type = (isPositive ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
						string factionLabel = UIStrings.Instance.CharacterSheet.GetFactionLabel(item.Faction);
						Sprite factionIcon = UIConfig.Instance.UIIcons.GetFactionIcon(item.Faction);
						TooltipTemplateSimple tooltipTemplateSimple = new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.GetFactionLabel(item.Faction), UIStrings.Instance.CharacterSheet.GetFactionDescription(item.Faction));
						string value = item.Reputation.ToString();
						Color? iconColor = null;
						TooltipBaseTemplate tooltip = tooltipTemplateSimple;
						list.Add(new TooltipBrickIconStatValue(factionLabel, value, null, factionIcon, type, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, iconColor));
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetGainColonyResourcesBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<GainColonyResources> list2 = new List<GainColonyResources>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (GainColonyResources item in list2)
				{
					ResourceData[] resources = item.Resources;
					foreach (ResourceData resourceData in resources)
					{
						BlueprintResource blueprintResource = resourceData?.Resource?.Get();
						if (blueprintResource != null)
						{
							TooltipTemplateSimple tooltip = new TooltipTemplateSimple(blueprintResource.Name, blueprintResource.Description);
							list.Add(new TooltipBrickIconStatValue(blueprintResource.Name, resourceData.Count.ToString(), null, blueprintResource.Icon, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip));
						}
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		public static List<ITooltipBrick> GetLooseColonyResourcesBricks(BlueprintAnswer answer)
		{
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			try
			{
				List<RemoveColonyResources> list2 = new List<RemoveColonyResources>();
				AssembleActionsOfType(answer.OnSelect.Actions, list2);
				foreach (RemoveColonyResources item in list2)
				{
					ResourceData[] resources = item.Resources;
					foreach (ResourceData resourceData in resources)
					{
						BlueprintResource blueprintResource = resourceData?.Resource?.Get();
						if (blueprintResource != null)
						{
							TooltipTemplateSimple tooltip = new TooltipTemplateSimple(blueprintResource.Name, blueprintResource.Description);
							list.Add(new TooltipBrickIconStatValue(blueprintResource.Name, resourceData.Count.ToString(), null, blueprintResource.Icon, TooltipBrickIconStatValueType.Negative, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip));
						}
					}
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot collect AddItemToPlayer bricks from OnSelect for {answer.name} \n{arg}");
			}
			return list;
		}

		private static void AssembleActionsOfType<T>(GameAction[] actions, List<T> result) where T : GameAction
		{
			if (result == null)
			{
				result = new List<T>();
			}
			result.AddRange(actions.OfType<T>().ToList());
			foreach (Conditional item in actions.OfType<Conditional>())
			{
				AssembleActionsOfType((item.ConditionsChecker.Check() ? item.IfTrue : item.IfFalse).Actions, result);
			}
		}
	}

	private readonly BlueprintAnswer m_BlueprintAnswer;

	public TooltipTemplateAnswerExchange(BlueprintAnswer blueprintAnswer)
	{
		m_BlueprintAnswer = blueprintAnswer;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		GetRewardsFromComponents(out var gainBricks, out var looseBricks);
		GetRewardsOnSelect(out var gainBricks2, out var looseBricks2);
		gainBricks.AddRange(gainBricks2);
		looseBricks.AddRange(looseBricks2);
		if (gainBricks.Any())
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.YouWillGainTitle.Text, TooltipTitleType.H1, TextAlignmentOptions.Left));
			list.AddRange(gainBricks);
		}
		if (looseBricks.Any())
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.YouWillLoseTitle.Text, TooltipTitleType.H1, TextAlignmentOptions.Left));
			list.AddRange(looseBricks);
		}
		return list;
	}

	private void GetRewardsFromComponents(out List<ITooltipBrick> gainBricks, out List<ITooltipBrick> looseBricks)
	{
		IEnumerable<Reward> rewards = m_BlueprintAnswer.GetRewards();
		gainBricks = new List<ITooltipBrick>();
		looseBricks = new List<ITooltipBrick>();
		foreach (Reward item in rewards)
		{
			RewardUI reward = RewardUIFactory.GetReward(item);
			List<ITooltipBrick> result = ((reward.Count >= 0) ? gainBricks : looseBricks);
			AddRewardBrick(result, reward);
		}
	}

	private void GetRewardsOnSelect(out List<ITooltipBrick> gainBricks, out List<ITooltipBrick> looseBricks)
	{
		gainBricks = new List<ITooltipBrick>();
		looseBricks = new List<ITooltipBrick>();
		bool isPositive;
		ITooltipBrick gainPFBrick = OnSelectConversionUtils.GetGainPFBrick(m_BlueprintAnswer, out isPositive);
		if (gainPFBrick != null)
		{
			(isPositive ? gainBricks : looseBricks).Add(gainPFBrick);
		}
		gainBricks.AddRange(OnSelectConversionUtils.GetGainItemsBricks(m_BlueprintAnswer));
		looseBricks.AddRange(OnSelectConversionUtils.GetLooseItemsBricks(m_BlueprintAnswer));
		gainBricks.AddRange(OnSelectConversionUtils.GetGainFactionReputationBricks(m_BlueprintAnswer, isPositive: true));
		looseBricks.AddRange(OnSelectConversionUtils.GetGainFactionReputationBricks(m_BlueprintAnswer, isPositive: false));
		gainBricks.AddRange(OnSelectConversionUtils.GetGainColonyResourcesBricks(m_BlueprintAnswer));
		looseBricks.AddRange(OnSelectConversionUtils.GetLooseColonyResourcesBricks(m_BlueprintAnswer));
	}

	private void AddRewardBrick(List<ITooltipBrick> result, RewardUI reward)
	{
		if (reward is RewardItemUI rewardItemUI)
		{
			if (!rewardItemUI.IsMiner)
			{
				AddRewardItem(result, rewardItemUI);
				return;
			}
		}
		else
		{
			if (reward is RewardAddFeatureUI feature)
			{
				AddRewardFeature(result, feature);
				return;
			}
			if (reward is RewardChangeStatContentmentUI rewardChangeStatUI)
			{
				AddRewardChangeStat(result, rewardChangeStatUI);
				return;
			}
			if (reward is RewardChangeStatEfficiencyUI rewardChangeStatUI2)
			{
				AddRewardChangeStat(result, rewardChangeStatUI2);
				return;
			}
			if (reward is RewardChangeStatSecurityUI rewardChangeStatUI3)
			{
				AddRewardChangeStat(result, rewardChangeStatUI3);
				return;
			}
		}
		TooltipBrickIconStatValueType type = ((reward.Count >= 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		string name = reward.Name;
		string countText = reward.CountText;
		Sprite icon = reward.Icon;
		Color? iconColor = reward.IconColor;
		TooltipBaseTemplate tooltip = reward.GetTooltip();
		result.Add(new TooltipBrickIconStatValue(name, countText, null, icon, type, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, iconColor));
	}

	private void AddRewardItem(List<ITooltipBrick> result, RewardItemUI item)
	{
		for (int i = 0; i < item.Count; i++)
		{
			TooltipBrickIconStatValueType tooltipBrickIconStatValueType = ((item.Count > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			string name = item.Name;
			Sprite icon = item.Icon;
			TooltipBaseTemplate tooltip = item.GetTooltip();
			Color? iconColor = Color.white;
			result.Add(new TooltipBrickIconStatValue(name, null, null, icon, tooltipBrickIconStatValueType, tooltipBrickIconStatValueType, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, iconColor, null, null, hasValue: false));
		}
	}

	private void AddRewardFeature(List<ITooltipBrick> result, RewardAddFeatureUI feature)
	{
		TooltipBrickIconStatValueType tooltipBrickIconStatValueType = ((feature.Count > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		string name = feature.Name;
		Sprite icon = feature.Icon;
		TooltipBaseTemplate tooltip = feature.GetTooltip();
		Color? iconColor = Color.white;
		result.Add(new TooltipBrickIconStatValue(name, null, null, icon, tooltipBrickIconStatValueType, tooltipBrickIconStatValueType, TooltipBrickIconStatValueStyle.Normal, null, tooltip, null, null, iconColor, null, null, hasValue: false));
	}

	private void AddRewardChangeStat(List<ITooltipBrick> result, RewardUI rewardChangeStatUI)
	{
		result.Add(new TooltipBrickIconStatValue(rewardChangeStatUI.Description, rewardChangeStatUI.CountText, null, rewardChangeStatUI.Icon, iconColor: rewardChangeStatUI.IconColor, tooltip: rewardChangeStatUI.GetTooltip(), type: (rewardChangeStatUI.Count > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative));
	}
}
