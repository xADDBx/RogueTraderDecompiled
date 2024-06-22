using System;
using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Base;

public class KeyBindingSetupDialogBaseView : ViewBase<KeyBindingSetupDialogVM>
{
	[SerializeField]
	private TextMeshProUGUI m_PressedKeysText;

	[SerializeField]
	private TextMeshProUGUI m_BindingIsOccupied;

	[SerializeField]
	private Color m_NormalColor = Color.white;

	[SerializeField]
	private Color m_TempColor = new Color(0.8f, 0.8f, 0.8f);

	[SerializeField]
	private Color m_OccupiedColor = Color.red;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_UnbindButton;

	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_Animator;

	private UIKeyboardTexts m_KeyboardTexts;

	private bool m_IsShowed;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_Animator.Initialize();
		m_BindingIsOccupied.text = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.HotkeyInUseErrorMessage;
	}

	protected override void BindViewImplementation()
	{
		Show();
		m_KeyboardTexts = UIStrings.Instance.KeyboardTexts;
		Coroutine bindingRoutine = StartCoroutine(BindingRoutine());
		AddDisposable(Disposable.Create(delegate
		{
			StopCoroutine(bindingRoutine);
		}));
		Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening = true;
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close));
		AddDisposable(m_UnbindButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Unbind));
		m_BindingIsOccupied.gameObject.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
		Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening = false;
		Hide();
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			m_Animator.AppearAnimation();
			UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		}
	}

	public void Hide()
	{
		if (m_IsShowed)
		{
			UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
			m_Animator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				m_IsShowed = false;
			});
		}
	}

	private IEnumerator BindingRoutine()
	{
		while (true)
		{
			yield return null;
			if (!Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening)
			{
				yield break;
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				break;
			}
			KeyBindingData keyBindingData;
			bool validBinding = GetValidBinding(out keyBindingData);
			DisplayPressedKeys();
			if (validBinding)
			{
				base.ViewModel.OnBindingChosen(keyBindingData);
			}
		}
		base.ViewModel.Close();
	}

	private void DisplayPressedKeys()
	{
		KeyBindingData keyBindingData = GetTempBinding();
		m_BindingIsOccupied.gameObject.SetActive(value: false);
		bool flag = true;
		if (keyBindingData.Key == KeyCode.None && !keyBindingData.IsCtrlDown && !keyBindingData.IsAltDown && !keyBindingData.IsShiftDown)
		{
			flag = false;
			keyBindingData = base.ViewModel.CurrentKeyBinding;
			if (keyBindingData.Key == KeyCode.None)
			{
				m_PressedKeysText.text = string.Empty;
				return;
			}
		}
		m_PressedKeysText.color = (flag ? m_TempColor : (base.ViewModel.CurrentBindingIsOccupied ? m_OccupiedColor : m_NormalColor));
		m_PressedKeysText.text = keyBindingData.GetPrettyString();
		if (base.ViewModel.CurrentBindingIsOccupied)
		{
			m_BindingIsOccupied.gameObject.SetActive(value: true);
		}
	}

	private KeyBindingData GetTempBinding()
	{
		KeyBindingData result = default(KeyBindingData);
		if (Input.anyKey && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
		{
			result.IsCtrlDown = KeyboardAccess.IsCtrlHold();
			result.IsAltDown = KeyboardAccess.IsAltHold();
			result.IsShiftDown = KeyboardAccess.IsShiftHold();
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKey(value) && (uint)(value - 303) > 5u)
				{
					result.Key = value;
					break;
				}
			}
		}
		return result;
	}

	private bool GetValidBinding(out KeyBindingData keyBindingData)
	{
		keyBindingData = new KeyBindingData
		{
			Key = KeyCode.None
		};
		if (CommandKeyDown())
		{
			return false;
		}
		if (Input.anyKeyDown && !CommandKeyDown() && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
		{
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKeyDown(value))
				{
					keyBindingData.Key = value;
					keyBindingData.IsCtrlDown = KeyboardAccess.IsCtrlHold();
					keyBindingData.IsAltDown = KeyboardAccess.IsAltHold();
					keyBindingData.IsShiftDown = KeyboardAccess.IsShiftHold();
					return true;
				}
			}
		}
		return GetValidBindingImpl(out keyBindingData);
	}

	protected virtual bool GetValidBindingImpl(out KeyBindingData keyBindingData)
	{
		keyBindingData = new KeyBindingData
		{
			Key = KeyCode.None
		};
		return false;
	}

	private bool CommandKeyDown()
	{
		if (!KeyboardAccess.IsAltDown() && !KeyboardAccess.IsCtrlDown())
		{
			return KeyboardAccess.IsShiftDown();
		}
		return true;
	}

	protected bool CommandKeyUp()
	{
		if (!KeyboardAccess.IsAltUp() && !KeyboardAccess.IsCtrlUp())
		{
			return KeyboardAccess.IsShiftUp();
		}
		return true;
	}

	protected bool CommandKeyHold()
	{
		if (!KeyboardAccess.IsAltHold() && !KeyboardAccess.IsCtrlHold())
		{
			return KeyboardAccess.IsShiftHold();
		}
		return true;
	}
}
