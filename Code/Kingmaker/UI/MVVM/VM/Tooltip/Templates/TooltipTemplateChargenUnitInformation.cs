using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateChargenUnitInformation : TooltipBaseTemplate
{
	private readonly BaseUnitEntity m_Unit;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly IList<CareerPathVM> m_Careers;

	private readonly bool m_ExpandedView;

	public TooltipTemplateChargenUnitInformation(BaseUnitEntity unit, LevelUpManager levelUpManager, IList<CareerPathVM> careers, bool expandedView = false)
	{
		m_Unit = unit;
		m_LevelUpManager = levelUpManager;
		m_Careers = careers;
		m_ExpandedView = expandedView;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Unit.CharacterName);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddCareers(list);
		AddBackgroundItems(list);
		AddAbilities(list);
		return list;
	}

	private void AddCareers(List<ITooltipBrick> bricks)
	{
		if (m_Careers.Count != 0)
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CharacterSheet.LevelProgression, TooltipTitleType.H3));
			bricks.Add(new TooltipBricksGroupStart(hasBackground: false, new TooltipBricksGroupLayoutParams
			{
				LayoutType = TooltipBricksGroupLayoutType.Vertical
			}));
			bricks.AddRange(m_Careers.Select((CareerPathVM career) => new TooltipBrickTitleWithIcon(career)));
			bricks.Add(new TooltipBricksGroupEnd());
			bricks.Add(new TooltipBrickSpace(14f));
		}
	}

	private void AddBackgroundItems(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CharGen.Background, TooltipTitleType.H3));
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, GetLayoutParams()));
		bricks.Add(GetBackgroundBrick(FeatureGroup.ChargenHomeworld));
		bricks.Add(GetBackgroundBrick(FeatureGroup.ChargenImperialWorld));
		bricks.Add(GetBackgroundBrick(FeatureGroup.ChargenOccupation));
		bricks.Add(GetBackgroundBrick(FeatureGroup.ChargenMomentOfTriumph));
		bricks.Add(GetBackgroundBrick(FeatureGroup.ChargenDarkestHour));
		bricks.Add(new TooltipBricksGroupEnd());
		bricks.Add(new TooltipBrickSpace(14f));
	}

	private ITooltipBrick GetBackgroundBrick(FeatureGroup group)
	{
		BlueprintPath path = m_LevelUpManager.Path;
		if (path == null)
		{
			return null;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = CharGenUtility.GetFeatureSelectionsByGroup(path, group).FirstOrDefault();
		if (blueprintSelectionFeature == null)
		{
			return null;
		}
		(BlueprintFeature, int)? selectedFeature = m_Unit.Progression.GetSelectedFeature(path, 0, blueprintSelectionFeature);
		if (selectedFeature.HasValue)
		{
			return new TooltipBrickBackgroundFeature(selectedFeature.Value.Item1, m_Unit);
		}
		return null;
	}

	private void AddAbilities(List<ITooltipBrick> bricks)
	{
		List<IUIDataProvider> list = new List<IUIDataProvider>();
		list.AddRange(UIUtilityUnit.CollectAbilities(m_Unit));
		list.AddRange(UIUtilityUnit.CollectActivatableAbilities(m_Unit));
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CharacterSheet.Abilities, TooltipTitleType.H3));
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, GetLayoutParams()));
		bricks.AddRange(list.Select((IUIDataProvider ability) => new TooltipBrickFeature(ability)));
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private TooltipBricksGroupLayoutParams GetLayoutParams()
	{
		return new TooltipBricksGroupLayoutParams
		{
			LayoutType = TooltipBricksGroupLayoutType.Grid,
			Padding = new RectOffset(8, 8, 0, 0),
			PreferredElementHeight = (m_ExpandedView ? 72f : 62f),
			CellSize = (m_ExpandedView ? new Vector2(300f, 72f) : new Vector2(256f, 62f))
		};
	}
}
