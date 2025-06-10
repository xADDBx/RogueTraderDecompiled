using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Legacy.BugReportDrawing;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.View.Common.InputField;
using Kingmaker.Code.UI.MVVM.VM.BugReport;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.BugReport;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.BugReport;

public abstract class BugReportBaseView : ViewBase<BugReportVM>
{
	[Header("Localizations")]
	[SerializeField]
	private TextMeshProUGUI m_MainTitleText;

	[SerializeField]
	private TextMeshProUGUI m_AspectTitleText;

	[SerializeField]
	private TextMeshProUGUI m_ContextTitleText;

	[SerializeField]
	private TextMeshProUGUI m_SendButtonText;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	[SerializeField]
	private TextMeshProUGUI m_SuggestionToggleText;

	[SerializeField]
	private TextMeshProUGUI m_NormalToggleText;

	[SerializeField]
	private TextMeshProUGUI m_CriticalToggleText;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionTitleText;

	[SerializeField]
	private TextMeshProUGUI m_EmailTitleText;

	[SerializeField]
	private TextMeshProUGUI m_BottomDescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_PrivacyDescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_DiscordTitleText;

	[SerializeField]
	private TextMeshProUGUI m_EmailUpdatesDescriptionText;

	[Header("Other")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private OwlcatMultiButton m_DrawingButton;

	[SerializeField]
	private OwlcatMultiButton m_DuplicatesButton;

	[SerializeField]
	private OwlcatInputField m_MessageInputField;

	[SerializeField]
	private GameObject m_EmailGroup;

	[SerializeField]
	private OwlcatInputField m_EmailInputField;

	[SerializeField]
	private OwlcatInputField m_DiscordInputField;

	[SerializeField]
	private GameObject m_DiscordGameObject;

	[SerializeField]
	protected OwlcatDropdown m_ContextDropdown;

	[SerializeField]
	private OwlcatDropdown m_AspectDropdown;

	[SerializeField]
	private GameObject m_AssigneeGO;

	[SerializeField]
	private OwlcatDropdown m_AssigneeDropdown;

	[SerializeField]
	private GameObject m_ManualSaveGroup;

	[SerializeField]
	private GameObject m_DevPriorityGroup;

	[SerializeField]
	private OwlcatDropdown m_ManualSaveDropdown;

	[SerializeField]
	private OwlcatDropdown m_DevPriorityDropdown;

	[SerializeField]
	private GameObject m_FixVersionGO;

	[SerializeField]
	private OwlcatDropdown m_FixVersionDropdown;

	[SerializeField]
	private OwlcatMultiButton m_SendButton;

	[Header("Issue Type")]
	[SerializeField]
	private OwlcatToggleGroup m_IssueTypeToggleGroup;

	[SerializeField]
	private OwlcatToggle m_CriticalToggle;

	[SerializeField]
	private OwlcatToggle m_NormalToggle;

	[SerializeField]
	private OwlcatToggle m_SuggestionToggle;

	[Header("Feedback")]
	[SerializeField]
	private OwlcatToggle m_FeedbackToggle;

	[Header("Labels")]
	[SerializeField]
	private GameObject m_LabelsGroup;

	[SerializeField]
	private OwlcatMultiButton m_LabelsButton;

	[SerializeField]
	private TextMeshProUGUI m_LabelsButtonText;

	[SerializeField]
	[FormerlySerializedAs("m_LabelTogglePrefab")]
	private GameObject m_LabelsPrefab;

	[SerializeField]
	[FormerlySerializedAs("m_LabelsBackgroundRectTransform")]
	private RectTransform m_LabelsList;

	[SerializeField]
	private RectTransform m_LabelsListContainer;

	[SerializeField]
	private Button m_LabelsBlocker;

	[Header("Privacy and Email")]
	[SerializeField]
	protected OwlcatToggle m_PrivacyToggle;

	[SerializeField]
	private OwlcatToggle m_EmailUpdatesToggle;

	[SerializeField]
	private GameObject m_EmailUpdatesGroup;

	[Header("Drawing")]
	[SerializeField]
	protected BugReportDrawingView m_BugReportDrawingView;

	[Header("Duplicates")]
	[SerializeField]
	protected BugReportDuplicatesBaseView m_BugReportDuplicatesBaseView;

	private string m_PrevEmail;

	private string m_PrevDiscord;

	private bool m_IsLabelsShow;

	private List<GameObject> m_LabelsListItems = new List<GameObject>();

	private List<IDisposable> m_LabelsDisposable = new List<IDisposable>();

	protected InputLayer m_InputLayer;

	public static string InputLayerContextName = "BugReportInput";

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private Dictionary<OwlcatToggle, ReportingUtils.Severity> m_IssueTypes;

	private IDisposable m_FixVersionDropdownDisposable;

	private IDisposable m_AssigneeDropdownDisposable;

	private IDisposable m_ManualSaveDropdownDisposable;

	public static string LabelsDisposableString => "m_LabelsDisposable";

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_IssueTypes = new Dictionary<OwlcatToggle, ReportingUtils.Severity>
		{
			{
				m_CriticalToggle,
				ReportingUtils.Severity.Critical
			},
			{
				m_NormalToggle,
				ReportingUtils.Severity.Normal
			},
			{
				m_SuggestionToggle,
				ReportingUtils.Severity.Suggestion
			}
		};
		m_MessageInputField.SetMaxTextLength(BuildModeUtility.IsDevelopment ? uint.MaxValue : 1000u);
		m_BugReportDrawingView.gameObject.SetActive(value: false);
		m_BugReportDuplicatesBaseView.gameObject.SetActive(value: false);
		m_LabelsBlocker.onClick.AddListener(OnLabelsShow);
	}

	protected override void BindViewImplementation()
	{
		Show(state: true);
		AddDisposable(base.ViewModel.BugReportDrawingVM.Subscribe(m_BugReportDrawingView.Bind));
		AddDisposable(base.ViewModel.BugReportDuplicatesVM.Subscribe(m_BugReportDuplicatesBaseView.Bind));
		AddDisposable(m_FeedbackToggle.IsOn.Subscribe(OnFeedbackToggleValueChanged));
		AddDisposable(m_PrivacyToggle.IsOn.Subscribe(m_SendButton.SetInteractable));
		AddDisposable(m_ContextDropdown.Index.Subscribe(delegate
		{
			OnContextDropdownValueChanged();
		}));
		AddDisposable(m_AspectDropdown.Index.Subscribe(delegate
		{
			OnAspectDropdownValueChanged();
		}));
		m_MessageInputField.SetPlaceholderText(UIStrings.Instance.UIBugReport.AdditionalPlaceholderText.Text);
		if (BuildModeUtility.IsDevelopment)
		{
			m_MessageInputField.Text = "\n\nШаги воспроизведения:\n\n\nФактический результат:\n\n\nОжидаемый результат:\n";
			m_BottomDescriptionText.text = "<b>Blocker:</b> ошибка, физически блокирующая разработку;\n<b>Crit:</b> ошибка, блокирующая выпуск версии;\n<b>Major:</b> ошибка, близкая к криту по нужности в версию, но не блокирующая ее выпуск;\n<b>Normal:</b> остальные ошибки, помимо критических;\n<b>Minor:</b> ошибка не влияющая на выпуск версии.";
		}
		BuildNavigation();
		m_DuplicatesButton.gameObject.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
		Show(state: false);
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
		DisposeDropdowns();
	}

	private void SetupDropdowns()
	{
		if (m_FixVersionDropdownDisposable != null || m_AssigneeDropdownDisposable != null || m_ManualSaveDropdownDisposable != null)
		{
			DisposeDropdowns();
		}
		if (!BuildModeUtility.IsDevelopment || !ReportingUtils.Instance.Assignees.IsCompletedSuccessfully)
		{
			return;
		}
		m_FixVersionDropdownDisposable = m_FixVersionDropdown.Index.Subscribe(delegate
		{
			OnFixVersionDropdownValueChanged();
		});
		m_AssigneeDropdownDisposable = m_AssigneeDropdown.Index.Subscribe(delegate
		{
			OnAssigneeDropdownValueChanged();
		});
		if (m_ManualSaveDropdown != null)
		{
			m_ManualSaveDropdownDisposable = m_ManualSaveDropdown.Index.Subscribe(delegate
			{
				OnManualSaveDropdownValueChanged();
			});
		}
	}

	private void DisposeDropdowns()
	{
		m_FixVersionDropdownDisposable?.Dispose();
		m_FixVersionDropdownDisposable = null;
		m_AssigneeDropdownDisposable?.Dispose();
		m_AssigneeDropdownDisposable = null;
		m_ManualSaveDropdownDisposable?.Dispose();
		m_ManualSaveDropdownDisposable = null;
	}

	private void BuildNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		BuildNavigationImpl(m_NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		CreateInputImpl(m_InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		List<IConsoleEntity> entities = new List<IConsoleEntity> { m_ContextDropdown, m_AspectDropdown };
		m_NavigationBehaviour.AddRow(entities);
		List<IConsoleEntity> entities2 = new List<IConsoleEntity> { m_EmailInputField, m_DiscordInputField };
		m_NavigationBehaviour.AddRow(entities2);
		List<IConsoleEntity> list = new List<IConsoleEntity> { m_LabelsButton, m_FixVersionDropdown, m_AssigneeDropdown };
		if (m_ManualSaveDropdown != null)
		{
			list.Add(m_ManualSaveDropdown);
		}
		m_NavigationBehaviour.AddRow(list);
		m_NavigationBehaviour.AddEntityVertical(m_MessageInputField);
		List<IConsoleEntity> list2 = new List<IConsoleEntity> { m_CriticalToggle, m_NormalToggle, m_SuggestionToggle, m_FeedbackToggle };
		if (m_DevPriorityDropdown != null)
		{
			list2.Add(m_DevPriorityDropdown);
		}
		m_NavigationBehaviour.AddRow(list2);
		m_NavigationBehaviour.AddEntityVertical(m_PrivacyToggle);
		m_NavigationBehaviour.AddEntityVertical(m_EmailUpdatesToggle);
		m_NavigationBehaviour.AddEntityVertical(m_SendButton);
		List<IConsoleNavigationEntity> entities3 = new List<IConsoleNavigationEntity> { m_CloseButton, m_DrawingButton, m_DuplicatesButton };
		m_NavigationBehaviour.AddColumn(entities3);
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_CloseButton.OnConfirmClickAsObservable().Subscribe(OnClose));
		AddDisposable(m_DrawingButton.OnConfirmClickAsObservable().Subscribe(OnShowDrawing));
		AddDisposable(m_DuplicatesButton.OnConfirmClickAsObservable().Subscribe(OnShowDuplicates));
		AddDisposable(m_SendButton.OnConfirmClickAsObservable().Subscribe(OnSend));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(OnClose));
		AddDisposable(m_DrawingButton.OnLeftClickAsObservable().Subscribe(OnShowDrawing));
		AddDisposable(m_SendButton.OnLeftClickAsObservable().Subscribe(OnSend));
		AddDisposable(m_DuplicatesButton.OnLeftClickAsObservable().Subscribe(OnShowDuplicates));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(OnClose));
		AddDisposable(m_LabelsButton.OnConfirmClickAsObservable().Subscribe(OnLabelsShow));
		AddDisposable(m_LabelsButton.OnLeftClickAsObservable().Subscribe(OnLabelsShow));
	}

	protected void OnShowDrawing()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		base.ViewModel.ShowDrawing();
	}

	protected void OnShowDuplicates()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		base.ViewModel.ShowDuplicates();
	}

	private void StoreUserData()
	{
		ReportingUtils.Instance.StoreEmail(m_EmailInputField.Text);
		ReportingUtils.Instance.StoreDiscord(m_DiscordInputField.Text);
		if (BuildModeUtility.IsDevelopment && ReportingUtils.Instance.Assignees.IsCompletedSuccessfully)
		{
			ReportingUtils.Instance.StoreFixVersion(m_FixVersionDropdown.Index.Value);
		}
	}

	private void RestoreUserData(bool restoreDevFields)
	{
		m_EmailInputField.Text = ReportingUtils.Instance.RestoreEmail();
		m_DiscordInputField.Text = ReportingUtils.Instance.RestoreDiscord();
		if (restoreDevFields)
		{
			RestoreDevFields();
		}
		(bool, bool) tuple = ReportingUtils.Instance.PrivacyStuffGetEmailAgreements(m_EmailInputField.Text);
		m_PrivacyToggle.Set(tuple.Item2);
		m_EmailUpdatesToggle.Set(tuple.Item1 && IsEmailMatchRegexp());
		void RestoreDevFields()
		{
			if (m_FixVersionDropdown == null)
			{
				ReportingUtils.Logger.Error("Can't show FixVersionDropDown because m_FixVersionDropdown is null");
			}
			else if (m_FixVersionDropdown.Index == null)
			{
				ReportingUtils.Logger.Error("Can't show FixVersionDropDown because m_FixVersionDropdown.Index is null");
			}
			else
			{
				m_FixVersionDropdown.SetIndex(ReportingUtils.Instance.RestoreFixVersion());
			}
		}
	}

	public void OnSend()
	{
		if (!m_PrivacyToggle.IsOn.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(BlueprintRoot.Instance.LocalizedTexts.UserInterfacesText.UIBugReport.SendindIsNotAvailable, addToLog: false, WarningNotificationFormat.Attention);
			});
			return;
		}
		string issueType = GetIssueType();
		ReportingUtils.Instance.SendReport(m_MessageInputField.Text, m_EmailInputField.Text, SystemInfo.deviceUniqueIdentifier, issueType, m_DiscordInputField.Text, m_EmailUpdatesToggle.IsOn.Value);
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(BlueprintRoot.Instance.LocalizedTexts.UserInterfacesText.CommonTexts.WarningBugReportWasSend);
		});
		m_MessageInputField.Text = "";
		StoreUserData();
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
		ReportingUtils.Instance.PrivacyStuffManage(m_EmailInputField.Text, m_EmailUpdatesToggle.IsOn.Value, m_PrivacyToggle.IsOn.Value, isSend: true);
		ReportingUtils.Instance.Clear();
		InputLog.SetLogInput(state: false);
	}

	public void OnClose()
	{
		StoreUserData();
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
		ReportingUtils.Instance.PrivacyStuffManage(m_EmailInputField.Text, m_EmailUpdatesToggle.IsOn.Value, m_PrivacyToggle.IsOn.Value, isSend: false);
		ReportingUtils.Instance.Clear();
	}

	public void Show(bool state)
	{
		SetTranslationText();
		m_DescriptionTitleText.text = UIStrings.Instance.UIBugReport.DesctiptionHeader;
		base.gameObject.SetActive(state);
		Game.Instance.RequestPauseUi(state);
		if (state)
		{
			OnShow();
		}
	}

	private void OnShow()
	{
		(m_IssueTypes.FirstOrDefault((KeyValuePair<OwlcatToggle, ReportingUtils.Severity> kvp) => kvp.Value == ReportingUtils.Instance.GetSuggestedSeverity()).Key ?? m_NormalToggle).Set(value: true);
		m_IsLabelsShow = false;
		HideLabelsButton();
		m_ContextDropdown.Bind(base.ViewModel.ContextDropdownVM);
		m_AspectDropdown.Bind(base.ViewModel.AspectDropdownVM);
		bool flag = BuildModeUtility.IsDevelopment && ReportingUtils.Instance.Assignees.IsCompletedSuccessfully;
		if (flag)
		{
			try
			{
				m_EmailGroup?.SetActive(value: true);
				m_AssigneeGO.gameObject.SetActive(value: true);
				m_AssigneeDropdown.Bind(base.ViewModel.GetAssigneeDropDownVM(m_ContextDropdown.Index.Value));
				m_FixVersionGO.SetActive(value: true);
				m_FixVersionDropdown.Bind(base.ViewModel.GetFixVersionDropDownVM());
				m_FixVersionDropdown.SetInteractable(ReportingUtils.Instance.CanSelectFixVersion());
				m_ContextDropdown.SetInteractable(ReportingUtils.Instance.CanSelectContext());
				if (m_ManualSaveGroup != null && m_ManualSaveDropdown != null)
				{
					m_ManualSaveGroup.SetActive(value: true);
					m_ManualSaveDropdown.Bind(base.ViewModel.GetManualSaveDropDownVM());
				}
				if (m_DevPriorityGroup != null && m_DevPriorityDropdown != null)
				{
					m_DevPriorityGroup.SetActive(value: true);
					m_CriticalToggle.gameObject.SetActive(value: false);
					m_NormalToggle.gameObject.SetActive(value: false);
					m_SuggestionToggle.gameObject.SetActive(value: false);
					m_DevPriorityDropdown.Bind(base.ViewModel.GetDevPriorityDropDownVM());
					m_DevPriorityDropdown.SetIndex(base.ViewModel.GetDefaultDevPriorityIndex());
				}
				SetupDropdowns();
				m_LabelsGroup.SetActive(value: true);
				ReportingUtils.Instance.ResetLabelsList();
				ReportingUtils.Instance.FillLabelsDictionary();
				LabelsButtonChangeText();
			}
			catch
			{
				m_AssigneeGO.gameObject.SetActive(value: false);
				m_FixVersionGO.SetActive(value: false);
				m_LabelsGroup.SetActive(value: false);
			}
		}
		else
		{
			m_AssigneeGO.gameObject.SetActive(value: false);
			m_FixVersionGO.SetActive(value: false);
			m_LabelsGroup.SetActive(value: false);
		}
		m_FeedbackToggle.gameObject.SetActive(flag);
		ToggleAdditionalContactsVisibility(IsEmailMatchRegexp());
		ExpandDescriptionOverMarket();
		RestoreUserData(flag);
	}

	public void OnLabelsShow()
	{
		if (m_IsLabelsShow)
		{
			HideLabelsButton();
		}
		else
		{
			CreateLabelsObjectList();
		}
		m_IsLabelsShow = !m_IsLabelsShow;
	}

	public void OnLabelSelected(string label, bool b)
	{
		ReportingUtils.Instance.LabelChangeValue(label, b);
		LabelsButtonChangeText();
	}

	private void LabelsButtonChangeText()
	{
		int num = 0;
		Dictionary<string, bool> labelsList = ReportingUtils.Instance.GetLabelsList();
		foreach (bool value in labelsList.Values)
		{
			if (value)
			{
				num++;
			}
		}
		m_LabelsButtonText.text = $"{num}/{labelsList.Count} labels selected";
	}

	private void CreateLabelsObjectList()
	{
		Dictionary<string, bool> labelsList = ReportingUtils.Instance.GetLabelsList();
		if (labelsList == null || labelsList.Count == 0)
		{
			return;
		}
		m_LabelsList.gameObject.SetActive(value: true);
		m_LabelsListItems = new List<GameObject>();
		InputLayer inputLayer = new InputLayer
		{
			ContextName = "m_LabelsDisposable"
		};
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		foreach (KeyValuePair<string, bool> item in labelsList)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_LabelsPrefab, m_LabelsListContainer);
			gameObject.SetActive(value: true);
			m_LabelsListItems.Add(gameObject);
			gridConsoleNavigationBehaviour.AddEntityVertical(gameObject.GetComponent<IConsoleEntity>());
			gameObject.GetComponent<BugReportLabelToggle>().Initiate(OnLabelSelected, item.Key, item.Value);
		}
		inputLayer.AddButton(delegate
		{
			OnLabelsShow();
		}, 9);
		gridConsoleNavigationBehaviour.GetInputLayer(inputLayer);
		m_LabelsDisposable.Add(gridConsoleNavigationBehaviour);
		m_LabelsDisposable.Add(GamePad.Instance.PushLayer(inputLayer));
		foreach (IDisposable item2 in m_LabelsDisposable)
		{
			AddDisposable(item2);
		}
	}

	public void HideLabelsButton()
	{
		m_LabelsList.gameObject.SetActive(value: false);
		foreach (GameObject labelsListItem in m_LabelsListItems)
		{
			UnityEngine.Object.Destroy(labelsListItem);
		}
		foreach (IDisposable item in m_LabelsDisposable)
		{
			RemoveDisposable(item);
			item.Dispose();
		}
		m_LabelsDisposable.Clear();
	}

	public void OnFeedbackToggleValueChanged(bool isFeedback)
	{
		ReportingUtils.Instance.SelectIsFeedback(isFeedback);
	}

	public void OnContextDropdownValueChanged()
	{
		ReportingUtils.Instance.SelectContext(m_ContextDropdown.Index.Value);
		m_AspectDropdown.SetIndex(ReportingUtils.Instance.GetInitialAspectIndex());
		if (BuildModeUtility.IsDevelopment && ReportingUtils.Instance.Assignees.IsCompletedSuccessfully)
		{
			try
			{
				m_AssigneeGO.gameObject.SetActive(value: true);
				m_AssigneeDropdown.Bind(base.ViewModel.GetAssigneeDropDownVM(m_ContextDropdown.Index.Value));
				SetupDropdowns();
				return;
			}
			catch
			{
				m_AssigneeGO.gameObject.SetActive(value: false);
				m_FixVersionGO.SetActive(value: false);
				return;
			}
		}
		m_AssigneeGO.gameObject.SetActive(value: false);
		if (m_FixVersionGO.activeSelf)
		{
			m_FixVersionGO.SetActive(value: false);
		}
		if (m_LabelsGroup.activeSelf)
		{
			m_LabelsGroup.SetActive(value: false);
		}
	}

	public void OnAspectDropdownValueChanged()
	{
		ReportingUtils.Instance.SelectAspect(m_AspectDropdown.Index.Value);
		int value = m_AspectDropdown.Index.Value;
		int value2 = m_ContextDropdown.Index.Value;
		if (BuildModeUtility.IsDevelopment && ReportingUtils.Instance.Assignees.IsCompletedSuccessfully)
		{
			try
			{
				int assigneeIndex = base.ViewModel.GetAssigneeIndex(value2, value);
				m_AssigneeDropdown.SetIndex(assigneeIndex);
				OnAssigneeDropdownValueChanged();
				m_AssigneeGO.gameObject.SetActive(value: true);
				return;
			}
			catch
			{
				m_AssigneeGO.gameObject.SetActive(value: false);
				m_FixVersionGO.SetActive(value: false);
				return;
			}
		}
		m_AssigneeGO.gameObject.SetActive(value: false);
		if (m_FixVersionGO.activeSelf)
		{
			m_FixVersionGO.SetActive(value: false);
		}
		if (m_LabelsGroup.activeSelf)
		{
			m_LabelsGroup.SetActive(value: false);
		}
	}

	public void OnAssigneeDropdownValueChanged()
	{
		string text = m_AssigneeDropdown.GetCurrentTextValue();
		if (text.Contains("ui_designer"))
		{
			text = "ui_designer";
		}
		ReportingUtils.Instance.SelectAssignee(text);
	}

	public void OnFixVersionDropdownValueChanged()
	{
		ReportingUtils.Instance.SelectFixVersion(m_FixVersionDropdown.Index.Value);
	}

	public void OnManualSaveDropdownValueChanged()
	{
		ReportingUtils.Instance.SelectManualSave(m_ManualSaveDropdown.Index.Value - 1);
	}

	public void OnLinkClick(PointerEventData eventData, TMP_LinkInfo linkInfo)
	{
		if (linkInfo.GetLinkID() == "pp")
		{
			Application.OpenURL("https://owlcatgames.com/privacy");
		}
	}

	private void SetTranslationText()
	{
		m_DescriptionTitleText.text = UIStrings.Instance.UIBugReport.DesctiptionHeader;
		m_MainTitleText.text = UIStrings.Instance.UIBugReport.Header;
		m_AspectTitleText.text = UIStrings.Instance.UIBugReport.AspectHeader;
		m_ContextTitleText.text = UIStrings.Instance.UIBugReport.ContextHeader;
		m_SendButtonText.text = UIStrings.Instance.UIBugReport.SendButton;
		m_HintText.text = UIStrings.Instance.UIBugReport.HintText;
		m_SuggestionToggleText.text = UIStrings.Instance.UIBugReport.SuggestionTogle;
		m_NormalToggleText.text = UIStrings.Instance.UIBugReport.NormalTogle;
		m_CriticalToggleText.text = UIStrings.Instance.UIBugReport.CriticalTogle;
		m_EmailTitleText.text = UIStrings.Instance.UIBugReport.EmailHeader;
		m_DiscordTitleText.text = UIStrings.Instance.UIBugReport.DiscordHeader;
		m_BottomDescriptionText.text = UIStrings.Instance.UIBugReport.ButtomDescription;
		m_PrivacyDescriptionText.text = UIStrings.Instance.UIBugReport.PrivacyCheckBoxDescription;
		m_EmailUpdatesDescriptionText.text = UIStrings.Instance.UIBugReport.EmailUpdatesCheckBoxDescription;
	}

	private string GetIssueType()
	{
		if (BuildModeUtility.IsDevelopment)
		{
			return m_DevPriorityDropdown.GetCurrentTextValue();
		}
		OwlcatToggle key = m_IssueTypeToggleGroup.ActiveToggles().FirstOrDefault() ?? m_NormalToggle;
		return m_IssueTypes[key].ToString();
	}

	private bool IsEmailMatchRegexp()
	{
		if (string.IsNullOrEmpty(m_EmailInputField.Text))
		{
			return false;
		}
		return new Regex("^\\w+[\\w-\\.]*\\@\\w+((-\\w+)|(\\w*))\\.[a-z]{2,9}$").Matches(m_EmailInputField.Text).Count == 1;
	}

	private void ToggleAdditionalContactsVisibility(bool toShow)
	{
		m_PrevEmail = m_EmailInputField.Text;
		if (toShow && !m_DiscordGameObject.activeSelf)
		{
			m_DiscordGameObject.SetActive(value: true);
		}
		else if (!toShow && m_DiscordGameObject.activeSelf)
		{
			m_DiscordGameObject.SetActive(value: false);
		}
	}

	private void HandleDiscordText()
	{
		if (m_PrevDiscord != m_DiscordInputField.Text)
		{
			try
			{
				m_DiscordInputField.Text = m_DiscordInputField.Text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
			}
			catch
			{
			}
		}
		m_PrevDiscord = m_DiscordInputField.Text;
	}

	private void ExpandDescriptionOverMarket()
	{
		bool item = ReportingUtils.Instance.PrivacyStuffGetEmailAgreements(m_EmailInputField.Text).promo;
		m_EmailUpdatesToggle.Set(item);
		m_EmailUpdatesGroup.SetActive(!item);
	}

	private void LateUpdate()
	{
		if (m_PrevEmail != m_EmailInputField.Text)
		{
			ToggleAdditionalContactsVisibility(IsEmailMatchRegexp());
			(bool, bool) tuple = ReportingUtils.Instance.PrivacyStuffGetEmailAgreements(m_EmailInputField.Text);
			m_PrivacyToggle.Set(tuple.Item2);
			ExpandDescriptionOverMarket();
		}
		HandleDiscordText();
	}
}
