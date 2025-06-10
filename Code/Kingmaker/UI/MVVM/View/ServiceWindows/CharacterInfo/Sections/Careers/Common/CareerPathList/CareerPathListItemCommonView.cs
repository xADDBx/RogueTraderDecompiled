using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Canvases;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;

public class CareerPathListItemCommonView : ViewBase<CareerPathVM>, IWidgetView, IConfirmClickHandler, IConsoleEntity, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IHasTooltipTemplates, IHasTooltipTemplate, ICareerPathHoverHandler, ISubscriber
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private TextMeshProUGUI m_CareerName;

	[SerializeField]
	private Image m_CareerIcon;

	[SerializeField]
	private GameObject m_SelectedMark;

	[SerializeField]
	private Image m_RecommendMark;

	[SerializeField]
	private GameObject m_CurrentRankEntryMark;

	[SerializeField]
	protected GameObject m_SelectedIcon;

	[SerializeField]
	private bool m_IsInsideCareerProgression;

	[SerializeField]
	private bool m_PrerequisitesAffectsHighlight = true;

	[Header("Progress")]
	[SerializeField]
	private GameObject m_ProgressBar;

	[SerializeField]
	private TextMeshProUGUI m_ProgressText;

	[SerializeField]
	private Image m_ProgressValueBar;

	[Header("Lines")]
	[ShowIf("CareerProgressionListItem")]
	[SerializeField]
	private RectTransform m_TopLineAnchor;

	[ShowIf("CareerProgressionListItem")]
	[SerializeField]
	private RectTransform m_BotLineAnchor;

	[ShowIf("CareerProgressionListItem")]
	[SerializeField]
	private RectTransform m_LinePrefab;

	[ShowIf("CareerProgressionListItem")]
	[SerializeField]
	private RectTransform m_PointPrefab;

	[ShowIf("CareerProgressionListItem")]
	[SerializeField]
	private RectTransform m_LinesContainer;

	protected readonly ReactiveProperty<CareerItemState> ItemState = new ReactiveProperty<CareerItemState>();

	private readonly BoolReactiveProperty m_IsHighlighted = new BoolReactiveProperty();

	protected bool ShouldShowTooltip = true;

	private RectTransform m_TooltipPlace;

	private AccessibilityTextHelper m_TextHelper;

	private List<GameObject> m_LinesObjects = new List<GameObject>();

	private bool CareerProgressionListItem => !m_IsInsideCareerProgression;

	public MonoBehaviour MonoBehaviour => this;

	public void SetViewParameters(RectTransform tooltipPlace)
	{
		m_TooltipPlace = tooltipPlace;
	}

	protected override void BindViewImplementation()
	{
		m_TextHelper = new AccessibilityTextHelper(m_CareerName);
		m_LinesContainer.Or(null)?.gameObject.SetActive(value: false);
		if (m_CareerName != null)
		{
			m_CareerName.text = base.ViewModel.Name;
		}
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite value)
		{
			m_CareerIcon.sprite = value;
		}));
		AddDisposable(m_IsHighlighted.Subscribe(delegate
		{
			OnUpdateData();
		}));
		AddDisposable(base.ViewModel.OnUpdateData.Subscribe(OnUpdateData));
		AddDisposable(base.ViewModel.ItemState.Subscribe(delegate(CareerItemState value)
		{
			ItemState.Value = value;
		}));
		AddDisposable(ItemState.Subscribe(delegate
		{
			UpdateButtonState();
		}));
		AddDisposable(base.ViewModel.OnUpdateSelected.Subscribe(UpdateSelectedState));
		AddDisposable(ObservableExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			HandleClick();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_MainButton.OnConfirmClickAsObservable(), delegate
		{
			HandleClick();
		}));
		AddDisposable(m_MainButton.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnHoverStart();
		}));
		AddDisposable(m_MainButton.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnHoverEnd();
		}));
		AddDisposable(m_MainButton.OnFocusAsObservable().Subscribe(HandleSetFocus));
		if (m_IsInsideCareerProgression && (bool)m_CurrentRankEntryMark)
		{
			AddDisposable(base.ViewModel.IsCurrentRankEntryItem.Subscribe(OnSelectedInPathChanged));
		}
		if (m_SelectedMark != null)
		{
			AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelectedChanged));
		}
		if (m_RecommendMark != null)
		{
			AddDisposable(base.ViewModel.IsRecommended.Subscribe(m_RecommendMark.gameObject.SetActive));
			AddDisposable(m_RecommendMark.SetHint(UIStrings.Instance.CharacterSheet.RecommendedCareerPath));
		}
		if (ShouldShowTooltip)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.CareerTooltip, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2>
				{
					new Vector2(1f, 0.5f)
				}
			}));
		}
		AddDisposable(EventBus.Subscribe(this));
		OnUpdateData();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
		foreach (GameObject linesObject in m_LinesObjects)
		{
			Object.Destroy(linesObject);
		}
	}

	public void OnSelectedInPathChanged(bool value)
	{
		m_CurrentRankEntryMark.SetActive(value);
	}

	public void OnSelectedChanged(bool value)
	{
		m_SelectedMark.SetActive(value);
		m_MainButton.CanConfirm = !value;
	}

	private void OnUpdateData()
	{
		UpdateProgress();
		UpdateButtonState();
	}

	protected virtual void HandleClick()
	{
		if (base.ViewModel.Unit.CanEditCareer())
		{
			if (m_IsInsideCareerProgression)
			{
				base.ViewModel.SetRankEntry(null);
			}
			else
			{
				base.ViewModel.SetCareerPath();
			}
		}
	}

	private void UpdateProgress()
	{
		bool flag = base.ViewModel.IsInProgress || base.ViewModel.IsFinished;
		if (m_ProgressText != null)
		{
			m_ProgressText.text = (flag ? $"{base.ViewModel.CurrentRank.Value}/{base.ViewModel.MaxRank}" : string.Empty);
		}
		if (m_ProgressBar != null)
		{
			m_ProgressBar.SetActive(base.ViewModel.IsInProgress);
			m_ProgressValueBar.fillAmount = base.ViewModel.Progress.Value;
		}
	}

	protected virtual void UpdateButtonState()
	{
		bool value = m_IsHighlighted.Value;
		bool flag = base.ViewModel.IsUnlocked || base.ViewModel.CanShowToAnotherCoopPlayer();
		string activeLayer = (value ? "Highlighted" : (flag ? "Unlocked" : "Locked"));
		m_MainButton.SetActiveLayer(activeLayer);
	}

	private void UpdateSelectedState()
	{
		OnHoverEnd();
		if ((bool)m_SelectedIcon)
		{
			m_SelectedIcon.gameObject.SetActive(base.ViewModel.IsSelected.Value);
		}
	}

	private void OnHoverStart()
	{
		m_LinesContainer.Or(null)?.gameObject.SetActive(value: true);
		EventBus.RaiseEvent(delegate(ICareerPathHoverHandler h)
		{
			h.HandleHoverStart(base.ViewModel.CareerPath);
		});
	}

	protected void OnHoverEnd()
	{
		m_LinesContainer.Or(null)?.gameObject.SetActive(value: false);
		EventBus.RaiseEvent(delegate(ICareerPathHoverHandler h)
		{
			h.HandleHoverStop();
		});
	}

	public void HandleHoverStart(BlueprintCareerPath careerPath)
	{
		if (base.gameObject.activeInHierarchy && m_PrerequisitesAffectsHighlight)
		{
			bool value = base.ViewModel.PrerequisiteCareerPaths.Contains(careerPath);
			m_IsHighlighted.Value = value;
		}
	}

	public void HandleHoverStop()
	{
		if (base.gameObject.activeInHierarchy && m_PrerequisitesAffectsHighlight)
		{
			m_IsHighlighted.Value = base.ViewModel.IsHighlighted.Value;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CareerPathVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CareerPathVM;
	}

	public void HandleSetFocus(bool value)
	{
		if (value)
		{
			OnHoverStart();
		}
		else
		{
			OnHoverEnd();
		}
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public virtual bool CanConfirmClick()
	{
		return m_MainButton.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		if (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value == null && base.ViewModel.IsInLevelupProcess && m_IsInsideCareerProgression)
		{
			if (!base.ViewModel.CanCommit.Value || !base.ViewModel.AllVisited.Value)
			{
				base.ViewModel.SelectNextItem();
			}
			return;
		}
		m_MainButton.OnConfirmClick();
		if (m_IsInsideCareerProgression)
		{
			EventBus.RaiseEvent(delegate(IRankEntryConfirmClickHandler h)
			{
				h.OnRankEntryConfirmClick();
			});
		}
	}

	public virtual string GetConfirmClickHint()
	{
		if (base.ViewModel.IsInLevelupProcess)
		{
			return (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value == null) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
		}
		return (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value == null && m_IsInsideCareerProgression) ? UIStrings.Instance.CommonTexts.Expand : UIStrings.Instance.CommonTexts.Select;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return new List<TooltipBaseTemplate>
		{
			base.ViewModel.CareerTooltip,
			base.ViewModel.CareerHintTemplate
		};
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.CareerTooltip;
	}

	public void CreateLines(List<CareerPathListItemCommonView> allCareers)
	{
		List<CareerPathVM> list = new List<CareerPathVM> { base.ViewModel };
		List<(CareerPathListItemCommonView, CareerPathListItemCommonView)> list2 = new List<(CareerPathListItemCommonView, CareerPathListItemCommonView)>();
		foreach (CareerPathListItemCommonView allCareer in allCareers)
		{
			for (int i = 0; i < list.Count; i++)
			{
				CareerPathVM linkedCareer = list[i];
				if (allCareer.ViewModel.CareerPath.Tier == linkedCareer.CareerPath.Tier + 1 && (allCareer.ViewModel.PrerequisiteCareerPaths.Contains(linkedCareer.CareerPath) || allCareer.ViewModel.SelectionHasPrerequisiteFeatures()))
				{
					list.Add(allCareer.ViewModel);
					CareerPathListItemCommonView item = allCareers.FirstOrDefault((CareerPathListItemCommonView c) => c.ViewModel == linkedCareer);
					list2.Add((item, allCareer));
				}
			}
		}
		CreateLinesView(list2.Select(((CareerPathListItemCommonView, CareerPathListItemCommonView) t) => (t.Item1.m_BotLineAnchor, t.Item2.m_TopLineAnchor)).ToList(), m_LinesContainer);
	}

	private void CreateLinesView(List<(RectTransform, RectTransform)> points, RectTransform container)
	{
		if (m_LinesContainer == null)
		{
			return;
		}
		float x = MainCanvas.Instance.RectTransform.localScale.x;
		foreach (var point in points)
		{
			if (!(point.Item1 == null) && !(point.Item2 == null))
			{
				Vector3 position = point.Item1.position;
				Vector3 position2 = point.Item2.position;
				if ((bool)m_LinePrefab)
				{
					RectTransform rectTransform = Object.Instantiate(m_LinePrefab, container, worldPositionStays: false);
					rectTransform.position = point.Item1.position;
					Vector3 vector = position2 - position;
					float magnitude = vector.magnitude;
					float z = Mathf.Asin(vector.x / magnitude) * 57.29578f;
					rectTransform.eulerAngles = new Vector3(0f, 0f, z);
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, magnitude / x);
					m_LinesObjects.Add(rectTransform.gameObject);
				}
				if ((bool)m_PointPrefab)
				{
					RectTransform rectTransform2 = Object.Instantiate(m_PointPrefab, container, worldPositionStays: false);
					rectTransform2.position = position;
					m_LinesObjects.Add(rectTransform2.gameObject);
				}
			}
		}
	}
}
