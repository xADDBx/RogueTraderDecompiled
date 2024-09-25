using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Colonization;

public class ProfitResourcePCView : ViewBase<ProfitResourceVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ProfitResourceValue;

	[SerializeField]
	private Image m_TooltipArea;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ProfitResource.Subscribe(delegate(float currentProfit)
		{
			m_ProfitResourceValue.text = currentProfit.ToString("F1");
		}));
		AddDisposable(m_TooltipArea.SetTooltip(new TooltipTemplateSimple(UIStrings.Instance.ColonizationTexts.ResourceStrings[1].Name, UIStrings.Instance.ColonizationTexts.ResourceStrings[1].Description)));
	}

	public void SetData()
	{
		base.ViewModel.ProfitResource.Value = Game.Instance.Player.ProfitFactor.Total;
	}

	public void UpdateData()
	{
		base.ViewModel.ProfitResource.Value = Game.Instance.Player.ProfitFactor.Total;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
