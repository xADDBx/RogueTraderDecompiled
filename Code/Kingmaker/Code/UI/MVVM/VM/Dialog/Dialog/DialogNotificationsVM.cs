using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;

public class DialogNotificationsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IPartyGainExperienceHandler, ISubscriber, IItemsCollectionHandler, IDialogNotificationHandler, IDialogCueHandler, IBookPageHandler, ICargoStateChangedHandler, IDamageHandler, INavigatorResourceCountChangedHandler, ISoulMarkShiftHandler, IProfitFactorHandler, IGainFactionReputationHandler, IGainFactionVendorDiscountHandler, IEntityGainFactHandler, ISubscriber<IMechanicEntity>
{
	public readonly List<string> RevealedLocationNames = new List<string>();

	public readonly Dictionary<string, int> ItemsChanged = new Dictionary<string, int>();

	public readonly List<int> XpGains = new List<int>();

	public readonly List<string> CargoAdded = new List<string>();

	public readonly List<string> CargoLost = new List<string>();

	public readonly List<EntityFact> AbilityAdded = new List<EntityFact>();

	public readonly List<EntityFact> BuffAdded = new List<EntityFact>();

	public readonly Dictionary<string, int> DamageDealt = new Dictionary<string, int>();

	public readonly List<int> NavigatorResourceAdded = new List<int>();

	public readonly List<float> ProfitFactorChanged = new List<float>();

	public readonly Dictionary<FactionType, int> FactionReputationChanged = new Dictionary<FactionType, int>();

	public readonly Dictionary<FactionType, int> FactionVendorDiscountChanged = new Dictionary<FactionType, int>();

	public readonly List<string> CustomNotifications = new List<string>();

	public readonly Dictionary<SoulMarkDirection, int> SoulMarkShifts = new Dictionary<SoulMarkDirection, int>();

	public CueShowData CueData;

	public readonly ReactiveCommand<bool> OnUpdateCommand = new ReactiveCommand<bool>();

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public DialogNotificationsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void OnUpdate(CueShowData cueData = null)
	{
		CueData = cueData;
		bool parameter = RevealedLocationNames.Count > 0 || ItemsChanged.Count > 0 || XpGains.Count > 0 || CustomNotifications.Count > 0 || CargoAdded.Count > 0 || CargoLost.Count > 0 || DamageDealt.Count > 0 || NavigatorResourceAdded.Count > 0 || SoulMarkShifts.Count > 0 || ProfitFactorChanged.Count > 0 || FactionReputationChanged.Count > 0 || FactionVendorDiscountChanged.Count > 0 || AbilityAdded.Count > 0 || BuffAdded.Count > 0;
		OnUpdateCommand.Execute(parameter);
		Clear();
	}

	private void OnItemsReceived(string itemName, int count)
	{
		if (!ContextData<GameLogDisabled>.Current)
		{
			if (ItemsChanged.TryGetValue(itemName, out var _))
			{
				ItemsChanged[itemName] += count;
			}
			else
			{
				ItemsChanged.Add(itemName, count);
			}
		}
	}

	private void Clear()
	{
		RevealedLocationNames.Clear();
		XpGains.Clear();
		ItemsChanged.Clear();
		CustomNotifications.Clear();
		CargoAdded.Clear();
		CargoLost.Clear();
		DamageDealt.Clear();
		NavigatorResourceAdded.Clear();
		SoulMarkShifts.Clear();
		ProfitFactorChanged.Clear();
		FactionReputationChanged.Clear();
		FactionVendorDiscountChanged.Clear();
		AbilityAdded.Clear();
		BuffAdded.Clear();
		CueData = null;
	}

	public string LinkGenerate(string label, string link)
	{
		TutorialColors tutorialColors = Game.Instance.BlueprintRoot.UIConfig.TutorialColors;
		string text = "#" + ColorUtility.ToHtmlStringRGB(tutorialColors.UILinkColor);
		return "<b><color=" + text + "><link=\"" + link + "\">" + label + "</link></color></b>";
	}

	public void HandlePartyGainExperience(int gained, bool isExperienceForDeath)
	{
		if (SettingsRoot.Game.Dialogs.ShowXPGainedNotification.GetValue())
		{
			XpGains.Add(gained);
		}
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (SettingsRoot.Game.Dialogs.ShowItemsReceivedNotification.GetValue() && collection == GameHelper.GetPlayerCharacter().Inventory.Collection && item?.Blueprint != null && !string.IsNullOrWhiteSpace(item.Name) && count != 0)
		{
			OnItemsReceived(LinkGenerate(item.Name, "i:" + item.UniqueId), count);
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (SettingsRoot.Game.Dialogs.ShowItemsReceivedNotification.GetValue() && collection.IsPlayerInventory && item.IsLootable && item?.Blueprint != null && !string.IsNullOrWhiteSpace(item.Name) && count != 0)
		{
			OnItemsReceived(LinkGenerate(item.Name, "ib:" + item.Blueprint.name), -count);
		}
	}

	public void HandleCreateNewCargo(CargoEntity entity)
	{
		ItemsItemOrigin originType = entity.Blueprint.OriginType;
		string item = LinkGenerate(UIStrings.Instance.CargoTexts.GetLabelByOrigin(originType).IsNullOrEmpty() ? entity.Blueprint.Name : UIStrings.Instance.CargoTexts.GetLabelByOrigin(originType), "cb:" + entity.Blueprint.name);
		CargoAdded.Add(item);
	}

	public void HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
	{
		ItemsItemOrigin originType = entity.Blueprint.OriginType;
		string item = LinkGenerate(UIStrings.Instance.CargoTexts.GetLabelByOrigin(originType).IsNullOrEmpty() ? entity.Blueprint.Name : UIStrings.Instance.CargoTexts.GetLabelByOrigin(originType), "cb:" + entity.Blueprint.name);
		CargoLost.Add(item);
	}

	public void HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
	{
	}

	public void HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from)
	{
	}

	public void HandleChaneNavigatorResourceCount(int count)
	{
		NavigatorResourceAdded.Add(count);
	}

	public void AddCustomNotification(string text)
	{
		CustomNotifications.Add(text);
	}

	public void HandleOnCueShow(CueShowData cueShowData)
	{
		OnUpdate(cueShowData);
	}

	public void HandleOnBookPageShow(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		OnUpdate();
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (DamageDealt.TryGetValue(dealDamage.ConcreteTarget.Name ?? string.Empty, out var _))
		{
			DamageDealt[dealDamage.ConcreteTarget.Name ?? string.Empty] += dealDamage.Result;
		}
		else
		{
			DamageDealt.Add(dealDamage.ConcreteTarget.Name ?? string.Empty, dealDamage.Result);
		}
	}

	public void HandleSoulMarkShift(ISoulMarkShiftProvider provider)
	{
		if (!provider.SoulMarkShift.Empty)
		{
			SoulMarkDirection direction = provider.SoulMarkShift.Direction;
			int value = provider.SoulMarkShift.Value;
			if (SoulMarkShifts.TryGetValue(provider.SoulMarkShift.Direction, out var _))
			{
				SoulMarkShifts[direction] += value;
			}
			else
			{
				SoulMarkShifts.Add(direction, value);
			}
		}
	}

	public void HandleProfitFactorModifierAdded(float max, ProfitFactorModifier modifier)
	{
		ProfitFactorChanged.Add(max);
	}

	public void HandleProfitFactorModifierRemoved(float max, ProfitFactorModifier modifier)
	{
		ProfitFactorChanged.Add(max);
	}

	public void HandleGainFactionReputation(FactionType factionType, int count)
	{
		OnFactionReputationReceived(factionType, count);
	}

	private void OnFactionReputationReceived(FactionType factionType, int count)
	{
		if (FactionReputationChanged.TryGetValue(factionType, out var _))
		{
			FactionReputationChanged[factionType] += count;
		}
		else
		{
			FactionReputationChanged.Add(factionType, count);
		}
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		IEntity owner = fact.Owner;
		if ((!(owner is BaseUnitEntity baseUnitEntity) || (!baseUnitEntity.IsInPlayerParty && !(owner is StarshipEntity))) ? true : false)
		{
			return;
		}
		owner = fact.Owner;
		string text = ((owner is StarshipEntity starshipEntity) ? starshipEntity.CharacterName : ((!(owner is BaseUnitEntity baseUnitEntity2)) ? string.Empty : baseUnitEntity2.CharacterName));
		string value = text;
		if ((fact is Buff && fact.Blueprint is BlueprintBuff blueprintBuff && (blueprintBuff.IsHiddenInUI || !blueprintBuff.ShowInDialogue)) || (fact is Ability && fact.Blueprint is BlueprintAbility { ShowInDialogue: false }) || (fact is Feature && fact.Blueprint is BlueprintFeature { ShowInDialogue: false }) || string.IsNullOrWhiteSpace(fact.Name) || string.IsNullOrWhiteSpace(value))
		{
			return;
		}
		if (!(fact is Buff))
		{
			if (!(fact is Ability))
			{
				if (fact is Feature)
				{
					AbilityAdded.Add(fact);
				}
			}
			else
			{
				AbilityAdded.Add(fact);
			}
		}
		else
		{
			BuffAdded.Add(fact);
		}
	}

	public void HandleGainFactionVendorDiscount(FactionType factionType, int discount)
	{
		OnFactionVendorDiscountReceived(factionType, discount);
	}

	private void OnFactionVendorDiscountReceived(FactionType factionType, int discount)
	{
		if (FactionVendorDiscountChanged.TryGetValue(factionType, out var _))
		{
			FactionVendorDiscountChanged[factionType] += discount;
		}
		else
		{
			FactionVendorDiscountChanged.Add(factionType, discount);
		}
	}
}
