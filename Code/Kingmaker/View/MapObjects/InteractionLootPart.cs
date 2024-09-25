using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.Interaction;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionLootPart : InteractionPart<InteractionLootSettings>, IItemsCollectionHandler, ISubscriber, ILootable, IHasInteractionVariantActors, IHashable
{
	private bool m_DestroyWhenEmpty;

	[CanBeNull]
	[JsonProperty(PropertyName = "Unit")]
	private BlueprintUnit m_Unit;

	[JsonProperty]
	private ItemsCollection m_Loot;

	[JsonProperty]
	private Dictionary<BlueprintItem, int> m_KnownItems;

	[JsonProperty(PropertyName = "TriggeredTake")]
	private bool m_TriggeredTake;

	[JsonProperty(PropertyName = "TriggeredPut")]
	private bool m_TriggeredPut;

	[JsonProperty(PropertyName = "TriggeredClose")]
	private bool m_TriggeredClose;

	private bool m_InteractThroughVariants;

	[JsonProperty]
	public bool IsViewed { get; set; }

	public ItemsCollection Loot
	{
		get
		{
			return m_Loot;
		}
		set
		{
			m_Loot = value;
			base.Enabled = m_Loot != null && (m_Loot.HasLoot || !DisableWhenEmpty);
		}
	}

	public bool DisableWhenEmpty
	{
		get
		{
			if (base.Settings.LootContainerType != 0 && base.Settings.LootContainerType != LootContainerType.Environment)
			{
				return base.Settings.LootContainerType == LootContainerType.Unit;
			}
			return true;
		}
	}

	public bool DestroyWhenEmpty
	{
		get
		{
			if (!m_DestroyWhenEmpty)
			{
				return base.Settings.DestroyWhenEmpty;
			}
			return true;
		}
	}

	public bool LootViewed => IsViewed;

	public IEnumerable<LootEntry> MapObjectLoot => base.Settings.LootTables.Where((BlueprintLoot t) => t).SelectMany((BlueprintLoot t) => t.Items);

	public string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(GetName()))
			{
				return GetName();
			}
			return UIStrings.Instance.LootWindow.GetLootName(base.Settings.LootContainerType);
		}
	}

	public string Description => GetDescription();

	public BaseUnitEntity OwnerEntity => null;

	public ItemsCollection Items => m_Loot;

	public List<BlueprintCargoReference> Cargo => null;

	Func<ItemEntity, bool> ILootable.CanInsertItem => CanInsertItem;

	public override bool InteractThroughVariants
	{
		get
		{
			if (m_InteractThroughVariants)
			{
				return !AlreadyUnlocked;
			}
			return false;
		}
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		m_Loot.PrePostLoad();
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		if (m_Loot == null)
		{
			m_Loot = new ItemsCollection(base.Owner);
		}
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		if (DisableWhenEmpty && base.Enabled && !m_Loot.HasLoot)
		{
			base.Enabled = false;
		}
	}

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	private void UpdateMarkerName()
	{
		if (base.Settings.AddMapMarker)
		{
			LocalMapMarkerPart orCreate = base.Owner.GetOrCreate<LocalMapMarkerPart>();
			orCreate.IsRuntimeCreated = true;
			orCreate.Settings.Type = LocalMapMarkType.Loot;
			orCreate.Settings.Description = base.Settings.MapMarkerName;
			orCreate.Settings.DescriptionUnit = m_Unit;
			orCreate.SetHidden(!base.Enabled || (!base.Settings.ShowOnMapWhenEmpty && Loot.Where((ItemEntity l) => l.IsLootable).Empty()));
		}
	}

	public override void SetSource(IAbstractEntityPartComponent source)
	{
		base.SetSource(source);
		if (m_KnownItems == null)
		{
			foreach (LootEntry item in MapObjectLoot)
			{
				if (item.Item == null)
				{
					continue;
				}
				Loot.Add(item.Item, item.Count, delegate(ItemEntity entity)
				{
					if (item.Identify)
					{
						entity.Identify();
					}
				});
			}
			m_KnownItems = CountItems(MapObjectLoot);
		}
		else
		{
			UpdateLoot(MapObjectLoot);
		}
		if (DisableWhenEmpty && Loot.Empty())
		{
			base.Enabled = false;
		}
		else
		{
			UpdateMarkerName();
		}
	}

	protected override void OnEnabledChanged()
	{
		UpdateMarkerName();
	}

	public override bool CanInteract()
	{
		if (base.Enabled)
		{
			return !Game.Instance.TurnController.TbActive;
		}
		return false;
	}

	private void TryToTrigger(InteractionLootSettings.TriggerData trigger, [CanBeNull] ItemEntity item, ref bool triggered)
	{
		if (trigger.Action.Get() == null || !trigger.Action.Get().Actions.HasActions || (trigger.OnlyTriggerWhenEmpty && Loot.Items.HasItem((ItemEntity i) => i.IsLootable)) || (trigger.TriggerOnSpecificItem && ((item != null && item.Blueprint != trigger.SpecificItem) || (item == null && !Loot.Contains(trigger.SpecificItem)))) || (trigger.TriggerOnce & triggered))
		{
			return;
		}
		triggered = true;
		using (ContextData<ItemEntity.ContextData>.RequestIf(item != null)?.Setup(item))
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				trigger.Action.Get().Run();
			}
		}
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection == Loot)
		{
			if (LoadingProcess.Instance.IsLoadingInProcess)
			{
				HandleItemsAddedImplementation(item);
			}
			else
			{
				Game.Instance.GameCommandQueue.TriggerLoot(this, TriggerLootGameCommand.TriggerType.Put, item);
			}
		}
	}

	public void HandleItemsAddedImplementation(ItemEntity item)
	{
		TryToTrigger(base.Settings.PutItemTrigger, item, ref m_TriggeredPut);
		base.Enabled = Loot.HasLoot;
		UpdateMarkerName();
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (Loot == collection)
		{
			Game.Instance.GameCommandQueue.TriggerLoot(this, TriggerLootGameCommand.TriggerType.Take, item);
		}
	}

	public void HandleItemsRemovedImplementation(ItemEntity item)
	{
		TryToTrigger(base.Settings.TakeItemTrigger, item, ref m_TriggeredTake);
		if ((bool)base.Settings.AttachedVendorTable)
		{
			Game.Instance.Player.SharedVendorTables.GetCollection(base.Settings.AttachedVendorTable).Remove(item.Blueprint);
		}
		if (Loot.Items.HasItem((ItemEntity i) => i.IsLootable))
		{
			return;
		}
		UpdateMarkerName();
		DroppedLoot.EntityData entityData = base.Owner as DroppedLoot.EntityData;
		if (DestroyWhenEmpty || base.Settings.LootContainerType == LootContainerType.Environment || (entityData != null && entityData.IsDroppedByPlayer))
		{
			if (entityData != null)
			{
				Game.Instance.EntityDestroyer.Destroy(base.Owner);
			}
			else
			{
				base.Owner.IsInGame = false;
			}
		}
		if (DisableWhenEmpty)
		{
			base.Enabled = false;
		}
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		EntityViewBase[] loots = ((base.Owner.View is DroppedLoot) ? MassLootHelper.GetObjectsWithLoot(base.View).ToArray() : new EntityViewBase[1] { base.View });
		EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<ILootInteractionHandler>)delegate(ILootInteractionHandler l)
		{
			l.HandleLootInteraction(loots, base.Settings.LootContainerType, OnLootClosed);
		}, isCheckRuntime: true);
		EntityViewBase[] array = loots;
		foreach (EntityViewBase entityViewBase in array)
		{
			if (entityViewBase.InteractionComponent is InteractionLootPart interactionLootPart)
			{
				interactionLootPart.IsViewed = true;
			}
			else if (entityViewBase.Data is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.MarkLootViewed();
			}
		}
	}

	private void OnLootClosed()
	{
		Game.Instance.GameCommandQueue.TriggerLoot(this, TriggerLootGameCommand.TriggerType.Close);
	}

	public void OnLootClosedImplementation()
	{
		TryToTrigger(base.Settings.CloseTrigger, null, ref m_TriggeredClose);
	}

	public void SetUnit(BlueprintUnit unit)
	{
		m_Unit = unit;
		UpdateMarkerName();
	}

	public void AddItems(IEnumerable<LootEntry> items)
	{
		List<LootEntry> list = items.ToList();
		foreach (LootEntry item in list)
		{
			if (item.Item != null)
			{
				Loot.Add(item.Item, item.Count);
			}
		}
		m_KnownItems = CountItems(list);
	}

	public bool CanInsertItem(ItemEntity item)
	{
		if (base.Settings.LootConditions == null)
		{
			return true;
		}
		using (ContextData<ItemEntity.ContextData>.Request().Setup(item))
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				return base.Settings.LootConditions.Get().Check();
			}
		}
	}

	internal void UpdateLoot(IEnumerable<LootEntry> items)
	{
		if (m_KnownItems == null)
		{
			return;
		}
		Dictionary<BlueprintItem, int> itemsCount = CountItems(items);
		foreach (KeyValuePair<BlueprintItem, int> item in itemsCount)
		{
			int oldCount = m_KnownItems.Get(item.Key, 0);
			UpdateLootItem(item.Key, oldCount, item.Value);
		}
		foreach (BlueprintItem item2 in m_KnownItems.Keys.Where((BlueprintItem i) => !itemsCount.ContainsKey(i)).ToList())
		{
			int oldCount2 = m_KnownItems.Get(item2, 0);
			UpdateLootItem(item2, oldCount2, 0);
		}
	}

	private void UpdateLootItem(BlueprintItem item, int oldCount, int newCount)
	{
		if (oldCount != newCount)
		{
			if (oldCount < newCount)
			{
				Loot.Add(item, newCount - oldCount);
			}
			else
			{
				Loot.Remove(item, oldCount - newCount);
			}
			m_KnownItems[item] = newCount;
		}
	}

	[NotNull]
	private static Dictionary<BlueprintItem, int> CountItems(IEnumerable<LootEntry> items)
	{
		Dictionary<BlueprintItem, int> dictionary = new Dictionary<BlueprintItem, int>();
		foreach (LootEntry item in items)
		{
			if (item.Item != null)
			{
				int num = dictionary.Get(item.Item, 0);
				dictionary[item.Item] = num + item.Count;
			}
		}
		return dictionary;
	}

	protected override void OnSubscribe()
	{
		Loot?.Subscribe();
	}

	protected override void OnUnsubscribe()
	{
		ItemsCollection loot = Loot;
		if (loot != null && !loot.IsSharedStash)
		{
			Loot?.Unsubscribe();
		}
	}

	protected override void OnPreSave()
	{
		Loot?.PreSave();
	}

	protected override void OnPostLoad()
	{
		Loot?.PostLoad();
	}

	protected override void OnDetach()
	{
		Loot?.Dispose();
	}

	public string GetName()
	{
		if (base.Settings.AddMapMarker)
		{
			if (m_Unit != null)
			{
				return m_Unit.CharacterName;
			}
			if (base.Settings.MapMarkerName != null)
			{
				return base.Settings.MapMarkerName.String;
			}
		}
		return string.Empty;
	}

	public string GetDescription()
	{
		LocalizedString localizedString = ObjectExtensions.Or(base.Settings.Description, null)?.String;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}

	IEnumerable<IInteractionVariantActor> IHasInteractionVariantActors.GetInteractionVariantActors()
	{
		if (base.Type == InteractionType.Direct || !InteractThroughVariants)
		{
			return null;
		}
		IEnumerable<IInteractionVariantActor> all = base.View.Data.Parts.GetAll<IInteractionVariantActor>();
		if (all.Any((IInteractionVariantActor x) => x is KeyRestrictionPart && x.CanInteract(GameHelper.GetPlayerCharacter())))
		{
			return null;
		}
		all = all.Where((IInteractionVariantActor x) => !(x is KeyRestrictionPart));
		if (!all.Any())
		{
			return null;
		}
		return all;
	}

	protected override void ConfigureRestrictions()
	{
		TechUseRestriction component = base.View.GetComponent<TechUseRestriction>();
		if (component != null)
		{
			base.Settings.ShowOvertip = false;
			component.Settings.InteractOnlyWithToolIfFailed = true;
			base.View.Data.Parts.GetOrCreate<TechUseMultikeyItemRestrictionPart>().Settings.CopyDCData(component.Settings);
			base.View.Data.Parts.GetOrCreate<DemolitionMeltaChargeRestrictionPart>().Settings.CopyDCData(component.Settings);
			m_InteractThroughVariants = true;
		}
		LoreXenosRestriction component2 = base.View.GetComponent<LoreXenosRestriction>();
		if (component2 != null)
		{
			base.Settings.ShowOvertip = false;
			component2.Settings.InteractOnlyWithToolIfFailed = true;
			base.View.Data.Parts.GetOrCreate<LoreXenosMultikeyItemRestrictionPart>().Settings.CopyDCData(component2.Settings);
			m_InteractThroughVariants = true;
		}
	}

	protected override void OnDidInteract(BaseUnitEntity user)
	{
		foreach (InteractionRestrictionPart needItemRestriction in GetNeedItemRestrictions())
		{
			if (needItemRestriction != null)
			{
				needItemRestriction.IsDisabled = true;
			}
		}
		IEnumerable<InteractionRestrictionPart> GetNeedItemRestrictions()
		{
			yield return base.View.Data.Parts.GetOptional<TechUseMultikeyItemRestrictionPart>();
			yield return base.View.Data.Parts.GetOptional<DemolitionMeltaChargeRestrictionPart>();
			yield return base.View.Data.Parts.GetOptional<LoreXenosMultikeyItemRestrictionPart>();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_Unit);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<ItemsCollection>.GetHash128(m_Loot);
		result.Append(ref val3);
		Dictionary<BlueprintItem, int> knownItems = m_KnownItems;
		if (knownItems != null)
		{
			int val4 = 0;
			foreach (KeyValuePair<BlueprintItem, int> item in knownItems)
			{
				Hash128 hash = default(Hash128);
				Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val5);
				int obj = item.Value;
				Hash128 val6 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val6);
				val4 ^= hash.GetHashCode();
			}
			result.Append(ref val4);
		}
		result.Append(ref m_TriggeredTake);
		result.Append(ref m_TriggeredPut);
		result.Append(ref m_TriggeredClose);
		bool val7 = IsViewed;
		result.Append(ref val7);
		return result;
	}
}
