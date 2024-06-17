using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureGroupPCView : ViewBase<CharInfoFeatureGroupVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoFeatureBaseView m_WidgetEntityView;

	[SerializeField]
	protected ExpandableCollapseMultiButtonPC m_ExpandableElement;

	private AccessibilityTextHelper m_TextHelper;

	public bool IsEmpty => base.ViewModel.IsEmpty;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label);
		}
		base.gameObject.SetActive(!base.ViewModel.IsEmpty);
		if (!base.ViewModel.IsEmpty)
		{
			SetupLabel();
			DrawEntities();
			m_TextHelper.UpdateTextSize();
			_ = base.ViewModel.TooltipKey;
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}

	private void SetupLabel()
	{
		if (m_Label != null)
		{
			m_Label.text = base.ViewModel.Label;
		}
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.FeatureList.ToArray(), m_WidgetEntityView, strictMatching: true);
	}

	public void Expand()
	{
		m_ExpandableElement.SetValue(isOn: true, isImmediately: true);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoFeatureGroupVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoFeatureGroupVM;
	}
}
