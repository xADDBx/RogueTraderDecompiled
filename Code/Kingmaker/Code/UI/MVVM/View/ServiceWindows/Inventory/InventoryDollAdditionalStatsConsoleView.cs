using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public class InventoryDollAdditionalStatsConsoleView : InventoryDollAdditionalStatsPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	private enum NavigationLayout
	{
		Vertical,
		Horizontal
	}

	[SerializeField]
	private NavigationLayout m_NavigationLayout;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private ICharInfoComponentConsoleView m_CharInfoComponentConsoleViewImplementation;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		TooltipConfig tooltipConfig = new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true);
		SimpleConsoleNavigationEntity item = new SimpleConsoleNavigationEntity(m_DeflectionTooltip, new TooltipTemplateGlossary("Deflection", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item2 = new SimpleConsoleNavigationEntity(m_AbsorptionTooltip, new TooltipTemplateGlossary("Absorption", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item3 = new SimpleConsoleNavigationEntity(m_DodgeTooltip, new TooltipTemplateGlossary("Dodge", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item4 = new SimpleConsoleNavigationEntity(m_DodgePenetrationTooltip, new TooltipTemplateGlossary("DodgeReduction", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item5 = new SimpleConsoleNavigationEntity(m_ResolveTooltip, new TooltipTemplateGlossary("Resolve", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item6 = new SimpleConsoleNavigationEntity(m_ParryTooltip, new TooltipTemplateGlossary("Parry", tooltipConfig.IsGlossary));
		List<SimpleConsoleNavigationEntity> entities = new List<SimpleConsoleNavigationEntity> { item, item2, item3, item4, item6, item5 };
		switch (m_NavigationLayout)
		{
		case NavigationLayout.Vertical:
			m_NavigationBehaviour.AddColumn(entities);
			break;
		case NavigationLayout.Horizontal:
			m_NavigationBehaviour.AddRow(entities);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_NavigationBehaviour;
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
