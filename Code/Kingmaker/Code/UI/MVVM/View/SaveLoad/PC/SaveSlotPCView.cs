using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.PC;

public class SaveSlotPCView : SaveSlotBaseView
{
	[Header("Buttons")]
	[SerializeField]
	private TextMeshProUGUI m_SaveLoadLabel;

	[SerializeField]
	private OwlcatButton m_SaveLoadButton;

	[SerializeField]
	private TextMeshProUGUI m_DeleteLabel;

	[SerializeField]
	private OwlcatButton m_DeleteButton;

	private IDisposable m_SaveLoadButtonHintDisposable;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_SaveLoadButton != null)
		{
			AddDisposable(m_SaveLoadButton.Or(null)?.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.SaveOrLoad();
			}));
			AddDisposable(m_DeleteButton.Or(null)?.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.Delete();
			}));
			AddDisposable(base.ViewModel.Mode.Subscribe(SetSaveLoadButton));
			if (m_DeleteLabel != null)
			{
				m_DeleteLabel.text = base.Texts?.DeleteLabel;
			}
			SetSaveLoadButton(base.ViewModel.Mode.Value);
		}
		AddDisposable(base.ViewModel.IsCurrentIronManSave.Subscribe(delegate
		{
			SetInteractableSaveLoadButton();
		}));
		SetInteractableSaveLoadButton();
		m_DeleteButton.Or(null)?.SetInteractable(state: true);
		if (!m_IsDetailedView && !(this is NewSaveSlotPCView))
		{
			AddDisposable(m_Button.OnLeftDoubleClickAsObservable().Subscribe(delegate
			{
				if (base.ViewModel.ShowSaveLoadButton)
				{
					base.ViewModel.SaveOrLoad();
				}
			}));
		}
		AddDisposable(m_ScreenshotImage.OnPointerEnterAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowScreenshot();
		}));
		AddDisposable(m_ScreenshotImage.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel.HideScreenshot();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_SaveLoadButtonHintDisposable?.Dispose();
		m_SaveLoadButtonHintDisposable = null;
		m_SaveLoadButton.Or(null)?.SetInteractable(state: false);
		m_DeleteButton.Or(null)?.SetInteractable(state: false);
		if (m_SaveLoadLabel != null)
		{
			m_SaveLoadLabel.text = string.Empty;
		}
		if (m_DeleteLabel != null)
		{
			m_DeleteLabel.text = string.Empty;
		}
		base.DestroyViewImplementation();
	}

	private void SetSaveLoadButton(SaveLoadMode mode)
	{
		if (m_SaveLoadLabel != null)
		{
			m_SaveLoadLabel.text = ((mode == SaveLoadMode.Load) ? base.Texts.LoadLabel : base.Texts.SaveLabel);
		}
		m_SaveLoadButton.Or(null)?.gameObject.SetActive(base.ViewModel.ShowSaveLoadButton);
	}

	protected override void UpdateDLCState(bool b)
	{
		base.UpdateDLCState(b);
		SetInteractableSaveLoadButton();
	}

	private void SetInteractableSaveLoadButton()
	{
		m_SaveLoadButton.Or(null)?.SetInteractable((!base.ViewModel.ShowDlcRequiredLabel.Value || base.ViewModel.Mode.Value == SaveLoadMode.Save) && !base.ViewModel.IsCurrentIronManSave.Value);
		m_SaveLoadButtonHintDisposable?.Dispose();
		if (base.ViewModel.IsCurrentIronManSave.Value)
		{
			m_SaveLoadButtonHintDisposable = m_SaveLoadButton.SetHint(UIStrings.Instance.SaveLoadTexts.CannotLoadCurrentIronManSave);
		}
	}
}
