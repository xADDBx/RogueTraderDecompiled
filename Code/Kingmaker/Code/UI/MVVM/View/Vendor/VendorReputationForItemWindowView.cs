using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorReputationForItemWindowView : ViewBase<VendorReputationForItemWindowVM>
{
	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[SerializeField]
	private VirtualListElementViewBase<VendorReputationForItemVM> m_ReputationForItemPrefab;

	public void Initialize()
	{
		m_VirtualList.Initialize(new VirtualListElementTemplate<VendorReputationForItemVM>(m_ReputationForItemPrefab));
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.AcceptItems));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
