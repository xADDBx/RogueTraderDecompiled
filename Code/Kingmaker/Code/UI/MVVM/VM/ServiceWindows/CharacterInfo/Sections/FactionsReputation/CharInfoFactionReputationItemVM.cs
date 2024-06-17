using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class CharInfoFactionReputationItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IFullScreenUIHandler, ISubscriber
{
	public readonly FactionType FactionType;

	public readonly ReactiveProperty<float> CurrentReputation = new ReactiveProperty<float>();

	public readonly ReactiveProperty<int?> NextLevelReputation = new ReactiveProperty<int?>();

	public readonly ReactiveProperty<int> ReputationLevel = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> IsMaxLevel = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<VendorLogic> Vendor = new ReactiveProperty<VendorLogic>();

	public readonly ReactiveProperty<int> Delta = new ReactiveProperty<int>();

	public readonly ReactiveProperty<float> Difference = new ReactiveProperty<float>();

	public readonly string Label;

	public readonly string Description;

	public readonly TooltipBaseTemplate Tooltip;

	public readonly List<FactionVendorInformationVM> Vendors = new List<FactionVendorInformationVM>();

	public bool CanTrade;

	public CharInfoFactionReputationItemVM(FactionType factionType, bool canTrade = false, List<MechanicEntity> vendors = null)
	{
		AddDisposable(EventBus.Subscribe(this));
		FactionType = factionType;
		CanTrade = canTrade;
		CurrentReputation.Value = ReputationHelper.GetCurrentReputationPoints(FactionType);
		NextLevelReputation.Value = ReputationHelper.GetNextLevelReputationPoints(FactionType);
		ReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(FactionType);
		IsMaxLevel.Value = ReputationHelper.IsMaxReputation(FactionType);
		Label = UIStrings.Instance.CharacterSheet.GetFactionLabel(FactionType);
		Description = UIStrings.Instance.CharacterSheet.GetFactionDescription(FactionType);
		Tooltip = new TooltipTemplateSimple(Label, Description);
		AddVendorsInfo(vendors);
	}

	private void AddVendorsInfo(IEnumerable<MechanicEntity> vendors = null)
	{
		Vendors.Clear();
		List<DetectedVendorData> list = ((vendors != null) ? (from vendor in vendors
			where vendor?.GetOptional<PartVendor>()?.Faction?.FactionType == FactionType
			let location = vendor.GetOptional<PartLastDetectedLocation>()
			select new DetectedVendorData(vendor, location?.Area, location?.AreaPart, location?.Chapter ?? 0)).ToList() : Game.Instance.Player.VendorsData.DetectedVendors.Where((DetectedVendorData x) => x.Faction?.Get()?.FactionType == FactionType).ToList());
		if (CanTrade)
		{
			foreach (DetectedVendorData item in list)
			{
				Vendors.Add(new FactionVendorInformationVM(item.Area?.Get()?.AreaName, item.VendorName, item.Entity));
			}
			return;
		}
		foreach (DetectedVendorData item2 in list)
		{
			Vendors.Add(new FactionVendorInformationVM(item2.Area?.Get()?.AreaName, item2.VendorName));
		}
	}

	public string GetCurrentAndNextLevelProgress()
	{
		IsMaxLevel.Value = ReputationHelper.IsMaxReputation(FactionType);
		NextLevelReputation.Value = ReputationHelper.GetNextLevelReputationPoints(FactionType);
		if (!IsMaxLevel.Value)
		{
			return string.Format(CultureInfo.InvariantCulture, (Mathf.RoundToInt(CurrentReputation.Value) < 10) ? "{0:0}" : "{0:0,0}", Mathf.RoundToInt(CurrentReputation.Value)) + " / " + string.Format(CultureInfo.InvariantCulture, "{0:0,0}", NextLevelReputation.Value);
		}
		return UIStrings.Instance.CharacterSheet.MaxReputationLevel;
	}

	public int GetCurrentReputationPoints()
	{
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(FactionType);
		int reputationPointsByLevel = ReputationHelper.GetReputationPointsByLevel(FactionType, currentReputationLevel);
		int reputationPointsByLevel2 = ReputationHelper.GetReputationPointsByLevel(FactionType, currentReputationLevel + 1);
		Delta.Value = reputationPointsByLevel2 - reputationPointsByLevel;
		return Delta.Value;
	}

	public float GetNextLevelReputationPoints()
	{
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(FactionType);
		int currentReputationPoints = ReputationHelper.GetCurrentReputationPoints(FactionType);
		int reputationPointsByLevel = ReputationHelper.GetReputationPointsByLevel(FactionType, currentReputationLevel);
		Difference.Value = currentReputationPoints - reputationPointsByLevel;
		return Difference.Value;
	}

	public float GetProgressToNextLevel()
	{
		return ReputationHelper.GetProgressToNextLevel(FactionType);
	}

	public float GetProgressPercent()
	{
		int? nextLevelReputationPoints = ReputationHelper.GetNextLevelReputationPoints(FactionType);
		if (nextLevelReputationPoints.HasValue)
		{
			return CurrentReputation.Value / (float)nextLevelReputationPoints.Value;
		}
		return 0f;
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.Vendor && !state)
		{
			CurrentReputation.Value = ReputationHelper.GetCurrentReputationPoints(FactionType);
			NextLevelReputation.Value = ReputationHelper.GetNextLevelReputationPoints(FactionType);
			ReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(FactionType);
			IsMaxLevel.Value = ReputationHelper.IsMaxReputation(FactionType);
		}
	}
}
