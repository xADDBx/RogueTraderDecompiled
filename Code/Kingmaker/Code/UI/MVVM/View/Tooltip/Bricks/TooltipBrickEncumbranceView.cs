using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickEncumbranceView : TooltipBaseBrickView<TooltipBrickEncumbranceVM>
{
	[SerializeField]
	private EncumbranceView m_EncumbranceView;

	protected override void BindViewImplementation()
	{
		m_EncumbranceView.Bind(base.ViewModel.EncumbranceVM);
	}
}
