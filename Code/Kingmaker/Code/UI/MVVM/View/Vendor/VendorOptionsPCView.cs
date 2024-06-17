using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorOptionsPCView : ViewBase<VendorOptionsVM>
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private VendorOptionsItemPCView m_ItemView;

	[SerializeField]
	private RectTransform m_Container;

	private bool m_IsShow;

	private readonly List<VendorOptionsItemPCView> m_Items = new List<VendorOptionsItemPCView>();

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_IsShow = false;
		foreach (VendorOptionsItemVM itemVm in base.ViewModel.ItemVms)
		{
			VendorOptionsItemPCView widget = WidgetFactory.GetWidget(m_ItemView);
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Bind(itemVm);
			m_Items.Add(widget);
		}
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			Show();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_Items.ForEach(WidgetFactory.DisposeWidget);
		m_Items.Clear();
	}

	public void Show()
	{
		m_IsShow = !m_IsShow;
		base.gameObject.SetActive(m_IsShow);
	}
}
