using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;

public class CharInfoAlignmentAbilitySlotPCView : CharInfoComponentView<CharInfoAlignmentAbilitySlotVM>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	private TooltipHandler m_TooltipHandler;

	private SimpleConsoleNavigationEntity m_NavigationEntity;

	public SimpleConsoleNavigationEntity NavigationEntity => m_NavigationEntity;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_NavigationEntity = new SimpleConsoleNavigationEntity(m_Button, base.ViewModel.Tooltip);
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected override void RefreshView()
	{
		m_Button.SetActiveLayer(base.ViewModel.CurrentSlotState.ToString());
		m_TooltipHandler?.Dispose();
		m_TooltipHandler = this.SetTooltip(base.ViewModel.Tooltip);
		AddDisposable(m_TooltipHandler);
		m_NavigationEntity = new SimpleConsoleNavigationEntity(m_Button, base.ViewModel.Tooltip);
	}
}
