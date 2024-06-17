using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class ActionBarSlotConsoleView : ViewBase<ActionBarSlotVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	[Header("Convert Block")]
	[SerializeField]
	private ActionBarConvertedConsoleView m_ConvertedView;

	private RectTransform m_RectTransform;

	private CanvasGroup m_CanvasGroup;

	private RectTransform RectTransform => m_RectTransform.Or(null) ?? (m_RectTransform = base.transform as RectTransform);

	private CanvasGroup CanvasGroup => m_CanvasGroup.Or(null) ?? (m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>());

	protected override void BindViewImplementation()
	{
		if ((bool)m_CanvasSortingComponent)
		{
			AddDisposable(m_CanvasSortingComponent.PushView());
		}
		if ((bool)m_ConvertedView)
		{
			AddDisposable(base.ViewModel.HasConvert.And(base.ViewModel.IsPossibleActive).Subscribe(m_ConvertedView.gameObject.SetActive));
			AddDisposable(base.ViewModel.ConvertedVm.Subscribe(m_ConvertedView.Bind));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		if (value)
		{
			base.ViewModel?.OnHoverOn();
		}
		else
		{
			base.ViewModel?.OnHoverOff();
		}
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public bool CanConfirmClick()
	{
		return !base.ViewModel.IsEmpty.Value;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.OnMainClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}
}
