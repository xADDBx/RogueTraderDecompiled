using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickTimerView : TooltipBaseBrickView<TooltipBrickTimerVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private GameObject m_TimeIcon;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Text.Subscribe(delegate(string t)
		{
			m_Text.text = t;
		}));
		m_TimeIcon.SetActive(base.ViewModel.ShowTimeIcon);
	}
}
