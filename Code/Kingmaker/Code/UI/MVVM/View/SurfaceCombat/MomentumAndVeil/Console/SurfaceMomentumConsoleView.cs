using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ActionBar.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil.Console;

public class SurfaceMomentumConsoleView : SurfaceMomentumView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		m_SurfaceMomentumEntityView.HighlightHeroicAct(value);
		m_SurfaceMomentumEntityView.HighlightDesperateMeasure(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.MomentumEntityVM?.Value?.Tooltip?.Value;
	}

	public List<IConsoleNavigationEntity> GetDesperateMeasureSlots()
	{
		return m_DesperateMeasureWidgetList.GetNavigationEntities();
	}

	public List<IConsoleNavigationEntity> GetHeroicActSlots()
	{
		return m_HeroicActWidgetList.GetNavigationEntities();
	}

	public void OnFocusEntity(IConsoleEntity entity)
	{
		SurfaceActionBarSlotAbilityConsoleView surfaceActionBarSlotAbilityConsoleView = entity as SurfaceActionBarSlotAbilityConsoleView;
		bool flag = surfaceActionBarSlotAbilityConsoleView != null && surfaceActionBarSlotAbilityConsoleView.IsHeroicAct;
		bool flag2 = surfaceActionBarSlotAbilityConsoleView != null && surfaceActionBarSlotAbilityConsoleView.IsDesperateMeasure;
		m_SurfaceMomentumEntityView.HighlightHeroicAct(flag || m_Button.IsFocus);
		m_SurfaceMomentumEntityView.HighlightDesperateMeasure(flag2 || m_Button.IsFocus);
	}
}
