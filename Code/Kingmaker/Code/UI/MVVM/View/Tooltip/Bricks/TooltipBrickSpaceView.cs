using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickSpaceView : TooltipBaseBrickView<TooltipBrickSpaceVM>
{
	[SerializeField]
	private LayoutElement m_LayoutElement;

	protected override void BindViewImplementation()
	{
		if ((bool)m_LayoutElement && base.ViewModel.Height.HasValue)
		{
			m_LayoutElement.minHeight = base.ViewModel.Height.Value;
		}
	}
}
