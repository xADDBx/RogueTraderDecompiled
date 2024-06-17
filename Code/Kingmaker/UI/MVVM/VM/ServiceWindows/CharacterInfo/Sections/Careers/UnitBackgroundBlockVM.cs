using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;

public class UnitBackgroundBlockVM : CharInfoComponentVM
{
	public readonly ReactiveProperty<BlueprintFeature> Homeworld = new ReactiveProperty<BlueprintFeature>();

	public readonly ReactiveProperty<BlueprintFeature> Occupation = new ReactiveProperty<BlueprintFeature>();

	public readonly ReactiveProperty<BlueprintFeature> MomentOfTriumph = new ReactiveProperty<BlueprintFeature>();

	public readonly ReactiveProperty<BlueprintFeature> DarkestHour = new ReactiveProperty<BlueprintFeature>();

	public readonly ReactiveProperty<TooltipBaseTemplate> HomeworldTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<TooltipBaseTemplate> OccupationTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<TooltipBaseTemplate> MomentOfTriumphTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<TooltipBaseTemplate> DarkestHourTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public UnitBackgroundBlockVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateSelections();
	}

	private BlueprintFeature GetCharGenSelectionFeature(BaseUnitEntity unit, FeatureGroup group)
	{
		BlueprintOriginPath unitOriginPath = CharGenUtility.GetUnitOriginPath(unit);
		if (unitOriginPath == null)
		{
			return null;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = CharGenUtility.GetFeatureSelectionsByGroup(unitOriginPath, group).FirstOrDefault();
		if (blueprintSelectionFeature != null)
		{
			return unit.Progression.GetSelectedFeature(unitOriginPath, 0, blueprintSelectionFeature)?.Feature;
		}
		return null;
	}

	private void UpdateSelections(BaseUnitEntity unit = null)
	{
		if (unit == null)
		{
			unit = Unit.Value;
		}
		if (unit != null)
		{
			bool flag = !unit.IsMainCharacter;
			Homeworld.Value = GetCharGenSelectionFeature(unit, FeatureGroup.ChargenHomeworld);
			Occupation.Value = GetCharGenSelectionFeature(unit, FeatureGroup.ChargenOccupation);
			MomentOfTriumph.Value = (flag ? null : GetCharGenSelectionFeature(unit, FeatureGroup.ChargenMomentOfTriumph));
			DarkestHour.Value = (flag ? null : GetCharGenSelectionFeature(unit, FeatureGroup.ChargenDarkestHour));
			HomeworldTooltip.Value = new TooltipTemplateChargenBackground(Homeworld.Value);
			OccupationTooltip.Value = new TooltipTemplateChargenBackground(Occupation.Value);
			MomentOfTriumphTooltip.Value = new TooltipTemplateChargenBackground(MomentOfTriumph.Value);
			DarkestHourTooltip.Value = new TooltipTemplateChargenBackground(DarkestHour.Value);
		}
	}

	public void UpdateSelectionsFromUnit(BaseUnitEntity unit)
	{
		UpdateSelections(unit);
	}
}
