using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA.Analytics;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.InputSystems.Enums;
using Kingmaker.Utility;
using Kingmaker.Utility.Reporting.Base;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace Kingmaker.Code.UI.Legacy.BugReportDrawing;

public class BugReportCanvas : MonoBehaviour, IBugReportUIHandler, ISubscriber, IAreaHandler
{
	public TextMeshProUGUI StatusTextMeshPro;

	private int m_SetStatusTmpAttempts;

	private ReportingRaycaster m_ReportingRaycaster;

	private bool m_State;

	private bool m_IsShowReportButton;

	private bool m_IsCrashDumpReport;

	private bool m_IsSendingReportInProgress;

	private float m_SendingReportAfterTimer;

	[SerializeField]
	private TextMeshProUGUI WatermarkText;

	[SerializeField]
	private CanvasGroup m_BugreportCanvasGroup;

	private const string BugReportPlayerPref = "BugReportMessageWasShown";

	public static bool IsBugReportVisible { get; private set; }

	private bool IsShowReportButton
	{
		get
		{
			return m_IsShowReportButton;
		}
		set
		{
			if (m_IsShowReportButton != value)
			{
				m_IsShowReportButton = value;
				m_BugreportCanvasGroup.alpha = (value ? 1f : 0f);
				m_BugreportCanvasGroup.blocksRaycasts = value;
				if (!value && m_State)
				{
					Show(state: false);
				}
			}
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	public void BindKeyboard(KeyboardAccess keyboardAccess)
	{
		if (keyboardAccess.CanBeRegistered("RapidBugReportWindowOpen", KeyCode.B, GameModesGroup.All, ctrl: false, alt: true, shift: false))
		{
			GameModeType[] gameModesArray = KeyboardAccess.GetGameModesArray(GameModesGroup.All);
			keyboardAccess.RegisterBinding("RapidBugReportWindowOpen", KeyCode.B, gameModesArray, ctrl: false, alt: true, shift: false);
			keyboardAccess.RegisterBinding("ClipboardCopyBuildInfo", KeyCode.C, gameModesArray, ctrl: true, alt: true, shift: true);
		}
		keyboardAccess.Bind("RapidBugReportWindowOpen", OnHotKeyBugReportOpen);
		keyboardAccess.Bind("ClipboardCopyBuildInfo", ClipboardCopyBuildInfoHotkey);
	}

	public void OnHotKeyBugReportOpen()
	{
		m_IsCrashDumpReport = false;
		IsShowReportButton = true;
		HandleBugReportOpen();
	}

	public void OnCrashDumpReportOpen()
	{
		m_IsCrashDumpReport = true;
		IsShowReportButton = true;
		OwlcatAnalytics.Instance.SendCrashEvent();
		Show(state: true);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		Game.Instance.Keyboard.Unbind("RapidBugReportWindowOpen", OnHotKeyBugReportOpen);
		EventBus.Unsubscribe(this);
	}

	public void HandleBugReportOpen(bool showBugReportOnly = false)
	{
		if (showBugReportOnly)
		{
			Show(state: true);
			return;
		}
		Game.Instance.RequestPauseUi(isPaused: true);
		if (PlayerPrefs.GetInt("BugReportMessageWasShown") == 1)
		{
			Show(state: true);
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(Game.Instance.IsControllerMouse ? UIStrings.Instance.UIBugReport.BugReportStartingMessagePC.Text : UIStrings.Instance.UIBugReport.BugReportStartingMessageConsole.Text, DialogMessageBoxBase.BoxType.Message, delegate(DialogMessageBoxBase.BoxButton value)
			{
				PlayerPrefs.SetInt("BugReportMessageWasShown", 1);
				PlayerPrefs.Save();
				Game.Instance.RequestPauseUi(isPaused: false);
				Show(value == DialogMessageBoxBase.BoxButton.Yes);
			}, null, UIStrings.Instance.UIBugReport.BugReportContinue.Text);
		});
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
		OnHotKeyBugReportOpen();
	}

	public void HandleBugReportShow()
	{
	}

	public void HandleBugReportHide()
	{
		Show(state: false);
	}

	public void HandleUIElementFeature(string featureName)
	{
	}

	private void Show(bool state)
	{
		if (m_State != state)
		{
			m_State = state;
			IsBugReportVisible = state;
			if (state)
			{
				StartCoroutine(ShowReportWindow());
			}
			else
			{
				HideReportWindow();
			}
		}
	}

	private IEnumerator ShowReportWindow()
	{
		if (m_ReportingRaycaster == null)
		{
			m_ReportingRaycaster = base.gameObject.AddComponent<ReportingRaycaster>();
		}
		try
		{
			EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
			{
				h.HandleUIElementFeature(m_ReportingRaycaster.GetFeatureName());
			});
		}
		catch
		{
		}
		if (m_IsCrashDumpReport)
		{
			m_IsCrashDumpReport = false;
			yield return ReportingUtils.Instance.MakeNewReport(makeScreenshot: false, makeSave: false, addCrashDump: true);
		}
		else
		{
			yield return ReportingUtils.Instance.MakeNewReport(makeScreenshot: true, makeSave: true, addCrashDump: false);
		}
		yield return null;
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportShow();
		});
		yield return null;
	}

	private void HideReportWindow()
	{
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
	}

	public void OnAreaBeginUnloading()
	{
		IsShowReportButton = false;
	}

	public void OnAreaDidLoad()
	{
		HideReportWindow();
		IsShowReportButton = true;
		try
		{
			string text = string.Empty;
			if (Application.isPlaying)
			{
				text = Game.Instance?.CurrentlyLoadedArea.NameSafe();
			}
			CrashReportHandler.SetUserMetadata("CurrentArea", text ?? "");
		}
		catch
		{
		}
	}

	private void Start()
	{
		if (m_ReportingRaycaster == null)
		{
			m_ReportingRaycaster = base.gameObject.AddComponent<ReportingRaycaster>();
		}
		Show(state: false);
		m_BugreportCanvasGroup.alpha = 0f;
		m_BugreportCanvasGroup.blocksRaycasts = false;
		WatermarkText.text = GameVersion.GetDisplayedVersion();
		WatermarkText.gameObject.SetActive(!string.IsNullOrEmpty(WatermarkText.text));
		try
		{
			KeyboardAccess keyboard = Game.Instance.Keyboard;
			BindKeyboard(keyboard);
		}
		catch
		{
		}
	}

	private void Update()
	{
		IsShowReportButton = true;
		if (StatusTextMeshPro == null && m_SetStatusTmpAttempts < 10)
		{
			m_SetStatusTmpAttempts++;
			try
			{
				GameObject gameObject = Object.Instantiate(base.gameObject.transform.Children().FirstOrDefault((Transform x) => x.gameObject.name == "GameVersion")?.gameObject, base.gameObject.transform);
				gameObject.name = "ReportSendStatusText";
				RectTransform component = gameObject.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(1f, 1f);
				component.anchorMax = new Vector2(1f, 1f);
				component.pivot = new Vector2(0f, 0f);
				component.anchoredPosition = new Vector3(-365f, -80f, 0f);
				component.sizeDelta = new Vector2(165f, 30f);
				StatusTextMeshPro = gameObject.gameObject.transform.Children().FirstOrDefault((Transform x) => x.GetComponent<TextMeshProUGUI>())?.GetComponent<TextMeshProUGUI>();
				StatusTextMeshPro.text = string.Empty;
			}
			catch
			{
				GameObject gameObject2 = GameObject.Find("ReportSendStatusText");
				if (gameObject2 != null)
				{
					Object.Destroy(gameObject2);
				}
			}
		}
		if (!m_IsSendingReportInProgress && StatusTextMeshPro != null)
		{
			if (m_SendingReportAfterTimer > 0f)
			{
				m_SendingReportAfterTimer -= Time.deltaTime;
				return;
			}
			StatusTextMeshPro.text = string.Empty;
			m_SendingReportAfterTimer = 10f;
		}
	}

	private static void ClipboardCopyBuildInfoHotkey()
	{
		GUIUtility.systemCopyBuffer = ReportVersionManager.GetBuildInfo();
	}
}
