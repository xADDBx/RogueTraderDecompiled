using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public static class SoulMarkTooltipExtensions
{
	public static string GetGlossaryKeyByDirection(SoulMarkDirection direction)
	{
		return direction switch
		{
			SoulMarkDirection.Faith => "SoulMarkFaith", 
			SoulMarkDirection.Corruption => "SoulMarkCorruption", 
			SoulMarkDirection.Hope => "SoulMarkHope", 
			_ => "", 
		};
	}

	public static BlueprintSoulMark GetSoulMarkWithTier(BlueprintSoulMark baseBlueprint, int tier)
	{
		List<BlueprintSoulMark> list = (from f in baseBlueprint.ComponentsArray.Select(delegate(BlueprintComponent c)
			{
				RankChangedTrigger obj = c as RankChangedTrigger;
				return (obj == null) ? null : obj.Facts.FirstOrDefault((BlueprintMechanicEntityFact f) => f is BlueprintSoulMark);
			})
			select f as BlueprintSoulMark).ToList();
		if (tier < 0 || tier > list.Count)
		{
			return null;
		}
		return list[tier - 1];
	}

	public static BlueprintFeature GetSoulMarkFeature(BlueprintSoulMark baseBlueprint, int tier)
	{
		RankChangedTrigger obj = baseBlueprint.ComponentsArray[tier - 1] as RankChangedTrigger;
		return ((obj != null) ? obj.Facts.FirstOrDefault((BlueprintMechanicEntityFact f) => !(f is BlueprintSoulMark) && f is BlueprintFeature) : null) as BlueprintFeature;
	}

	public static void GetSoulMarkInfo(BlueprintSoulMark baseBlueprint, BaseUnitEntity unit, out List<int> rankThresholds, out int maxValue, out int currentValue, out int currentTier)
	{
		rankThresholds = baseBlueprint.ComponentsArray.Select(delegate(BlueprintComponent c)
		{
			RankChangedTrigger obj4 = c as RankChangedTrigger;
			return (obj4 == null) ? (-1) : (obj4.RankValue.Value - 1);
		}).ToList();
		rankThresholds.RemoveAll((int r) => r < 0);
		rankThresholds.Sort((int r0, int r1) => (r0 >= r1) ? 1 : (-1));
		rankThresholds.Insert(0, 0);
		maxValue = Enumerable.LastOrDefault(rankThresholds);
		if (unit.IsMainCharacter)
		{
			EntityFact entityFact = Enumerable.FirstOrDefault(unit.Facts.List, (EntityFact f) => f is Feature && f.Blueprint == baseBlueprint);
			int rank = ((entityFact != null) ? (entityFact.GetRank() - 1) : 0);
			currentValue = rank;
			currentTier = rankThresholds.IndexOf(rankThresholds.LastOrDefault((int r) => rank >= r));
			return;
		}
		currentTier = 0;
		currentValue = 0;
		int num = -1;
		List<BlueprintSoulMark> list = baseBlueprint.ComponentsArray.SelectMany(delegate(BlueprintComponent c)
		{
			RankChangedTrigger obj3 = c as RankChangedTrigger;
			return (obj3 == null) ? null : obj3.Facts.Select((BlueprintMechanicEntityFact f) => f as BlueprintSoulMark);
		}).ToList();
		IEnumerable<BlueprintFact> unitFactsBlueprints = unit.Facts.List.Select((EntityFact f) => f.Blueprint);
		list.RemoveAll((BlueprintSoulMark f) => !unitFactsBlueprints.Contains(f));
		foreach (BlueprintSoulMark item in list)
		{
			for (int i = 0; i < baseBlueprint.ComponentsArray.Length; i++)
			{
				RankChangedTrigger obj = baseBlueprint.ComponentsArray[i] as RankChangedTrigger;
				if (obj != null && obj.Facts.Contains(item))
				{
					num = Mathf.Max(num, i);
				}
			}
		}
		if (num >= 0)
		{
			RankChangedTrigger obj2 = baseBlueprint.ComponentsArray[num] as RankChangedTrigger;
			currentValue = ((obj2 != null) ? (obj2.RankValue.Value - 1) : 0);
			currentTier = num + 1;
		}
	}

	public static IEnumerable<ITooltipBrick> GetSlider(List<int> rankThresholds, int currentValue, int maxValue)
	{
		Color32 progressbarBonus = UIConfig.Instance.TooltipColors.ProgressbarBonus;
		Color32 defaultColor = UIConfig.Instance.TooltipColors.ProgressbarNeutral;
		List<BrickSliderValueVM> sliderValues = new List<BrickSliderValueVM>
		{
			new BrickSliderValueVM(maxValue, currentValue, null, needColor: true, progressbarBonus, new Color32(0, 0, 0, 0))
		};
		LocalizedString endText = null;
		int num = rankThresholds.IndexOf(maxValue);
		if (num >= 0)
		{
			endText = UIUtility.GetSoulMarkRankText(num);
		}
		yield return new TooltipBricksGroupStart(hasBackground: false, new TooltipBricksGroupLayoutParams
		{
			LayoutType = TooltipBricksGroupLayoutType.Vertical,
			Padding = new RectOffset(0, 0, -15, -25)
		});
		yield return new TooltipBrickSlider(maxValue, currentValue, sliderValues, showValue: false, 40, defaultColor, endText);
		yield return new TooltipBricksGroupEnd();
		yield return new TooltipBrickText($"{UIStrings.Instance.Tooltips.CurrentValue.Text} {currentValue}", TooltipTextType.Centered, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
	}

	public static List<ITooltipBrick> GetFeatureBlock(BlueprintSoulMark baseBlueprint, int tier, bool highlight)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (tier < 1 || tier > baseBlueprint.ComponentsArray.Length)
		{
			return list;
		}
		if (!(baseBlueprint.ComponentsArray[tier - 1] is RankChangedTrigger rankChangedTrigger))
		{
			return list;
		}
		Color32 color = (highlight ? new Color32(130, 174, 115, byte.MaxValue) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		list.Add(new TooltipBricksGroupStart(hasBackground: true, null, color));
		list.Add(new TooltipBrickIconValueStat(UIUtility.GetSoulMarkRankText(tier), (rankChangedTrigger.RankValue.Value - 1).ToString(), null, TooltipIconValueStatType.Normal, isWhite: false, needChangeSize: false, 18, 18, needChangeColor: true, Color.black, Color.black));
		list.Add(new TooltipBricksGroupEnd());
		list.Add(new TooltipBrickText(GetSoulMarkWithTier(baseBlueprint, tier)?.Description));
		BlueprintFeature soulMarkFeature = GetSoulMarkFeature(baseBlueprint, tier);
		list.Add(new TooltipBricksGroupStart(hasBackground: true, null, new Color32(183, 170, 144, byte.MaxValue)));
		list.Add(new TooltipBrickFeature(soulMarkFeature));
		list.Add(new TooltipBricksGroupEnd());
		return list;
	}
}
