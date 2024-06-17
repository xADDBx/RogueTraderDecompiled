using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Difficulty;

public class SettingsEntityDropdownGameDifficultyItemPCView : ViewBase<SettingsEntityDropdownGameDifficultyItemVM>
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
		if (!base.ViewModel.IsCustom)
		{
			AddDisposable(m_Button.OnLeftClick.AsObservable().Subscribe(base.ViewModel.SetSelected));
		}
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
