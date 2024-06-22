using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Menu;

public class SettingsMenuEntityConsoleView : ViewBase<SettingsMenuEntityVM>, IConsoleNavigationEntity, IConsoleEntity, IWidgetView
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[Header("Scale Settings")]
	[SerializeField]
	private float m_MaxWidth = 225f;

	[SerializeField]
	private float m_SetScaleWidth = 0.9f;

	[SerializeField]
	private float m_SetCharacterSpacing = -25f;

	public UISettingsManager.SettingsScreen SettingsScreenType => base.ViewModel.SettingsScreenType;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Title.Subscribe(SetText));
		AddDisposable(base.ViewModel.IsSelected.Subscribe(SetFocus));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetText(string text)
	{
		m_Title.text = text;
		DelayedInvoker.InvokeInFrames(delegate
		{
			RectTransform rectTransform = m_Title.transform as RectTransform;
			if (!(rectTransform == null))
			{
				bool flag = rectTransform.sizeDelta.x <= m_MaxWidth;
				rectTransform.localScale = new Vector3(flag ? 1f : m_SetScaleWidth, 1f, 1f);
				m_Title.characterSpacing = (flag ? 0f : m_SetCharacterSpacing);
			}
		}, 1);
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		return true;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((SettingsMenuEntityVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is SettingsMenuEntityVM;
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.Confirm();
	}
}
