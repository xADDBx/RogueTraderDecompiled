using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities.Difficulty;

public class SettingsEntityDropdownGameDifficultyItemConsoleView : ViewBase<SettingsEntityDropdownGameDifficultyItemVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasGroup m_FocusCanvas;

	protected override void BindViewImplementation()
	{
		if (m_Icon != null)
		{
			m_Icon.sprite = base.ViewModel.Icon;
		}
		m_Title.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.IsSelected.Subscribe(SetValueFromSettings));
	}

	private void SetValueFromSettings(bool value)
	{
		m_Button.SetActiveLayer(value ? "On" : "Off");
		if (m_FocusCanvas != null)
		{
			m_FocusCanvas.alpha = (value ? 1 : 0);
		}
		if (value)
		{
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
		if (value)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(null, base.ViewModel.Title, base.ViewModel.Description);
			});
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
