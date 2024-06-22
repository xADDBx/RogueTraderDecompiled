using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickProfitFactorView : TooltipBaseBrickView<TooltipBrickProfitFactorVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TotalValueText;

	[SerializeField]
	private TextMeshProUGUI m_CurrentValueText;

	[SerializeField]
	private TextMeshProUGUI m_RestoreSpeedText;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ProfitFactorVM.TotalValue.Subscribe(delegate(float value)
		{
			m_TotalValueText.text = value.ToString("0.#");
		}));
		AddDisposable(base.ViewModel.ProfitFactorVM.TotalValue.Subscribe(delegate(float value)
		{
			m_CurrentValueText.text = value.ToString("0.#");
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
