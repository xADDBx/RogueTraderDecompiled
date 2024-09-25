using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Legacy.BugReportDrawing;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Visual;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker;

public class GameMainMenu : MonoBehaviour
{
	private bool m_GraphicsSettingsApplied;

	private void Awake()
	{
		if (CrushDumpMessage.Exception == null)
		{
			return;
		}
		if (ReportingUtils.Instance.IsReportWithMods(isCrash: false))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueMods);
			});
		}
		else if (ReportingUtils.Instance.IsCorruptedBundleCrash(isCrash: false))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.ExceptionDialogueCorrupted);
			});
		}
		else if (ReportingUtils.Instance.IsDiskFreeSpaceCrash(isCrash: false))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.ExceptionDialogueFreeSpace);
			});
		}
		else
		{
			Exception exception = CrushDumpMessage.Exception;
			string dialogMessage = exception.ToString();
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(dialogMessage, DialogMessageBoxBase.BoxType.Message, delegate(DialogMessageBoxBase.BoxButton b)
				{
					if (b == DialogMessageBoxBase.BoxButton.Yes)
					{
						OnUnhandledException(exception);
					}
				}, null, UIStrings.Instance.CommonTexts.ReportButton);
			});
		}
		CrushDumpMessage.Exception = null;
	}

	private void OnUnhandledException(Exception exception)
	{
		EventBus.RaiseEvent(delegate(IBugReportDescriptionUIHandler h)
		{
			h.HandleException(exception);
		});
		UnityEngine.Object.FindObjectOfType<BugReportCanvas>().Or(null)?.OnHotKeyBugReportOpen();
	}

	private void Start()
	{
		PFLog.System.Log("MainMenu.Start()");
		if (CommandLineArguments.Parse().Contains("skipmainmenu"))
		{
			Game.Instance.SaveManager.UpdateSaveListIfNeeded();
			MainMenuUI.Instance.EnterGame(Game.Instance.LoadNewGame);
		}
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(GameScenes.MainMenu));
		GameHeapSnapshot.MainMenuSnapshot();
		if (!ReportingUtils.Instance.CheckCrashDumpFound())
		{
			return;
		}
		if (ReportingUtils.Instance.IsReportWithMods(isCrash: true))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueMods);
			});
		}
		else if (ReportingUtils.Instance.IsOutOfMemoryCrash())
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueRam);
			});
		}
		else if (ReportingUtils.Instance.IsCorruptedBundleCrash(isCrash: true))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueCorrupted);
			});
		}
		else if (ReportingUtils.Instance.IsDiskFreeSpaceCrash(isCrash: true))
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogueFreeSpace);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogue, DialogMessageBoxBase.BoxType.Dialog, OnCrashDumpReport);
			});
		}
	}

	private void OnCrashDumpReport(DialogMessageBoxBase.BoxButton buttonType)
	{
		if (buttonType == DialogMessageBoxBase.BoxButton.Yes)
		{
			UnityEngine.Object.FindObjectOfType<BugReportCanvas>().Or(null)?.OnCrashDumpReportOpen();
		}
	}

	private void Update()
	{
		SoundState.Instance.UpdateScheduledAreaMusic();
		Game.Instance.Statistic.Tick(Game.Instance, isMainMenu: true);
		if (!m_GraphicsSettingsApplied && !CommandLineArguments.Parse().Contains("menu-default-graphics"))
		{
			PFLog.System.Log("MainMenu.Update(): Applying Graphics Settings");
			m_GraphicsSettingsApplied = true;
			if ((bool)RenderingManager.Instance)
			{
				RenderingManager.Instance.ApplySettings();
			}
			PFLog.System.Log("MainMenu.Update(): Applied Graphics Settings");
		}
	}
}
