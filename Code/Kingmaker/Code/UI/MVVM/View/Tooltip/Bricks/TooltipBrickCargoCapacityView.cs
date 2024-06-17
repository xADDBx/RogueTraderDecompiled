using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickCargoCapacityView : TooltipBaseBrickView<TooltipBrickCargoCapacityVM>
{
	[Header("Fill Details")]
	[SerializeField]
	private TextMeshProUGUI m_TotalFillValue;

	[SerializeField]
	private Image m_UsableFillBar;

	[SerializeField]
	private Image m_UnusableFillBar;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.TotalFillValue.Subscribe(delegate(int value)
		{
			m_TotalFillValue.text = $"{value}%";
		}));
		AddDisposable(base.ViewModel.TotalFillValue.CombineLatest(base.ViewModel.UnusableFillValue, (int total, int unusable) => new { total, unusable }).Subscribe(value =>
		{
			int num = value.total - value.unusable;
			m_UsableFillBar.fillAmount = (float)num / 100f;
			m_UnusableFillBar.fillAmount = (float)value.total / 100f;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
