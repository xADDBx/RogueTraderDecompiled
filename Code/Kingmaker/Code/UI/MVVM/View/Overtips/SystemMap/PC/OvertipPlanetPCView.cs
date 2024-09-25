using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.PC;

public class OvertipPlanetPCView : OvertipPlanetView
{
	[SerializeField]
	private RectTransform m_TooltipPlace;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_TooltipTaker.SetTooltip(new TooltipTemplateSystemMapPlanet(SystemObject, base.ViewModel.PlanetView.Value), new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace)));
	}
}
