using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionLootSettings : InteractionSettings
{
	[Serializable]
	public class TriggerData
	{
		public bool TriggerOnce = true;

		public bool OnlyTriggerWhenEmpty;

		public bool TriggerOnSpecificItem;

		[ShowIf("TriggerOnSpecificItem")]
		[SerializeField]
		private BlueprintItemReference m_SpecificItem;

		public ActionsReference Action;

		public BlueprintItem SpecificItem => m_SpecificItem?.Get();
	}

	[Serializable]
	public class ConditionableLootTable
	{
		public ConditionsReference Conditions = new ConditionsReference();

		public List<BlueprintLootReference> LootTables = new List<BlueprintLootReference>();
	}

	[Header("Loot settings")]
	[SerializeField]
	public LootContainerType LootContainerType;

	[SerializeField]
	private BlueprintLootReference[] m_LootTables = new BlueprintLootReference[0];

	[SerializeField]
	private ConditionableLootTable[] m_ConditionableLootTables = new ConditionableLootTable[0];

	public bool AddMapMarker = true;

	public bool ShowOnMapWhenEmpty;

	[SerializeField]
	public bool DestroyWhenEmpty;

	[FormerlySerializedAs("MapMarkerDesc")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.MapMarker)]
	public SharedStringAsset MapMarkerName;

	[CanBeNull]
	public SharedStringAsset Description;

	[FormerlySerializedAs("ItemRestriction")]
	[CanBeNull]
	[InfoBox("Evaluators: ItemFromContextEvaluator, InteractedMapObject")]
	public ConditionsReference LootConditions;

	[FormerlySerializedAs("ItemTakenTrigger")]
	public TriggerData TakeItemTrigger;

	public TriggerData PutItemTrigger;

	[FormerlySerializedAs("OnClosedTrigger")]
	public TriggerData CloseTrigger;

	[SerializeField]
	private BlueprintSharedVendorTableReference m_AttachedVendorTable;

	public override bool ShouldShowUseAnimationState => false;

	public override bool ShouldShowDialog => false;

	public override bool ShouldShowUnlimitedInteractionsPerRound => false;

	public override bool ShouldShowOverrideActionPointsCost => false;

	public BlueprintSharedVendorTable AttachedVendorTable => m_AttachedVendorTable?.Get();

	public ReferenceArrayProxy<BlueprintLoot> LootTables
	{
		get
		{
			List<BlueprintLootReference> list = m_LootTables.ToList();
			IEnumerable<BlueprintLootReference> collection = m_ConditionableLootTables.Where((ConditionableLootTable c) => c.Conditions.Get().Check()).SelectMany((ConditionableLootTable c) => c.LootTables);
			list.AddRange(collection);
			BlueprintReference<BlueprintLoot>[] array = list.ToArray();
			return array;
		}
	}

	public bool OneSlotMode => LootContainerType == LootContainerType.OneSlot;
}
