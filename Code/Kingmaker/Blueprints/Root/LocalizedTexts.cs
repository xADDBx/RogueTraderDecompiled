using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Localization;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class LocalizedTexts
{
	[ValidateNotNull]
	public GlossaryStrings[] Glossaries;

	[ValidateNotNull]
	public StatsStrings Stats;

	[ValidateNotNull]
	public ItemsFilterStrings ItemsFilter;

	[ValidateNotNull]
	public SizeStrings Sizes;

	[ValidateNotNull]
	public GameLogStrings GameLog;

	[ValidateNotNull]
	public UIStrings UserInterfacesText;

	[ValidateNotNull]
	public AbilityTypeString AbilityTypes;

	[ValidateNotNull]
	public WeaponRangeTypeString WeaponRangeTypes;

	[ValidateNotNull]
	public WarningNotificationString WarningNotification;

	[ValidateNotNull]
	public ItemTypeNameString UnidentifiedItemNames;

	[ValidateNotNull]
	public UsableItemTypeString UsableItemTypeNames;

	[ValidateNotNull]
	public AbilityTargetStrings AbilityTargets;

	[ValidateNotNull]
	public AbilityRangeStrings AbilityTargetRanges;

	[ValidateNotNull]
	public DescriptorTypeStrings Descriptors;

	[ValidateNotNull]
	public ItemsStrings Items;

	[ValidateNotNull]
	public ReasonStrings Reasons;

	[ValidateNotNull]
	public WeaponCategoryString WeaponCategories;

	[ValidateNotNull]
	public WeaponSubCategoryString WeaponSubCategories;

	[ValidateNotNull]
	public WeaponFamilyStrings WeaponFamilies;

	[ValidateNotNull]
	public DamageTypeStrings DamageTypes;

	[ValidateNotNull]
	public AttackTypeString AttackTypes;

	[ValidateNotNull]
	public InspectPartString InspectParts;

	[ValidateNotNull]
	public SpellDescriptorString SpellDescriptorNames;

	[ValidateNotNull]
	public ScatterRaysStrings ScatterRays;

	[FormerlySerializedAs("SpellDescriptorConditions")]
	public ConditionsString UnitConditions;

	[ValidateNotNull]
	public CalculatedPrerequisiteStrings CalculatedPrerequisites;

	public LocalizedString DefaultTrapName;

	public LocalizedString LockedContainer;

	public LocalizedString UnlockedContainer;

	public LocalizedString LockedwithKey;

	public LocalizedString UnlockedWithKey;

	public LocalizedString TrapCanNotBeDisarmedDirectly;

	public LocalizedString NeedSupplyPrefix;

	public LocalizedString AccessDenied;

	public LocalizedString AccessReceived;

	[InfoBox("Interact only with {text}")]
	public LocalizedString InteractOnlyWithTool;

	public static LocalizedTexts Instance => Game.Instance.BlueprintRoot.LocalizedTexts;
}
