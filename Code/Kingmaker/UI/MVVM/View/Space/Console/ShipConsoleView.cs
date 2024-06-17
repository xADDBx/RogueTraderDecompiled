using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Space.Console;

public class ShipConsoleView : ShipPCView
{
	[SerializeField]
	private OwlcatMultiButton m_HPBarTooltipObject;

	[SerializeField]
	private OwlcatMultiButton m_VoidshipHealthTextTooltipObject;

	[SerializeField]
	private OwlcatMultiButton m_RepairBlock;

	public List<IFloatConsoleNavigationEntity> GetNavigation()
	{
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>
		{
			new SimpleConsoleNavigationEntity(m_ProwRamDamageBlock, ProwRamDamageTooltip, null, TooltipPlace),
			new SimpleConsoleNavigationEntity(m_ProwRamSelfDamageReduceBlock, ProwRamSelfDamageReduceTooltip, null, TooltipPlace)
		};
		OwlcatMultiButton[] hulls = m_Hulls;
		foreach (OwlcatMultiButton button in hulls)
		{
			list.Add(new SimpleConsoleNavigationEntity(button, HullTooltip, null, TooltipPlace));
		}
		hulls = m_Shields;
		foreach (OwlcatMultiButton button2 in hulls)
		{
			list.Add(new SimpleConsoleNavigationEntity(button2, ShieldsTooltip, null, TooltipPlace));
		}
		list.Add(new SimpleConsoleNavigationEntity(m_RepairBlock, new TooltipTemplateSimple(UIStrings.Instance.ShipCustomization.Repair, UIStrings.Instance.ShipCustomization.RepairDescription)));
		list.Add(new SimpleConsoleNavigationEntity(m_HPBarTooltipObject, new TooltipTemplateGlossary("HullIntegritySpace"), null, TooltipPlace));
		list.Add(new SimpleConsoleNavigationEntity(m_VoidshipHealthTextTooltipObject, new TooltipTemplateGlossary("HullIntegritySpace"), null, TooltipPlace));
		return list;
	}
}
