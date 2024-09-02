using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.SwitchOnDlcs;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;

public class DlcManagerSwitchOnDlcEntityBaseView : SelectionGroupEntityView<DlcManagerSwitchOnDlcEntityVM>, IWidgetView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Info")]
	[SerializeField]
	private TextMeshProUGUI m_DlcName;

	[SerializeField]
	private Image m_CommonPointMark;

	[SerializeField]
	private Image m_WarningMark;

	[Header("Visual")]
	[SerializeField]
	private Image m_HighlightedImage;

	[SerializeField]
	private Color NormalColor = Color.clear;

	[SerializeField]
	private Color HighlightedColor = new Color(0.52f, 0.52f, 0.52f, 0.29f);

	[Header("Switch On Off")]
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	private readonly BoolReactiveProperty m_IsFocused = new BoolReactiveProperty();

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_DlcName.text = base.ViewModel.Title;
		SetupColor(isHighlighted: false);
		SetToggleTexts();
		AddDisposable(base.ViewModel.DlcSwitchState.Subscribe(SetValueFromSettings));
		if (!base.ViewModel.IsSaveAllowed || base.ViewModel.ItIsLateToSwitchDlcOn)
		{
			m_MultiButton.SetActiveLayer(base.ViewModel.GetActualDlcState() ? "LockedOn" : "LockedOff");
		}
		else if (base.ViewModel.GetActualDlcState())
		{
			m_MultiButton.SetActiveLayer("LockedOn");
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetupColor(bool isHighlighted)
	{
		if (m_HighlightedImage != null)
		{
			m_HighlightedImage.color = (isHighlighted ? HighlightedColor : NormalColor);
		}
	}

	private void SetToggleTexts()
	{
		m_OnText.text = UIStrings.Instance.SettingsUI.SettingsToggleOn;
		m_OffText.text = UIStrings.Instance.SettingsUI.SettingsToggleOff;
	}

	protected void SwitchValue()
	{
		base.ViewModel.ChangeValue();
	}

	private void SetValueFromSettings(bool value)
	{
		m_CommonPointMark.gameObject.SetActive(value);
		m_WarningMark.gameObject.SetActive(!value);
		m_MultiButton.SetActiveLayer(value ? "On" : "Off");
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		base.ViewModel.ShowDescription(state: true);
		SetupColor(isHighlighted: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetupColor(isHighlighted: false);
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		if (value)
		{
			base.ViewModel.SelectMe();
		}
		OnChangeSelectedStateImpl(value);
	}

	protected virtual void OnChangeSelectedStateImpl(bool value)
	{
	}

	public override void SetFocus(bool value)
	{
		SetupColor(value);
		m_IsFocused.Value = value;
		base.ViewModel.ShowDescription(value);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((DlcManagerSwitchOnDlcEntityVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DlcManagerSwitchOnDlcEntityVM;
	}
}
