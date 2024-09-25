using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickButtonView : TooltipBaseBrickView<TooltipBrickButtonVM>
{
	[SerializeField]
	private OwlcatButton m_Button;

	[SerializeField]
	protected TextMeshProUGUI m_Text;

	protected override void BindViewImplementation()
	{
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClick();
		}));
		m_Text.text = base.ViewModel.Text;
	}
}
