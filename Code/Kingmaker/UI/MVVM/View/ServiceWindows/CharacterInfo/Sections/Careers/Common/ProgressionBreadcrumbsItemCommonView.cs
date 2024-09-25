using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;

public class ProgressionBreadcrumbsItemCommonView : ViewBase<ProgressionBreadcrumbsItemVM>, IWidgetView
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	private AccessibilityTextHelper m_TextHelper;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label);
		}
		m_Button.SetActiveLayer(base.ViewModel.IsCurrent ? "Current" : "Default");
		m_Button.SetInteractable(!base.ViewModel.IsCurrent);
		m_Label.text = base.ViewModel.Text;
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.HandleClick();
		}));
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ProgressionBreadcrumbsItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ProgressionBreadcrumbsItemVM;
	}
}
