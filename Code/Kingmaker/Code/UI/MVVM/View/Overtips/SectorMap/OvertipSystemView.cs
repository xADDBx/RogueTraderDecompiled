using DG.Tweening;
using Kingmaker.Code.UI.Common;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.UI.Common.Animations;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap;

public abstract class OvertipSystemView : BaseOvertipView<OvertipEntitySystemVM>
{
	[Header("NavigatorResource")]
	[SerializeField]
	private TextMeshProUGUI m_PlusNavigatorResourceObject;

	[SerializeField]
	private FadeAnimator m_PlusNavigatorResourceAimator;

	[FormerlySerializedAs("m_ResourceBlockPCView")]
	[Header("Resources")]
	[SerializeField]
	private OvertipSystemResourceBlockView m_ResourceBlockView;

	[Header("Views")]
	[SerializeField]
	private Image m_ColonizedIcon;

	[SerializeField]
	private Image m_QuestIcon;

	[SerializeField]
	private Image m_RumourIcon;

	[SerializeField]
	private TextMeshProUGUI m_SystemName;

	[SerializeField]
	protected FadeAnimator m_CircleBackgroundAnimator;

	[SerializeField]
	protected FadeAnimator m_CircleBackgroundMiniAnimator;

	protected override bool CheckVisibility => base.ViewModel.IsExplored.Value;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CircleBackgroundMiniAnimator.DisappearAnimation();
		AddDisposable(base.ViewModel.IsVisitAvailable.AsObservable().Subscribe(delegate(bool state)
		{
			if (state)
			{
				m_CircleBackgroundAnimator.AppearAnimation();
			}
			else
			{
				m_CircleBackgroundAnimator.DisappearAnimation();
			}
		}));
		AddDisposable(base.ViewModel.IsScanning.CombineLatest(base.ViewModel.IsTraveling, base.ViewModel.IsDialogActive, (bool isScanning, bool isTraveling, bool isDialogActive) => isScanning || isTraveling || isDialogActive).Subscribe(LockButtons));
		base.name = base.ViewModel.SectorMapObject.View.name + "_Overtip";
		AddDisposable(base.ViewModel.SectorResourceBlockVM.Subscribe(m_ResourceBlockView.Bind));
		AddDisposable(base.ViewModel.CurrentValueOfResources.Subscribe(delegate(int v)
		{
			if (v > 0)
			{
				HandleCostChanged();
			}
		}));
		CheckIconsState();
	}

	protected void ShowHideCircleBackground(bool state)
	{
		if (state)
		{
			if (base.ViewModel.IsCurrentSystem.Value)
			{
				m_CircleBackgroundAnimator.AppearAnimation();
				m_CircleBackgroundMiniAnimator.DisappearAnimation();
			}
			else
			{
				m_CircleBackgroundMiniAnimator.AppearAnimation();
				m_CircleBackgroundAnimator.DisappearAnimation();
			}
		}
		else if (base.ViewModel.SpaceSystemNavigationButtonsVM.Value == null)
		{
			m_CircleBackgroundAnimator.DisappearAnimation();
			m_CircleBackgroundMiniAnimator.DisappearAnimation();
		}
	}

	private void CheckIconsState()
	{
		m_ColonizedIcon.gameObject.SetActive(base.ViewModel.CheckColonization());
		if (m_ColonizedIcon.gameObject.activeSelf)
		{
			AddDisposable(m_ColonizedIcon.SetHint(UIUtilitySpaceColonization.GetColonizationInformation(base.ViewModel.Colony.Value)));
		}
		m_QuestIcon.gameObject.SetActive(base.ViewModel.CheckQuests());
		if (m_QuestIcon.gameObject.activeSelf)
		{
			AddDisposable(m_QuestIcon.SetHint(base.ViewModel.QuestObjectiveName.Value));
		}
		m_RumourIcon.gameObject.SetActive(base.ViewModel.CheckRumours());
		if (m_RumourIcon.gameObject.activeSelf)
		{
			AddDisposable(m_RumourIcon.SetHint(base.ViewModel.RumourObjectiveName.Value));
		}
	}

	protected void HandleVisitDialogButtonClick()
	{
		if (!base.ViewModel.CheckPingCoop() && base.ViewModel.IsVisitAvailable.Value)
		{
			base.ViewModel.ShowVisitDialogBox(base.ViewModel.SectorMapObject, base.ViewModel.SectorMapObject.IsVisited);
		}
	}

	protected void HandleTradeButtonClick()
	{
		if (base.ViewModel.IsTradeActive.Value)
		{
			base.ViewModel.StopTrade();
		}
		else
		{
			base.ViewModel.StartTrade();
		}
	}

	protected void PlusNavigatorResetPosition()
	{
		m_PlusNavigatorResourceObject.gameObject.transform.position = Vector3.zero;
	}

	protected virtual void LockButtons(bool isLocked)
	{
	}

	public void ClosePopup(bool withCircle = true)
	{
		if (Game.Instance?.SectorMapController?.CurrentStarSystem != null && base.ViewModel?.SectorMapObject != Game.Instance?.SectorMapController?.CurrentStarSystem)
		{
			base.ViewModel?.CloseSpaceSystemPopup();
			if (withCircle)
			{
				m_CircleBackgroundAnimator.DisappearAnimation();
				m_CircleBackgroundMiniAnimator.DisappearAnimation();
			}
		}
	}

	private void HandleCostChanged()
	{
		int currentValueOfResourcesChangeCount = base.ViewModel.CurrentValueOfResourcesChangeCount;
		if (currentValueOfResourcesChangeCount > 0)
		{
			m_PlusNavigatorResourceObject.text = "+" + currentValueOfResourcesChangeCount;
			m_PlusNavigatorResourceAimator.AppearAnimation(delegate
			{
				m_PlusNavigatorResourceAimator.DisappearAnimation();
			});
			Sequence s = DOTween.Sequence();
			s.Append(m_PlusNavigatorResourceObject.gameObject.transform.DOLocalMoveY(20f, 0.5f));
			s.AppendInterval(0.5f);
			s.Append(m_PlusNavigatorResourceObject.gameObject.transform.DOLocalMoveY(70f, 1f));
			s.Append(m_PlusNavigatorResourceObject.gameObject.transform.DOLocalMoveY(Vector3.zero.y, 0f));
			base.ViewModel.CurrentValueOfResourcesChangeCount = 0;
		}
	}

	public bool IsCurrentSystem()
	{
		return base.ViewModel?.IsCurrentSystem.Value ?? false;
	}

	public SectorMapPassageEntity GetPassage()
	{
		return Game.Instance.SectorMapController.FindPassageBetween(Game.Instance.SectorMapController.CurrentStarSystem, base.ViewModel.SectorMapObject);
	}
}
