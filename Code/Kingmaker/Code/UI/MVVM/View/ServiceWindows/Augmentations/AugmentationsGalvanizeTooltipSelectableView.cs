using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Augmentations;

public class AugmentationsGalvanizeTooltipSelectableView : OwlcatMultiButton, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	public TooltipTemplateGlossary Tooltip;

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public void SetTooltipTemplate(TooltipTemplateGlossary template)
	{
		Tooltip = template;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return Tooltip;
	}
}
