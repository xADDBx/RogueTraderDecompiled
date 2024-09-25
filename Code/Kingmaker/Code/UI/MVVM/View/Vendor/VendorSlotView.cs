using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorSlotView : VendorGenericSlotView<ItemSlotBaseView>, IHasTooltipTemplates
{
	[SerializeField]
	private TextMeshProUGUI m_DisplayNameText;

	[SerializeField]
	private TextMeshProUGUI m_TypeText;

	[SerializeField]
	private GameObject m_DiscountBlock;

	[SerializeField]
	private TextMeshProUGUI m_OldCostText;

	[SerializeField]
	private TextMeshProUGUI m_CurrentCostText;

	[SerializeField]
	private Image m_Frame;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (base.ViewModel.HasDiscount)
		{
			m_OldCostText.text = ((base.ViewModel.PriceWithoutDiscountPF.Value > 0.0) ? UIUtility.GetProfitFactorFormatted((float)base.ViewModel.PriceWithoutDiscountPF.Value) : string.Empty);
		}
		m_DiscountBlock.gameObject.SetActive(base.ViewModel.HasDiscount);
		AddDisposable(base.ViewModel.CurrentCostPF.Subscribe(delegate(double value)
		{
			m_CurrentCostText.text = ((value > 0.0) ? UIUtility.GetProfitFactorFormatted((float)value) : string.Empty);
		}));
		AddDisposable(base.ViewModel.TypeName.Subscribe(delegate(string value)
		{
			m_TypeText.text = value;
		}));
		AddDisposable(base.ViewModel.DisplayName.Subscribe(delegate(string value)
		{
			m_DisplayNameText.text = value;
		}));
	}

	public override void RefreshItem()
	{
		base.RefreshItem();
		m_ItemSlotView.SetLayer(!base.ViewModel.IsLockedByRep && !base.ViewModel.IsLockedByCost);
		m_Frame.raycastTarget = base.ViewModel.IsLockedByRep || base.ViewModel.IsLockedByCost;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.Value;
	}
}
