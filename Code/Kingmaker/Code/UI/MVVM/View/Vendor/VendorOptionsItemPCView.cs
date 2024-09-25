using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorOptionsItemPCView : ViewBase<VendorOptionsItemVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string value)
		{
			m_TitleText.text = value;
		}));
		AddDisposable(base.ViewModel.State.Subscribe(delegate(bool value)
		{
			m_Button.SetActiveLayer(value ? 1 : 0);
		}));
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SwitchOption();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
