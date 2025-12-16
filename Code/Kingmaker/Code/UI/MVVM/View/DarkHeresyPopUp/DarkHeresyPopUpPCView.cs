using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DarkHeresyPopUp;

public class DarkHeresyPopUpPCView : DarkHeresyPopUpView
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(Hide));
		AddDisposable(m_WishlistButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			Hide();
			base.ViewModel.OpenStoreToWishlist();
		}));
	}
}
