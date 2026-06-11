using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.NecronTimer;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.NecronTimer;

public class NecronTimerView : ViewBase<NecronTimerVM>, IGameModeHandler, ISubscriber
{
	[SerializeField]
	private Image SliderHead;

	[SerializeField]
	private Image SliderBody;

	[SerializeField]
	private Image StartMilestoneIcon;

	[SerializeField]
	private Image MiddleMilestoneIcon;

	[SerializeField]
	private Image FinishMilestoneIcon;

	[SerializeField]
	private List<CanvasGroup> SelectorObjects;

	[SerializeField]
	private List<GameObject> ScaleObjects;

	[SerializeField]
	private MoveAnimator Animator;

	[SerializeField]
	private CanvasGroup m_CutsceneCanvasGroup;

	[SerializeField]
	private float m_FillAnimationDuration = 0.3f;

	[SerializeField]
	private float m_FillAnimationDelay = 0.5f;

	[SerializeField]
	private Ease m_FillAnimationEase = Ease.OutCubic;

	[SerializeField]
	private Vector3 m_HeadPunchScale = new Vector3(0.3f, 0.3f, 0f);

	[SerializeField]
	private float m_HeadPunchDuration = 0.2f;

	private Tweener m_FillTween;

	private Tweener m_HeadTween;

	[SerializeField]
	protected RectTransform m_TooltipPlace;

	protected override void BindViewImplementation()
	{
		SetUpTimerValues();
		SetInitialColors();
		AddDisposable(base.ViewModel.IsUnlockedAndVisible.Subscribe(HandleVisibility));
		AddDisposable(base.ViewModel.CurrentTimerValue.Subscribe(delegate(int value)
		{
			UpdateTimerValue(value);
		}));
		AddDisposable(base.ViewModel.UpdateTimer.Subscribe(delegate(int value)
		{
			SetUpTimerValues();
			SetInitialColors();
			UpdateTimerValue(value, instant: true);
		}));
		UpdateTimerValue(0, instant: true);
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.TooltipPlace = m_TooltipPlace;
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(0f, 1f),
			new Vector2(0f, 0.75f),
			new Vector2(0f, 0.5f),
			new Vector2(0f, 0.25f)
		};
		TooltipConfig config = tooltipConfig;
		AddDisposable(this.SetTooltip(new TooltipTemplateSimple(tooltips.NecronTimerHeader, tooltips.NecronTimerDescription), config));
		AddDisposable(EventBus.Subscribe(this));
		SetHiddenByCutscene(Game.Instance.IsModeActive(GameModeType.Cutscene));
	}

	protected override void DestroyViewImplementation()
	{
		m_FillTween?.Kill();
		m_HeadTween?.Kill();
	}

	private void SetInitialColors()
	{
		SliderHead.color = UIConfig.Instance.NecronTimer.SliderHandle;
		SliderBody.color = UIConfig.Instance.NecronTimer.Slider;
		StartMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOn;
		MiddleMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOff;
		FinishMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOff;
		MiddleMilestoneIcon.gameObject.transform.localPosition = SelectorObjects[UIConfig.Instance.NecronTimer.MiddleMilestoneIndex].gameObject.transform.localPosition;
		StartMilestoneIcon.gameObject.transform.localPosition = SelectorObjects[0].gameObject.transform.localPosition;
		FinishMilestoneIcon.gameObject.transform.localPosition = SelectorObjects[UIConfig.Instance.NecronTimer.MaxTimerValue - 1].gameObject.transform.localPosition;
	}

	private void UpdateTimerValue(int value, bool instant = false)
	{
		int maxTimerValue = UIConfig.Instance.NecronTimer.MaxTimerValue;
		float num = (float)(value + 1) / (float)maxTimerValue;
		float duration = Mathf.Abs(num - SliderBody.fillAmount) * (float)maxTimerValue * m_FillAnimationDuration;
		m_FillTween?.Kill();
		if (instant)
		{
			SliderBody.fillAmount = num;
		}
		else
		{
			m_FillTween = SliderBody.DOFillAmount(num, duration).SetDelay(m_FillAnimationDelay).SetEase(m_FillAnimationEase)
				.SetUpdate(isIndependentUpdate: true)
				.SetAutoKill(autoKillOnCompletion: true);
		}
		SelectorObjects.ForEach(delegate(CanvasGroup cG)
		{
			cG.alpha = 0f;
		});
		if (value <= maxTimerValue)
		{
			SelectorObjects[value].alpha = 1f;
			Vector3 localPosition = SelectorObjects[value].transform.localPosition;
			m_HeadTween?.Kill();
			if (instant)
			{
				SliderHead.transform.localPosition = localPosition;
			}
			else
			{
				m_HeadTween = SliderHead.transform.DOLocalMove(localPosition, duration).SetDelay(m_FillAnimationDelay).SetEase(m_FillAnimationEase)
					.SetUpdate(isIndependentUpdate: true)
					.SetAutoKill(autoKillOnCompletion: true);
			}
		}
		if (value >= 0)
		{
			StartMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOn;
			SelectorObjects[0].alpha = 1f;
		}
		else
		{
			StartMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOff;
			SelectorObjects[0].alpha = 0f;
		}
		if (value >= UIConfig.Instance.NecronTimer.MiddleMilestoneIndex)
		{
			MiddleMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOn;
			SelectorObjects[UIConfig.Instance.NecronTimer.MiddleMilestoneIndex].alpha = 1f;
		}
		else
		{
			MiddleMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOff;
			SelectorObjects[UIConfig.Instance.NecronTimer.MiddleMilestoneIndex].alpha = 0f;
		}
		if (value >= UIConfig.Instance.NecronTimer.MaxTimerValue)
		{
			FinishMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOn;
			SelectorObjects[UIConfig.Instance.NecronTimer.MaxTimerValue].alpha = 1f;
		}
		else
		{
			FinishMilestoneIcon.color = UIConfig.Instance.NecronTimer.TimerMilestoneOff;
			SelectorObjects[UIConfig.Instance.NecronTimer.MaxTimerValue].alpha = 0f;
		}
	}

	private void SetUpTimerValues()
	{
		SelectorObjects.ForEach(delegate(CanvasGroup cG)
		{
			cG.gameObject.SetActive(value: false);
		});
		ScaleObjects.ForEach(delegate(GameObject gO)
		{
			gO.SetActive(value: false);
		});
		for (int i = 0; i < UIConfig.Instance.NecronTimer.MaxTimerValue; i++)
		{
			if (i > SelectorObjects.Count || i > ScaleObjects.Count)
			{
				SelectorObjects.ForEach(delegate(CanvasGroup cG)
				{
					cG.gameObject.SetActive(value: false);
				});
				ScaleObjects.ForEach(delegate(GameObject gO)
				{
					gO.SetActive(value: false);
				});
				break;
			}
			SelectorObjects[i].gameObject.SetActive(value: true);
			ScaleObjects[i].SetActive(value: true);
		}
	}

	private void HandleVisibility(bool value)
	{
		if (value)
		{
			Animator.AppearAnimation();
		}
		else
		{
			Animator.DisappearAnimation();
		}
		SetInitialColors();
		UpdateTimerValue(base.ViewModel.CurrentTimerValue.Value, instant: true);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		SetHiddenByCutscene(gameMode == GameModeType.Cutscene);
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private void SetHiddenByCutscene(bool hidden)
	{
		if (!(m_CutsceneCanvasGroup == null))
		{
			m_CutsceneCanvasGroup.alpha = (hidden ? 0f : 1f);
			m_CutsceneCanvasGroup.blocksRaycasts = !hidden;
			m_CutsceneCanvasGroup.interactable = !hidden;
		}
	}
}
