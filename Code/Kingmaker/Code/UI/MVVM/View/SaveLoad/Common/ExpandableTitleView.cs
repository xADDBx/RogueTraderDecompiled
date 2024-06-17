using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Common;

public class ExpandableTitleView : VirtualListElementViewBase<ExpandableTitleVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IVirtualListElementIdentifier
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private OwlcatMultiButton m_ExpandButton;

	[SerializeField]
	private Transform m_ExpandArrow;

	[SerializeField]
	private float m_CollapsedAngle = 90f;

	[SerializeField]
	private bool m_HasNavigation = true;

	[ConditionalShow("m_HasNavigation")]
	[SerializeField]
	private OwlcatMultiButton m_ConsoleFocusButton;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	private bool m_IsExpanded;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public bool IsExpanded => base.ViewModel.IsExpanded.Value;

	public int VirtualListTypeId => 0;

	protected override void BindViewImplementation()
	{
		m_Title.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.IsExpanded.Subscribe(ExpandStatedChanged));
		if (base.ViewModel.IsSwitchable)
		{
			AddDisposable(m_ExpandButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				Switch();
			}));
		}
		m_ExpandArrow.Or(null)?.gameObject.SetActive(base.ViewModel.IsSwitchable);
	}

	private void ExpandStatedChanged(bool expanded)
	{
		m_ExpandButton.SetActiveLayer(expanded ? "Expanded" : "Collapsed");
		m_ConsoleFocusButton.ConfirmClickHint = (expanded ? UIStrings.Instance.CommonTexts.Collapse : UIStrings.Instance.CommonTexts.Expand);
		m_ExpandArrow.Or(null)?.DOLocalRotate(new Vector3(0f, 0f, expanded ? 0f : m_CollapsedAngle), 0.2f).SetUpdate(isIndependentUpdate: true);
	}

	public void Expand()
	{
		base.ViewModel.Expand();
	}

	public void Collapse()
	{
		base.ViewModel.Collapse();
	}

	public void Switch()
	{
		base.ViewModel.Switch();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		if (m_HasNavigation)
		{
			m_ConsoleFocusButton.SetFocus(value);
		}
		m_ExpandButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ExpandButton.Interactable;
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel.IsSwitchable;
	}

	public void OnConfirmClick()
	{
		Switch();
	}

	public string GetConfirmClickHint()
	{
		return IsExpanded ? UIStrings.Instance.CommonTexts.Collapse : UIStrings.Instance.CommonTexts.Expand;
	}
}
