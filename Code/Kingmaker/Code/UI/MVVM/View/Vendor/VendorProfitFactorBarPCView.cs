using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorProfitFactorBarPCView : ViewBase<ProfitFactorVM>
{
	[Header("Text Fields")]
	[SerializeField]
	private TextMeshProUGUI m_TotalValueText;

	[SerializeField]
	private TextMeshProUGUI m_CurrentValueText;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentValue.CombineLatest(base.ViewModel.LockedValue, base.ViewModel.TotalValue, base.ViewModel.DiffValue, (float current, float locked, float total, float diff) => new { current, locked, total, diff }).Subscribe(profit =>
		{
			if (profit.total != 0f)
			{
				m_CurrentValueText.text = UIUtility.GetProfitFactorFormatted(profit.current);
				m_TotalValueText.text = UIUtility.GetProfitFactorFormatted(profit.total);
			}
		}));
		AddDisposable(this.SetTooltip(new TooltipTemplateProfitFactor(base.ViewModel), new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
