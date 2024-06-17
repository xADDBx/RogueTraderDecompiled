using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFeature : ITooltipBrick
{
	private readonly TooltipBrickFeatureVM m_FeatureVM;

	public TooltipBrickFeature(BlueprintFeatureBase feature, bool isHeader = false, bool available = true, bool showIcon = true, FeatureTypes type = FeatureTypes.Common, MechanicEntity caster = null)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(feature, isHeader, available, showIcon, null, type, caster);
	}

	public TooltipBrickFeature(string name, Sprite icon, bool isHeader = false, bool available = true, bool showIcon = true, FeatureTypes type = FeatureTypes.Common)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(name, icon, isHeader, available, showIcon, null, type);
	}

	public TooltipBrickFeature(BlueprintAbility ability, bool isHeader = false, FeatureTypes type = FeatureTypes.Common, MechanicEntity caster = null)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(ability, isHeader, null, type, caster);
	}

	public TooltipBrickFeature(ActivatableAbility activatableAbility, bool isHeader = false)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(activatableAbility, isHeader);
	}

	public TooltipBrickFeature(BlueprintActivatableAbility activatableAbility, bool isHeader = false, FeatureTypes type = FeatureTypes.Common)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(activatableAbility, isHeader, null, type);
	}

	public TooltipBrickFeature(IUIDataProvider dataProvider, bool isHeader = false, FeatureTypes type = FeatureTypes.Common)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(dataProvider, isHeader, null, type);
	}

	public TooltipBrickFeature(UIUtilityItem.UIAbilityData uiAbilityData, bool isHeader = false, FeatureTypes type = FeatureTypes.Common)
	{
		m_FeatureVM = new TooltipBrickFeatureVM(uiAbilityData, isHeader, null, type);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return m_FeatureVM;
	}
}
