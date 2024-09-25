using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFeature : ITooltipBrick
{
	private readonly TooltipBrickFeatureVM m_FeatureVM;

	public TooltipBrickFeature(BlueprintFeature feature, bool isHeader = false, bool available = true, bool showIcon = true, MechanicEntity caster = null, bool forceSetName = false, bool isHidden = false)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(feature, isHeader, available, showIcon, null, caster, forceSetName, isHidden);
	}

	public TooltipBrickFeature(string name, Sprite icon, bool isHeader = false, bool available = true, bool showIcon = true)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(name, icon, isHeader, available, showIcon);
	}

	public TooltipBrickFeature(BlueprintAbility ability, bool isHeader = false, MechanicEntity caster = null, bool isHidden = false)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(ability, isHeader, null, caster, isHidden);
	}

	public TooltipBrickFeature(IUIDataProvider dataProvider, bool isHeader = false)
	{
		if (dataProvider is BlueprintFeature feature)
		{
			m_FeatureVM = new TooltipBrickFeatureVM(feature, isHeader, available: true);
		}
		else
		{
			m_FeatureVM = new TooltipBrickFeatureVM(dataProvider, isHeader);
		}
	}

	public TooltipBaseBrickVM GetVM()
	{
		return m_FeatureVM;
	}
}
