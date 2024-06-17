using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;

public class EncyclopediaPageImageBaseView : ViewBase<EncyclopediaPageImageVM>, IWidgetView
{
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	protected OwlcatButton m_ZoomButton;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Image.sprite = base.ViewModel.Image;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as EncyclopediaPageImageVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is EncyclopediaPageImageVM;
	}

	public void OnButtonClick()
	{
		base.ViewModel.HandleZoomClick();
	}
}
