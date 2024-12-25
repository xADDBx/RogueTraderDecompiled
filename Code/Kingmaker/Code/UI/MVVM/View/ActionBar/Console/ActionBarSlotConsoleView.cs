using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.Utility.Attributes;
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

	[SerializeField]
	private bool m_ShowConvertButton;

	[SerializeField]
	[ShowIf("m_ShowConvertButton")]
	private GameObject m_ConvertButton;

	private RectTransform m_RectTransform;

	private CanvasGroup m_CanvasGroup;

	public ActionBarConvertedConsoleView ConvertedView => m_ConvertedView;

	protected override void BindViewImplementation()
	{
		if ((bool)m_CanvasSortingComponent)
		{
			AddDisposable(m_CanvasSortingComponent.PushView());
		}
		if ((bool)m_ConvertedView)
		{
			AddDisposable(base.ViewModel.HasConvert.And(base.ViewModel.IsPossibleActive).Subscribe(delegate(bool value)
			{
				m_ConvertedView.gameObject.SetActive(value);
				m_ConvertButton.Or(null)?.SetActive(value);
			}));
			AddDisposable(base.ViewModel.ConvertedVm.Skip(1).Subscribe(m_ConvertedView.Bind));
			AddDisposable(base.ViewModel.HasConvert.Subscribe(delegate(bool value)
			{
				m_ConvertButton.Or(null)?.SetActive(value && m_ShowConvertButton);
			}));
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

	public void ShowConvertRequest()
	{
		if (base.ViewModel.MechanicActionBarSlot is MechanicActionBarShipWeaponSlot variantsShipWeaponSlot)
		{
			base.ViewModel.OnShowVariantsConvertRequest(variantsShipWeaponSlot);
		}
		else
		{
			base.ViewModel.OnShowConvertRequest();
		}
	}
}
