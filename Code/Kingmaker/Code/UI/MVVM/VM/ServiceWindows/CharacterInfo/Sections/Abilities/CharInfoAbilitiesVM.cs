using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.Blueprints;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoAbilitiesVM : CharInfoComponentVM, IActionBarPartAbilitiesHandler, ISubscriber
{
	public readonly AutoDisposingList<CharInfoFeatureGroupVM> ActiveAbilities = new AutoDisposingList<CharInfoFeatureGroupVM>();

	public readonly AutoDisposingList<CharInfoFeatureGroupVM> PassiveAbilities = new AutoDisposingList<CharInfoFeatureGroupVM>();

	public readonly AutoDisposingList<CharInfoFeatureGroupVM> Augmentations = new AutoDisposingList<CharInfoFeatureGroupVM>();

	public SurfaceActionBarPartAbilitiesVM ActionBarPartAbilitiesVM;

	public readonly BoolReactiveProperty ChooseAbilityMode = new BoolReactiveProperty();

	public int TargetSlotIndex = -1;

	public readonly BoolReactiveProperty IsNotControllableCharacter;

	public readonly ReactiveProperty<FeatureGroupingMode> GroupingMode = new ReactiveProperty<FeatureGroupingMode>(FeatureGroupingMode.ByType);

	private static readonly FeaturesFilter.FeatureFilterType[] TypeGroupOrder = new FeaturesFilter.FeatureFilterType[7]
	{
		FeaturesFilter.FeatureFilterType.ArchetypeFilter,
		FeaturesFilter.FeatureFilterType.OriginFilter,
		FeaturesFilter.FeatureFilterType.WarpFilter,
		FeaturesFilter.FeatureFilterType.OffenseFilter,
		FeaturesFilter.FeatureFilterType.DefenseFilter,
		FeaturesFilter.FeatureFilterType.SupportFilter,
		FeaturesFilter.FeatureFilterType.UniversalFilter
	};

	private static UITextCharSheet Strings => UIStrings.Instance.CharacterSheet;

	public CharInfoAbilitiesVM(IReactiveProperty<BaseUnitEntity> unit, BoolReactiveProperty isNotControllableCharacter = null)
		: base(unit)
	{
		IsNotControllableCharacter = isNotControllableCharacter;
		AddDisposable(GroupingMode.Subscribe(delegate
		{
			if (Unit.Value != null)
			{
				RefreshAbilitiesList();
			}
		}));
	}

	public void SetGroupingMode(FeatureGroupingMode mode)
	{
		GroupingMode.Value = mode;
	}

	protected override void DisposeImplementation()
	{
		ActiveAbilities.Clear();
		PassiveAbilities.Clear();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		AssembleActiveAbilities();
		AssemblePassiveAbilities();
		Unit.Value?.UISettings.TryToInitialize();
		if (ActionBarPartAbilitiesVM == null)
		{
			AddDisposable(ActionBarPartAbilitiesVM = new SurfaceActionBarPartAbilitiesVM(isInCharScreen: true));
		}
		ActionBarPartAbilitiesVM.SetUnit(Unit.Value);
	}

	public void RefreshAbilitiesList()
	{
		AssembleActiveAbilities();
		AssemblePassiveAbilities();
		AssembleAugmentations();
	}

	private void AssembleAugmentations()
	{
		Augmentations.Clear();
		UnitAugments unitAugments = Unit.Value.GetOptional<PartUnitBody>()?.Augments;
		if (unitAugments != null && unitAugments.OverdriveAbility != null)
		{
			Augmentations.Add(ToFeatureGroup(new List<Ability> { unitAugments.OverdriveAbility }, UIStrings.Instance.UIAugmentations.CharScreenOverdriveAbilitiesLabel));
		}
		if (unitAugments != null)
		{
			List<ItemEntity> list = (from slot in unitAugments.Slots.Values
				where slot.HasItem
				select slot.Item).ToList();
			if (list.Count > 0)
			{
				Augmentations.Add(ToFeatureGroup(list, UIStrings.Instance.UIAugmentations.CharScreenAllAugmentationsLabel));
			}
		}
	}

	private void AssembleActiveAbilities()
	{
		ActiveAbilities.Clear();
		if (GroupingMode.Value == FeatureGroupingMode.BySource)
		{
			AssembleActiveAbilitiesBySource();
		}
		else
		{
			AssembleActiveAbilitiesByType();
		}
	}

	private void AssembleActiveAbilitiesBySource()
	{
		List<Ability> first = UIUtilityUnit.CollectAbilities(Unit.Value).ToList();
		(List<Ability>, string) abilitiesWithTier = GetAbilitiesWithTier(CareerPathTier.Three);
		(List<Ability>, string) abilitiesWithTier2 = GetAbilitiesWithTier(CareerPathTier.Two);
		(List<Ability>, string) abilitiesWithTier3 = GetAbilitiesWithTier(CareerPathTier.One);
		List<Ability> itemsAndSoulMarksAbilities = GetItemsAndSoulMarksAbilities();
		AddToFeatureGroup(abilitiesWithTier, ActiveAbilities);
		AddToFeatureGroup(abilitiesWithTier2, ActiveAbilities);
		AddToFeatureGroup(abilitiesWithTier3, ActiveAbilities);
		List<Ability> list = first.Except(abilitiesWithTier.Item1).Except(abilitiesWithTier2.Item1).Except(abilitiesWithTier3.Item1)
			.Except(itemsAndSoulMarksAbilities)
			.ToList();
		if (list.Count > 0)
		{
			ActiveAbilities.Add(ToFeatureGroup(list, UIStrings.Instance.CharacterSheet.BackgroundAbilities));
		}
		if (itemsAndSoulMarksAbilities.Count > 0)
		{
			ActiveAbilities.Add(ToFeatureGroup(itemsAndSoulMarksAbilities, Strings.ItemsAbilities));
		}
	}

	private void AssembleActiveAbilitiesByType()
	{
		List<Ability> list = UIUtilityUnit.CollectAbilities(Unit.Value).ToList();
		Dictionary<FeaturesFilter.FeatureFilterType, List<Ability>> dictionary = new Dictionary<FeaturesFilter.FeatureFilterType, List<Ability>>();
		FeaturesFilter.FeatureFilterType[] typeGroupOrder = TypeGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType key in typeGroupOrder)
		{
			dictionary[key] = new List<Ability>();
		}
		List<Ability> list2 = new List<Ability>();
		HashSet<Ability> hashSet = new HashSet<Ability>();
		typeGroupOrder = TypeGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType featureFilterType in typeGroupOrder)
		{
			foreach (Ability item in list)
			{
				if (!hashSet.Contains(item) && item.FirstSource?.Fact?.Blueprint is BlueprintFeature feature && feature.MeetsFilter(featureFilterType))
				{
					dictionary[featureFilterType].Add(item);
					hashSet.Add(item);
				}
			}
		}
		foreach (Ability item2 in list)
		{
			if (!hashSet.Contains(item2))
			{
				list2.Add(item2);
			}
		}
		typeGroupOrder = TypeGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType featureFilterType2 in typeGroupOrder)
		{
			List<Ability> list3 = dictionary[featureFilterType2];
			if (list3.Count != 0)
			{
				ActiveAbilities.Add(ToFeatureGroup(list3, GetFilterTypeTitle(featureFilterType2)));
			}
		}
		if (list2.Count > 0)
		{
			ActiveAbilities.Add(ToFeatureGroup(list2, UIStrings.Instance.CharGen.Other));
		}
	}

	private (List<Ability>, string) GetAbilitiesWithTier(CareerPathTier tier)
	{
		IEnumerable<Ability> source = UIUtilityUnit.CollectAbilities(Unit.Value).Where(HasTier);
		return new ValueTuple<List<Ability>, string>(item2: string.Format(arg0: Unit.Value.Facts.List.FirstOrDefault((EntityFact f) => f.Blueprint is BlueprintCareerPath blueprintCareerPath && blueprintCareerPath.Tier == tier)?.Name, format: Strings.CareerAbilities), item1: source.ToList());
		bool HasTier(Ability ability)
		{
			BlueprintCareerPath obj = ability.FirstSource?.Fact?.FirstSource?.Path as BlueprintCareerPath;
			if (obj == null)
			{
				return false;
			}
			return obj.Tier == tier;
		}
	}

	private void AddToFeatureGroup((List<Ability>, string) abilitiesTier, AutoDisposingList<CharInfoFeatureGroupVM> targetList)
	{
		CharInfoFeatureGroupVM charInfoFeatureGroupVM = ToFeatureGroup(abilitiesTier.Item1, abilitiesTier.Item2);
		if (charInfoFeatureGroupVM != null)
		{
			targetList.Add(charInfoFeatureGroupVM);
		}
	}

	private void AddToFeatureGroup((List<Feature>, string) abilitiesTier, AutoDisposingList<CharInfoFeatureGroupVM> targetList)
	{
		CharInfoFeatureGroupVM charInfoFeatureGroupVM = ToFeatureGroup(abilitiesTier.Item1, abilitiesTier.Item2);
		if (charInfoFeatureGroupVM != null)
		{
			targetList.Add(charInfoFeatureGroupVM);
		}
	}

	private CharInfoFeatureGroupVM ToFeatureGroup(List<Ability> abilities, string name)
	{
		List<CharInfoFeatureVM> list = abilities.Select((Ability a) => new CharInfoFeatureVM(a, Unit.Value)).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		return new CharInfoFeatureGroupVM(list, name, CharInfoFeatureGroupVM.FeatureGroupType.Abilities, name);
	}

	private CharInfoFeatureGroupVM ToFeatureGroup(List<Feature> features, string name)
	{
		return new CharInfoFeatureGroupVM(features.Select((Feature f) => new CharInfoFeatureVM(f, Unit.Value)).ToList(), name, CharInfoFeatureGroupVM.FeatureGroupType.Talents, name);
	}

	private CharInfoFeatureGroupVM ToFeatureGroup(List<ItemEntity> features, string name)
	{
		return new CharInfoFeatureGroupVM(features.Select((ItemEntity f) => new CharInfoFeatureVM(f, Unit.Value)).ToList(), name, CharInfoFeatureGroupVM.FeatureGroupType.Talents, name);
	}

	private List<Ability> GetItemsAndSoulMarksAbilities()
	{
		return new List<Ability>();
	}

	private void AssemblePassiveAbilities()
	{
		PassiveAbilities.Clear();
		if (GroupingMode.Value == FeatureGroupingMode.BySource)
		{
			AssemblePassiveAbilitiesBySource();
		}
		else
		{
			AssemblePassiveAbilitiesByType();
		}
	}

	private void AssemblePassiveAbilitiesBySource()
	{
		List<Feature> first = UIUtilityUnit.CollectFeatures(Unit.Value).ToList();
		(List<Feature>, string) featuresWithTier = GetFeaturesWithTier(CareerPathTier.Three);
		(List<Feature>, string) featuresWithTier2 = GetFeaturesWithTier(CareerPathTier.Two);
		(List<Feature>, string) featuresWithTier3 = GetFeaturesWithTier(CareerPathTier.One);
		List<Feature> itemsAndSoulMarkFeatures = GetItemsAndSoulMarkFeatures();
		List<Feature> postsFeatures = GetPostsFeatures();
		AddToFeatureGroup(featuresWithTier, PassiveAbilities);
		AddToFeatureGroup(featuresWithTier2, PassiveAbilities);
		AddToFeatureGroup(featuresWithTier3, PassiveAbilities);
		List<Feature> list = first.Except(featuresWithTier.Item1).Except(featuresWithTier2.Item1).Except(featuresWithTier3.Item1)
			.Except(itemsAndSoulMarkFeatures)
			.Except(postsFeatures)
			.ToList();
		if (list.Count > 0)
		{
			PassiveAbilities.Add(ToFeatureGroup(list, Strings.BackgroundAbilities));
		}
		if (postsFeatures.Count > 0)
		{
			PassiveAbilities.Add(ToFeatureGroup(postsFeatures, Strings.PostsAbilities));
		}
		if (itemsAndSoulMarkFeatures.Count > 0)
		{
			PassiveAbilities.Add(ToFeatureGroup(itemsAndSoulMarkFeatures, Strings.SoulMarkAbilities));
		}
	}

	private void AssemblePassiveAbilitiesByType()
	{
		List<Feature> list = UIUtilityUnit.CollectFeatures(Unit.Value).ToList();
		Dictionary<FeaturesFilter.FeatureFilterType, List<Feature>> dictionary = new Dictionary<FeaturesFilter.FeatureFilterType, List<Feature>>();
		FeaturesFilter.FeatureFilterType[] typeGroupOrder = TypeGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType key in typeGroupOrder)
		{
			dictionary[key] = new List<Feature>();
		}
		List<Feature> list2 = new List<Feature>();
		HashSet<Feature> hashSet = new HashSet<Feature>();
		typeGroupOrder = TypeGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType featureFilterType in typeGroupOrder)
		{
			foreach (Feature item in list)
			{
				if (!hashSet.Contains(item) && item.Blueprint.MeetsFilter(featureFilterType))
				{
					dictionary[featureFilterType].Add(item);
					hashSet.Add(item);
				}
			}
		}
		foreach (Feature item2 in list)
		{
			if (!hashSet.Contains(item2))
			{
				list2.Add(item2);
			}
		}
		typeGroupOrder = TypeGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType featureFilterType2 in typeGroupOrder)
		{
			List<Feature> list3 = dictionary[featureFilterType2];
			if (list3.Count != 0)
			{
				PassiveAbilities.Add(ToFeatureGroup(list3, GetFilterTypeTitle(featureFilterType2)));
			}
		}
		if (list2.Count > 0)
		{
			PassiveAbilities.Add(ToFeatureGroup(list2, Strings.BackgroundAbilities));
		}
	}

	private List<Feature> GetItemsAndSoulMarkFeatures()
	{
		IEnumerable<Feature> source = UIUtilityUnit.CollectFeatures(Unit.Value);
		List<Feature> list = source.Where((Feature f) => f.FirstSource?.Blueprint is BlueprintSoulMark).ToList();
		list.AddRange(source.Where((Feature f) => f.FirstSource?.Blueprint is BlueprintItem));
		return list;
	}

	private List<Feature> GetPostsFeatures()
	{
		return (from f in UIUtilityUnit.CollectFeatures(Unit.Value)
			where f.Blueprint is BlueprintShipPostExpertise
			select f).ToList();
	}

	private (List<Feature>, string) GetFeaturesWithTier(CareerPathTier tier)
	{
		IEnumerable<Feature> source = UIUtilityUnit.CollectFeatures(Unit.Value).Where(HasTier);
		return new ValueTuple<List<Feature>, string>(item2: string.Format(arg0: Unit.Value.Facts.List.FirstOrDefault((EntityFact f) => f.Blueprint is BlueprintCareerPath blueprintCareerPath && blueprintCareerPath.Tier == tier)?.Name, format: Strings.CareerAbilities), item1: source.ToList());
		bool HasTier(Feature feature)
		{
			BlueprintCareerPath obj = (feature.Blueprint as BlueprintCareerPath) ?? (feature.FirstSource?.Path as BlueprintCareerPath);
			if (obj == null)
			{
				return false;
			}
			return obj.Tier == tier;
		}
	}

	private static string GetFilterTypeTitle(FeaturesFilter.FeatureFilterType filterType)
	{
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		return filterType switch
		{
			FeaturesFilter.FeatureFilterType.ArchetypeFilter => characterSheet.ArchetypeFilterHint, 
			FeaturesFilter.FeatureFilterType.OriginFilter => characterSheet.OriginFilterHint, 
			FeaturesFilter.FeatureFilterType.WarpFilter => characterSheet.WarpFilterHint, 
			FeaturesFilter.FeatureFilterType.OffenseFilter => characterSheet.OffenseFilterHint, 
			FeaturesFilter.FeatureFilterType.DefenseFilter => characterSheet.DefenseFilterHint, 
			FeaturesFilter.FeatureFilterType.SupportFilter => characterSheet.SupportFilterHint, 
			FeaturesFilter.FeatureFilterType.UniversalFilter => characterSheet.UniversalFilterHint, 
			_ => filterType.ToString(), 
		};
	}

	public void MoveSlot(Ability sourceAbility, int targetIndex)
	{
	}

	public void MoveSlot(MechanicActionBarSlot sourceSlot, int sourceIndex, int targetIndex)
	{
	}

	public void DeleteSlot(int sourceIndex)
	{
	}

	public void ChooseAbilityToSlot(int targetIndex)
	{
		ChooseAbilityMode.Value = true;
		TargetSlotIndex = targetIndex;
	}

	public void SetMoveAbilityMode(bool on)
	{
	}
}
