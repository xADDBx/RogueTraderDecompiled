using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;

public class CareerPathListSelectedCareerCommonView : ViewBase<CareerPathVM>, IConfirmClickHandler, IConsoleEntity, IConsoleNavigationEntity, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private TextMeshProUGUI m_SelectedCareerStatusLabel;

	[SerializeField]
	private TextMeshProUGUI m_SelectedCareerUpgradesLabel;

	[SerializeField]
	private TextMeshProUGUI m_ClickToDoctrineLabel;

	[SerializeField]
	private string m_ClickToDoctrinePrefix = "<----   ";

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[Header("Career")]
	[SerializeField]
	private Image m_CareerIcon;

	[SerializeField]
	private Image m_AppliedProgressBar;

	[SerializeField]
	private Image m_CurrentProgressBar;

	[SerializeField]
	private RectTransform m_ProgressPointer;

	private IDisposable m_TooltipHandler;

	private AccessibilityTextHelper m_TextHelper;

	private UITextCharSheet Strings => UIStrings.Instance.CharacterSheet;

	public void Initialize()
	{
		m_TextHelper = new AccessibilityTextHelper(m_SelectedCareerUpgradesLabel, m_ClickToDoctrineLabel);
		Hide();
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.CareerTooltip, new TooltipConfig
		{
			TooltipPlace = m_TooltipPlace,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		}));
		AddDisposable(m_MainButton.OnPointerEnterAsObservable().Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(ICareerPathHoverHandler h)
			{
				h.HandleHoverStart(base.ViewModel.CareerPath);
			});
		}));
		AddDisposable(m_MainButton.OnPointerExitAsObservable().Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(ICareerPathHoverHandler h)
			{
				h.HandleHoverStop();
			});
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetCareerPath();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_MainButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.SetCareerPath();
		}));
		string text = (Game.Instance.IsControllerMouse ? UIStrings.Instance.CharacterSheet.ClickToDoctrine.Text : UIStrings.Instance.CharacterSheet.ClickToDoctrineConsole.Text);
		m_ClickToDoctrineLabel.text = m_ClickToDoctrinePrefix + text;
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite value)
		{
			m_CareerIcon.sprite = value;
		}));
		AddDisposable(base.ViewModel.CurrentRank.Subscribe(SetupUpgradesLabel));
		AddDisposable(base.ViewModel.CanUpgrade.Subscribe(delegate
		{
			SetupUpgradesLabel(base.ViewModel.CurrentRank.Value);
		}));
		AddDisposable(base.ViewModel.OnUpdateData.Subscribe(UpdateStatus));
		UpdateStatus();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_TextHelper.Dispose();
	}

	public void UpdateStatus()
	{
		string activeLayer = string.Empty;
		if (base.ViewModel.IsFinished)
		{
			activeLayer = "CareerFinished";
		}
		else if (base.ViewModel.IsInProgress)
		{
			activeLayer = "CareerInProgress";
		}
		m_SelectedCareerStatusLabel.text = base.ViewModel.Name;
		m_MainButton.SetActiveLayer(activeLayer);
		UpdateProgress();
	}

	private void UpdateProgress()
	{
		if (base.ViewModel.IsFinished)
		{
			Image appliedProgressBar = m_AppliedProgressBar;
			float fillAmount = (m_CurrentProgressBar.fillAmount = 1f);
			appliedProgressBar.fillAmount = fillAmount;
			m_ProgressPointer.gameObject.SetActive(value: false);
		}
		else if (!base.ViewModel.IsAvailableToUpgrade && !base.ViewModel.IsInProgress)
		{
			Image appliedProgressBar2 = m_AppliedProgressBar;
			float fillAmount = (m_CurrentProgressBar.fillAmount = 0f);
			appliedProgressBar2.fillAmount = fillAmount;
			m_ProgressPointer.rotation = Quaternion.Euler(0f, 0f, 0f);
			m_ProgressPointer.gameObject.SetActive(value: true);
		}
		else
		{
			(int, int) currentLevelupRange = base.ViewModel.GetCurrentLevelupRange();
			int num3 = ((currentLevelupRange.Item1 == -1) ? (base.ViewModel.CurrentRank.Value - 1) : (currentLevelupRange.Item1 - 2));
			int num4 = ((currentLevelupRange.Item2 == base.ViewModel.MaxRank) ? base.ViewModel.MaxRank : (currentLevelupRange.Item2 - 1));
			float num5 = (float)num3 / (float)base.ViewModel.MaxRank;
			m_AppliedProgressBar.fillAmount = num5;
			m_CurrentProgressBar.fillAmount = (float)num4 / (float)base.ViewModel.MaxRank;
			float z = -360f * num5;
			m_ProgressPointer.rotation = Quaternion.Euler(0f, 0f, z);
			m_ProgressPointer.gameObject.SetActive(num5 < 1f);
		}
	}

	private void SetupUpgradesLabel(int currentRank)
	{
		if (base.ViewModel.IsFinished)
		{
			m_SelectedCareerUpgradesLabel.gameObject.SetActive(value: true);
			m_SelectedCareerUpgradesLabel.text = string.Format(Strings.SelectedCareerFinished.Text, base.ViewModel.Name);
		}
		else if (base.ViewModel.IsAvailableToUpgrade)
		{
			int characterLevel = base.ViewModel.Unit.Progression.CharacterLevel;
			int experienceLevel = base.ViewModel.Unit.Progression.ExperienceLevel;
			int a = Math.Max(0, experienceLevel - characterLevel);
			a = Mathf.Min(a, base.ViewModel.MaxRank - base.ViewModel.CurrentRank.Value);
			m_SelectedCareerUpgradesLabel.gameObject.SetActive(value: true);
			m_SelectedCareerUpgradesLabel.text = string.Format(Strings.RanksCounterLabel.Text, a.ToString());
		}
		else
		{
			m_SelectedCareerUpgradesLabel.gameObject.SetActive(value: false);
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.CareerTooltip;
	}

	public bool CanConfirmClick()
	{
		return m_MainButton.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		m_MainButton.OnConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
