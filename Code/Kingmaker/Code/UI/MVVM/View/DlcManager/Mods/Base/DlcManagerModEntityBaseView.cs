using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Base;

public class DlcManagerModEntityBaseView : SelectionGroupEntityView<DlcManagerModEntityVM>, IWidgetView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Info")]
	[SerializeField]
	private TextMeshProUGUI m_ModName;

	[SerializeField]
	private TextMeshProUGUI m_ModVersion;

	[SerializeField]
	private Image m_CommonPointMark;

	[Header("Warnings")]
	[SerializeField]
	private Image m_WarningUpdateModMark;

	[SerializeField]
	private TextMeshProUGUI m_WarningUpdateModLabel;

	[SerializeField]
	private Image m_WarningReloadGameMark;

	[SerializeField]
	private TextMeshProUGUI m_WarningReloadGameLabel;

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

	protected readonly BoolReactiveProperty IsFocused = new BoolReactiveProperty();

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_ModName.text = base.ViewModel.ModInfo.DisplayName;
		m_ModVersion.text = base.ViewModel.ModInfo.Version;
		SetupColor(isHighlighted: false);
		SetToggleTexts();
		AddDisposable(base.ViewModel.ModSwitchState.Subscribe(SetValueFromSettings));
		AddDisposable(base.ViewModel.WarningUpdateMod.Subscribe(delegate(bool value)
		{
			m_WarningUpdateModMark.gameObject.SetActive(value);
			m_WarningUpdateModLabel.transform.parent.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.WarningReloadGame.Subscribe(delegate(bool value)
		{
			m_WarningReloadGameMark.gameObject.SetActive(value);
			m_WarningReloadGameLabel.transform.parent.gameObject.SetActive(value);
			m_CommonPointMark.gameObject.SetActive(!value);
		}));
		m_MultiButton.SetInteractable(base.ViewModel.IsSaveAllowed);
		if (!base.ViewModel.IsSaveAllowed)
		{
			AddDisposable(m_MultiButton.SetHint(UIStrings.Instance.DlcManager.CannotChangeModSwitchState));
		}
		m_WarningUpdateModLabel.text = UIStrings.Instance.DlcManager.NeedToUpdateThisMod;
		m_WarningReloadGameLabel.text = UIStrings.Instance.DlcManager.ModChangedNeedToReloadGame;
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

	protected void OpenSettings()
	{
		base.ViewModel.OpenModSettings();
	}

	protected void SwitchValue()
	{
		base.ViewModel.ChangeValue();
	}

	private void SetValueFromSettings(bool value)
	{
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
		IsFocused.Value = value;
		if (value)
		{
			base.ViewModel.ShowDescription(state: true);
		}
		else
		{
			base.ViewModel.ShowDescription(state: false);
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((DlcManagerModEntityVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DlcManagerModEntityVM;
	}
}
