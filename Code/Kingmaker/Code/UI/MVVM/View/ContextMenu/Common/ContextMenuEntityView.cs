using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;

public abstract class ContextMenuEntityView : ViewBase<ContextMenuEntityVM>, IConfirmClickHandler, IConsoleEntity
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	protected Image m_Icon;

	[SerializeReference]
	protected ContextButtonFx m_ButtonFx;

	protected override void BindViewImplementation()
	{
		if (m_ButtonFx != null)
		{
			AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
			{
				if (value)
				{
					DelayedInvoker.InvokeInFrames(delegate
					{
						m_ButtonFx.DoBlink();
					}, 1);
				}
			}));
		}
		m_Button.SetActiveLayer(base.ViewModel.IsInteractable ? "Unlocked" : "Locked");
		UISounds.Instance.SetClickSound(m_Button, base.ViewModel.GetClickSoundType());
		UISounds.Instance.SetHoverSound(m_Button, base.ViewModel.GetHoverSoundType());
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(m_Button.SetInteractable));
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string t)
		{
			m_Label.text = t;
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			Execute();
		}));
		if (m_Icon != null)
		{
			if (base.ViewModel.Sprite != null)
			{
				m_Icon.gameObject.SetActive(value: true);
				m_Icon.sprite = base.ViewModel.Sprite;
			}
			else
			{
				m_Icon.gameObject.SetActive(value: false);
			}
		}
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void Execute()
	{
		base.ViewModel.Execute();
		ContextMenuHelper.HideContextMenu();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.ViewModel != null)
		{
			return base.ViewModel.IsEnabled.Value;
		}
		return false;
	}

	public bool CanConfirmClick()
	{
		return m_Button.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		UISounds.Instance.PlayButtonClickSound((int)base.ViewModel.GetClickSoundType());
		base.ViewModel.Execute();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
