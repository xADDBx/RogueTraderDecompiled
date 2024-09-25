using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorSelectingWindowPCView : VendorSelectingWindowBaseView
{
	[SerializeField]
	private OwlcatButton CloseButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnCloseClick();
		}));
	}
}
