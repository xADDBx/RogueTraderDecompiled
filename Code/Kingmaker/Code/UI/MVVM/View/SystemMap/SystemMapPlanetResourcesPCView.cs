using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

public class SystemMapPlanetResourcesPCView : TooltipBrickResourceInfoView, IWidgetView
{
	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((TooltipBrickResourceInfoVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TooltipBrickResourceInfoVM;
	}
}
