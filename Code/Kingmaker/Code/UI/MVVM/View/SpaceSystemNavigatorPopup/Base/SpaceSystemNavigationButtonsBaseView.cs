using System;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SpaceSystemNavigatorPopup;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup.Base;

public class SpaceSystemNavigationButtonsBaseView : ViewBase<SpaceSystemNavigationButtonsVM>
{
	[Header("Buttons")]
	[SerializeField]
	protected OwlcatButton m_CreateWayButton;

	[SerializeField]
	protected OwlcatButton[] m_UpgradeButtons;

	[Header("UpgradeWay")]
	[SerializeField]
	private TextMeshProUGUI[] m_UpgradeCostText;

	[SerializeField]
	protected FadeAnimator m_CreateAndUpgradeWayLabelFadeAnimator;

	[SerializeField]
	protected TextMeshProUGUI m_CreateAndUpgradeWayLabelText;

	[SerializeField]
	private Image[] m_UpgradeWayCostFillingImage;

	[Header("CreateWay")]
	[SerializeField]
	private TextMeshProUGUI m_CreateWayCostText;

	[SerializeField]
	private Image m_CreateWayFillingImage;

	[Header("Another")]
	[SerializeField]
	private FadeAnimator m_ChangeNavigatorResourceAnimator;

	[SerializeField]
	private TextMeshProUGUI m_PlusNavigatorCostText;

	[SerializeField]
	private FadeAnimator m_NavigationWindowFadeAnimator;

	[SerializeField]
	private Image[] m_ChangeDifficultyColorObjects;

	[SerializeField]
	private Color[] m_DifficultyColors;

	[SerializeField]
	protected Image m_EagleImage;

	[SerializeField]
	private Color[] m_DifficultyEagleColors;

	[SerializeField]
	protected Color[] m_DifficultyHintPanelTextsColors;

	protected override void BindViewImplementation()
	{
		m_CreateWayButton.Interactable = base.ViewModel.IsScannedFrom.Value && base.ViewModel.IsAvailable.Value;
		if (!base.ViewModel.IsScannedFrom.Value)
		{
			AddDisposable(m_CreateWayButton.SetHint(UIStrings.Instance.SystemMap.ScanRequired));
		}
		m_NavigationWindowFadeAnimator.AppearAnimation();
		base.ViewModel.ClosePopupsCanvas(state: true);
		ConfigurePopupDetails();
		AddDisposable(base.ViewModel.CreateWayCost.Subscribe(delegate(int cost)
		{
			m_CreateWayCostText.text = cost.ToString();
		}));
		AddDisposable(base.ViewModel.IsTravelNewSectorAvailable.Subscribe(WayIsOpen));
		AddDisposable(base.ViewModel.CurrentValueOfResources.Subscribe(delegate
		{
			HandleCostChanged();
		}));
		AddDisposable(base.ViewModel.IsScanning.CombineLatest(base.ViewModel.IsTraveling, base.ViewModel.IsDialogActive, base.ViewModel.IsWayUpgrading, base.ViewModel.IsWayCreating, (bool isScanning, bool isTraveling, bool isDialogActive, bool isWayUpgrading, bool isWayCreating) => isScanning || isTraveling || isDialogActive || isWayUpgrading || isWayCreating).Subscribe(LockButtons));
	}

	protected void ShowCreateWayButtonHoverPanel(bool state)
	{
		if (!state || !base.ViewModel.IsScannedFrom.Value)
		{
			m_CreateAndUpgradeWayLabelFadeAnimator.DisappearAnimation();
			EventBus.RaiseEvent(delegate(IGlobalMapWillChangeNavigatorResourceEffectHandler h)
			{
				h.HandleWillChangeNavigatorResourceEffect(state: false, 0);
			});
			return;
		}
		m_CreateAndUpgradeWayLabelFadeAnimator.AppearAnimation();
		bool flag = base.ViewModel.CurrentValueOfResources.Value >= base.ViewModel.CreateWayCost.Value;
		m_CreateAndUpgradeWayLabelText.text = (flag ? string.Concat(UIStrings.Instance.GlobalMap.CreateWay, " <color=#", ColorUtility.ToHtmlStringRGBA(m_DifficultyHintPanelTextsColors.LastOrDefault()), ">[", UIStrings.Instance.GlobalMapPassages.GetDifficultyString(SectorMapPassageEntity.PassageDifficulty.Deadly), "]</color>") : string.Concat(UIStrings.Instance.GlobalMap.NoResource, " [", UIStrings.Instance.SpaceCombatTexts.NavigatorResource, "]"));
		EventBus.RaiseEvent(delegate(IGlobalMapWillChangeNavigatorResourceEffectHandler h)
		{
			h.HandleWillChangeNavigatorResourceEffect(state: true, base.ViewModel.CreateWayCost.Value);
		});
	}

	protected void ShowUpgradeWayButtonHoverPanel(OwlcatButton b, bool state)
	{
		if (!state)
		{
			m_CreateAndUpgradeWayLabelFadeAnimator.DisappearAnimation();
			EventBus.RaiseEvent(delegate(IGlobalMapWillChangeNavigatorResourceEffectHandler h)
			{
				h.HandleWillChangeNavigatorResourceEffect(state: false, 0);
			});
			return;
		}
		m_CreateAndUpgradeWayLabelFadeAnimator.AppearAnimation();
		SectorMapPassageEntity passage = base.ViewModel.GetPassage();
		if (passage != null)
		{
			int cost = (int)(passage.CurrentDifficulty - m_UpgradeButtons.IndexOf(b)) * base.ViewModel.UpgradeWayCost.Value;
			bool flag = base.ViewModel.CurrentValueOfResources.Value >= cost;
			int num = m_UpgradeButtons.IndexOf(b);
			m_CreateAndUpgradeWayLabelText.text = (flag ? string.Concat(UIStrings.Instance.GlobalMap.UpgradeWayCost, " <color=#", ColorUtility.ToHtmlStringRGBA(m_DifficultyHintPanelTextsColors[num]), ">[", UIStrings.Instance.GlobalMapPassages.GetDifficultyString((SectorMapPassageEntity.PassageDifficulty)num), "]</color>") : string.Concat(UIStrings.Instance.GlobalMap.NoResource, " [", UIStrings.Instance.SpaceCombatTexts.NavigatorResource, "]"));
			EventBus.RaiseEvent(delegate(IGlobalMapWillChangeNavigatorResourceEffectHandler h)
			{
				h.HandleWillChangeNavigatorResourceEffect(state: true, cost);
			});
		}
	}

	protected virtual void WayIsOpen(bool open)
	{
		m_CreateWayButton.transform.parent.gameObject.SetActive(!open);
		m_UpgradeButtons.FirstOrDefault()?.transform.parent.gameObject.SetActive(open);
		if (open)
		{
			CheckUpgradeButtonsVisible();
		}
		else
		{
			m_CreateWayButton.Interactable = base.ViewModel.CurrentValueOfResources.Value >= base.ViewModel.CreateWayCost.Value && base.ViewModel.IsScannedFrom.Value;
		}
	}

	protected void HandleLowerSectorMapPassageDifficulty()
	{
		CheckUpgradeButtonsVisible();
	}

	protected virtual void CheckUpgradeButtonsVisible()
	{
		m_UpgradeButtons.ForEach(delegate(OwlcatButton b)
		{
			b.gameObject.SetActive(value: false);
		});
		m_UpgradeWayCostFillingImage.ForEach(delegate(Image i)
		{
			i.fillAmount = 0f;
		});
		m_CreateWayFillingImage.fillAmount = 0f;
		SectorMapPassageEntity passage = base.ViewModel.GetPassage();
		if (passage == null)
		{
			return;
		}
		m_ChangeDifficultyColorObjects.ForEach(delegate(Image img)
		{
			img.color = m_DifficultyColors[(int)passage.CurrentDifficulty];
		});
		m_EagleImage.color = m_DifficultyEagleColors[(int)passage.CurrentDifficulty];
		if (passage.CurrentDifficulty == SectorMapPassageEntity.PassageDifficulty.Safe)
		{
			m_UpgradeButtons.FirstOrDefault()?.transform.parent.gameObject.SetActive(value: false);
			return;
		}
		for (int j = 0; j < (int)passage.CurrentDifficulty && j < 3; j++)
		{
			m_UpgradeButtons[j].gameObject.SetActive(value: true);
			int num = (int)(passage.CurrentDifficulty - j) * base.ViewModel.UpgradeWayCost.Value;
			m_UpgradeButtons[j].SetInteractable(base.ViewModel.CurrentValueOfResources.Value >= num);
			m_UpgradeCostText[j].text = num.ToString();
		}
	}

	private void HandleCostChanged()
	{
		int currentValueOfResourcesChangeCount = base.ViewModel.CurrentValueOfResourcesChangeCount;
		if (currentValueOfResourcesChangeCount < 0)
		{
			m_PlusNavigatorCostText.text = currentValueOfResourcesChangeCount.ToString() ?? "";
			m_ChangeNavigatorResourceAnimator.AppearAnimation(delegate
			{
				m_ChangeNavigatorResourceAnimator.DisappearAnimation();
			});
			Sequence s = DOTween.Sequence();
			s.Append(m_PlusNavigatorCostText.gameObject.transform.DOLocalMoveY(70f, 0.5f));
			s.AppendInterval(0.5f);
			s.Append(m_PlusNavigatorCostText.gameObject.transform.DOLocalMoveY(130f, 1f));
			s.Append(m_PlusNavigatorCostText.gameObject.transform.DOLocalMoveY(Vector3.zero.y, 0f));
			base.ViewModel.CurrentValueOfResourcesChangeCount = 0;
		}
	}

	private void ConfigurePopupDetails()
	{
	}

	protected void CreateWay(Action action)
	{
		if (base.ViewModel.CurrentValueOfResources.Value < base.ViewModel.CreateWayCost.Value)
		{
			ShowNoMoneyNoHoney(null, 1f, base.ViewModel.CreateWayCost.Value);
			return;
		}
		base.ViewModel.BlockPopups(Game.Instance.IsControllerMouse);
		base.ViewModel.IsWayCreating.Value = true;
		UISounds.Instance.Sounds.SpaceExploration.KoronusRouteBuild.Play(base.ViewModel.SectorMapObject.View.gameObject);
		m_CreateWayFillingImage.DOFillAmount(1f, 2f).OnComplete(delegate
		{
			action();
			base.ViewModel.IsWayCreating.Value = false;
		});
	}

	protected void UpgradeWay(Action onEnded, SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		SectorMapPassageEntity passage = base.ViewModel.GetPassage();
		if (passage == null)
		{
			return;
		}
		int num = (passage.CurrentDifficulty - difficulty) * base.ViewModel.UpgradeWayCost.Value;
		if (base.ViewModel.CurrentValueOfResources.Value < num)
		{
			ShowNoMoneyNoHoney(null, 1f, num);
			return;
		}
		base.ViewModel.BlockPopups(Game.Instance.IsControllerMouse);
		base.ViewModel.IsWayUpgrading.Value = true;
		PlayUpgradeDifficultySound(difficulty);
		m_UpgradeWayCostFillingImage[(int)difficulty].DOFillAmount(1f, 2f).OnComplete(delegate
		{
			base.ViewModel.SpaceSystemUpgradeWay(onEnded, difficulty);
		});
	}

	private void PlayUpgradeDifficultySound(SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		BlueprintUISound sounds = UISounds.Instance.Sounds;
		GameObject gameObject = base.ViewModel.SectorMapObject.View.gameObject;
		switch (difficulty)
		{
		case SectorMapPassageEntity.PassageDifficulty.Dangerous:
			sounds.SpaceExploration.KoronusRouteImproveToDangerous.Play(gameObject);
			break;
		case SectorMapPassageEntity.PassageDifficulty.Unsafe:
			sounds.SpaceExploration.KoronusRouteImproveToUnsafe.Play(gameObject);
			break;
		case SectorMapPassageEntity.PassageDifficulty.Safe:
			sounds.SpaceExploration.KoronusRouteImproveToSafe.Play(gameObject);
			break;
		default:
			throw new ArgumentOutOfRangeException("difficulty", difficulty, null);
		case SectorMapPassageEntity.PassageDifficulty.Deadly:
			break;
		}
	}

	private void ShowNoMoneyNoHoney(CanvasGroup panel, float interval, int needMoneyCount)
	{
		base.ViewModel.NoMoneyReaction(needMoneyCount);
		if (!(panel == null))
		{
			Sequence s = DOTween.Sequence();
			s.Append(panel.DOFade(1f, 0.5f));
			s.AppendInterval(interval);
			s.Append(panel.DOFade(0f, 0.5f));
		}
	}

	protected virtual void LockButtons(bool isLocked)
	{
	}

	protected override void DestroyViewImplementation()
	{
		m_NavigationWindowFadeAnimator.DisappearAnimation();
	}
}
