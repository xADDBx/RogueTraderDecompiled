using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public abstract class BaseRankEntryFeatureVM : CharInfoFeatureVM
{
	public readonly ReactiveProperty<RankFeatureState> FeatureState = new ReactiveProperty<RankFeatureState>(RankFeatureState.NotActive);

	public readonly BoolReactiveProperty FocusedState = new BoolReactiveProperty();

	public readonly ReactiveProperty<bool> IsCurrentRankEntryItem = new ReactiveProperty<bool>();

	public readonly CareerPathVM CareerPathVM;

	protected readonly UIFeature UIFeature;

	protected readonly ReactiveCommand OnUpdateState = new ReactiveCommand();

	private bool m_HasFavorites;

	private TooltipBaseTemplate m_HintTooltip;

	public BlueprintFeature Feature => UIFeature.Feature;

	public BaseUnitProgressionVM UnitProgressionVM => CareerPathVM.UnitProgressionVM;

	public bool IsRecommended => CareerPathVM.CareerPathUIMetaData?.RecommendedFeatures.Contains(UIFeature.Feature) ?? false;

	public bool IsFavorite
	{
		get
		{
			if (Game.Instance.Player.UISettings.UnitToFavoritesMap.TryGetValue(UnitProgressionVM.Unit.Value.Ref, out var value))
			{
				return value.Contains(UIFeature.Feature.ToReference<BlueprintFeature.Reference>());
			}
			return false;
		}
	}

	public bool HasFavorites => m_HasFavorites;

	public string HintText
	{
		get
		{
			if (IsKeystoneFeature)
			{
				return UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader.Text;
			}
			if (IsUltimateFeature)
			{
				return UIStrings.Instance.CharacterSheet.UltimateUpgradeAbilityFeatureGroupHint.Text;
			}
			if (IsKeystoneAbility)
			{
				return UIStrings.Instance.CharacterSheet.KeystoneAbilitiesHeader.Text;
			}
			return UIStrings.Instance.CharacterSheet.HeaderImprovement.Text;
		}
	}

	public TooltipBaseTemplate HintTooltip => m_HintTooltip ?? (m_HintTooltip = CreateHintTooltip());

	private bool IsKeystoneFeature => CareerPathVM.CareerPathUIMetaData?.KeystoneFeatures.Contains(UIFeature.Feature) ?? false;

	private bool IsUltimateFeature => CareerPathVM.CareerPathUIMetaData?.UltimateFeatures.Contains(UIFeature.Feature) ?? false;

	private bool IsKeystoneAbility
	{
		get
		{
			CareerPathUIMetaData careerPathUIMetaData = CareerPathVM.CareerPathUIMetaData;
			if (careerPathUIMetaData == null)
			{
				return false;
			}
			return careerPathUIMetaData.KeystoneAbilities.Any((BlueprintAbility a) => UIUtilityUnit.GetBlueprintUnitFactFromFact<BlueprintAbility>(UIFeature.Feature)?.Contains(a) ?? false);
		}
	}

	protected BaseRankEntryFeatureVM(CareerPathVM careerPathVM, UIFeature uiFeature)
		: base(uiFeature, careerPathVM.Unit)
	{
		CareerPathVM = careerPathVM;
		UIFeature = uiFeature;
		AddDisposable(UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem item)
		{
			IsCurrentRankEntryItem.Value = item == this;
		}));
	}

	private TooltipBaseTemplate CreateHintTooltip()
	{
		if (IsKeystoneFeature || IsKeystoneAbility)
		{
			return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader.Text, UIStrings.Instance.CharacterSheet.KeystoneFeaturesChargenDescription.Text);
		}
		if (IsUltimateFeature)
		{
			return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.UltimateUpgradeAbilityFeatureGroupHint.Text, UIStrings.Instance.CharacterSheet.UltimateAbilitiesChargenDescription.Text);
		}
		return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.HeaderImprovement, UIStrings.Instance.CharacterSheet.PredefinedAbilitiesChargenDescription.Text);
	}

	protected abstract void UpdateFeatureState();

	public void UpdateState(LevelUpManager levelUpManager)
	{
		UpdateFeatureState();
		OnUpdateState.Execute();
	}

	public abstract void Select();

	public abstract bool CanSelect();

	public void SetHasFavorites(bool hasFavorites)
	{
		m_HasFavorites = hasFavorites;
	}

	public void SetFavoritesState(bool state)
	{
		Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>> unitToFavoritesMap = Game.Instance.Player.UISettings.UnitToFavoritesMap;
		EntityRef<MechanicEntity> @ref = UnitProgressionVM.Unit.Value.Ref;
		if (!unitToFavoritesMap.TryGetValue(@ref, out var value))
		{
			value = new List<BlueprintFeature.Reference>();
			unitToFavoritesMap.Add(@ref, value);
		}
		BlueprintFeature.Reference favoriteFeatureRef = UIFeature.Feature.ToReference<BlueprintFeature.Reference>();
		if (state)
		{
			if (!value.Contains(favoriteFeatureRef))
			{
				value.Add(favoriteFeatureRef);
				unitToFavoritesMap[@ref] = value;
			}
		}
		else if (value.Contains(favoriteFeatureRef))
		{
			value.RemoveAll((BlueprintFeature.Reference f) => f.Equals(favoriteFeatureRef));
			unitToFavoritesMap[@ref] = value;
		}
	}

	public void SetFocusOn(BaseRankEntryFeatureVM featureVM)
	{
		FocusedState.Value = featureVM == this;
	}
}
