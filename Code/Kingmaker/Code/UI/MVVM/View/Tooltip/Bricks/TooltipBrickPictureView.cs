using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPictureView : TooltipBaseBrickView<TooltipBrickPictureVM>
{
	[SerializeField]
	private Image m_Image;

	protected override void BindViewImplementation()
	{
		m_Image.sprite = base.ViewModel.Picture;
		m_Image.color = base.ViewModel.PictureColor;
	}
}
