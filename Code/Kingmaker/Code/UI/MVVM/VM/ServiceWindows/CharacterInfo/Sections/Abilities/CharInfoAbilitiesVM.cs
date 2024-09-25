using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.Blueprints;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoAbilitiesVM : CharInfoComponentVM, IActionBarPartAbilitiesHandler, ISubscriber
{
	public readonly AutoDisposingList<CharInfoFeatureGroupVM> ActiveAbilities = new AutoDisposingList<CharInfoFeatureGroupVM>();

	public readonly AutoDisposingList<CharInfoFeatureGroupVM> PassiveAbilities = new AutoDisposingList<CharInfoFeatureGroupVM>();

	public SurfaceActionBarPartAbilitiesVM ActionBarPartAbilitiesVM;

	public readonly BoolReactiveProperty ChooseAbilityMode = new BoolReactiveProperty();

	public int TargetSlotIndex = -1;

	private static UITextCharSheet Strings => UIStrings.Instance.CharacterSheet;

	public CharInfoAbilitiesVM(IReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
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

	private void AssembleActiveAbilities()
	{
		List<Ability> first = UIUtilityUnit.CollectAbilities(Unit.Value).ToList();
		(List<Ability>, string) abilitiesWithTier = GetAbilitiesWithTier(CareerPathTier.Three);
		(List<Ability>, string) abilitiesWithTier2 = GetAbilitiesWithTier(CareerPathTier.Two);
		(List<Ability>, string) abilitiesWithTier3 = GetAbilitiesWithTier(CareerPathTier.One);
		List<Ability> itemsAndSoulMarksAbilities = GetItemsAndSoulMarksAbilities();
		ActiveAbilities.Clear();
		ActiveAbilities.Add(ToFeatureGroup(abilitiesWithTier.Item1, abilitiesWithTier.Item2));
		ActiveAbilities.Add(ToFeatureGroup(abilitiesWithTier2.Item1, abilitiesWithTier2.Item2));
		ActiveAbilities.Add(ToFeatureGroup(abilitiesWithTier3.Item1, abilitiesWithTier3.Item2));
		List<Ability> abilities = first.Except(abilitiesWithTier.Item1).Except(abilitiesWithTier2.Item1).Except(abilitiesWithTier3.Item1)
			.Except(itemsAndSoulMarksAbilities)
			.ToList();
		ActiveAbilities.Add(ToFeatureGroup(abilities, UIStrings.Instance.CharacterSheet.BackgroundAbilities));
		ActiveAbilities.Add(ToFeatureGroup(itemsAndSoulMarksAbilities, Strings.ItemsAbilities));
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

	private CharInfoFeatureGroupVM ToFeatureGroup(List<Ability> abilities, string name)
	{
		return new CharInfoFeatureGroupVM(abilities.Select((Ability a) => new CharInfoFeatureVM(a, Unit.Value)).ToList(), name, CharInfoFeatureGroupVM.FeatureGroupType.Abilities, name);
	}

	private CharInfoFeatureGroupVM ToFeatureGroup(List<Feature> features, string name)
	{
		return new CharInfoFeatureGroupVM(features.Select((Feature f) => new CharInfoFeatureVM(f, Unit.Value)).ToList(), name, CharInfoFeatureGroupVM.FeatureGroupType.Talents, name);
	}

	private List<Ability> GetItemsAndSoulMarksAbilities()
	{
		IEnumerable<Ability> visible = Unit.Value.Abilities.Visible;
		List<Ability> list = (from a in visible
			where !a.Blueprint.IsCantrip && a.SourceItem != null
			where a.SourceItem != Unit.Value.GetFirstWeapon() && a.SourceItem != Unit.Value.GetSecondaryHandWeapon()
			select a).ToList();
		List<Ability> collection = visible.Where((Ability f) => f.FirstSource?.Blueprint is BlueprintSoulMark).ToList();
		list.AddRange(collection);
		return list;
	}

	private void AssemblePassiveAbilities()
	{
		List<Feature> first = UIUtilityUnit.CollectFeatures(Unit.Value).ToList();
		(List<Feature>, string) featuresWithTier = GetFeaturesWithTier(CareerPathTier.Three);
		(List<Feature>, string) featuresWithTier2 = GetFeaturesWithTier(CareerPathTier.Two);
		(List<Feature>, string) featuresWithTier3 = GetFeaturesWithTier(CareerPathTier.One);
		List<Feature> itemsAndSoulMarkFeatures = GetItemsAndSoulMarkFeatures();
		List<Feature> postsFeatures = GetPostsFeatures();
		PassiveAbilities.Clear();
		PassiveAbilities.Add(ToFeatureGroup(featuresWithTier.Item1, featuresWithTier.Item2));
		PassiveAbilities.Add(ToFeatureGroup(featuresWithTier2.Item1, featuresWithTier2.Item2));
		PassiveAbilities.Add(ToFeatureGroup(featuresWithTier3.Item1, featuresWithTier3.Item2));
		List<Feature> features = first.Except(featuresWithTier.Item1).Except(featuresWithTier2.Item1).Except(featuresWithTier3.Item1)
			.Except(itemsAndSoulMarkFeatures)
			.Except(postsFeatures)
			.ToList();
		PassiveAbilities.Add(ToFeatureGroup(features, Strings.BackgroundAbilities));
		PassiveAbilities.Add(ToFeatureGroup(postsFeatures, Strings.PostsAbilities));
		PassiveAbilities.Add(ToFeatureGroup(itemsAndSoulMarkFeatures, Strings.SoulMarkAbilities));
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

	protected override void DisposeImplementation()
	{
		ActiveAbilities.Clear();
		PassiveAbilities.Clear();
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
