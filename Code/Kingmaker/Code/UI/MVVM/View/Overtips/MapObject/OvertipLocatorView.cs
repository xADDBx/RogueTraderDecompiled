using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public class OvertipLocatorView : BaseOvertipView<OvertipLocatorVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Common Block")]
	[SerializeField]
	private float m_FarDistance = 120f;

	[SerializeField]
	private float m_StandardOvertipPositionYCorrection = 30f;

	[Header("Bark")]
	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	private readonly ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private List<Graphic> m_AdditionalRaycastBlockers = new List<Graphic>();

	private List<Graphic> m_MainRaycastBlockers = new List<Graphic>();

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	private IDisposable m_HoverDelay;

	protected override bool CheckVisibility
	{
		get
		{
			if (base.ViewModel.IsVisibleForPlayer.Value)
			{
				return !base.ViewModel.LocatorEntity.Suppressed;
			}
			return false;
		}
	}

	private bool CheckVisibleTrigger => base.ViewModel.IsBarkActive.Value;

	private void Awake()
	{
		List<DoNotDisableRaycasts> list = new List<DoNotDisableRaycasts>();
		GetComponentsInChildren(includeInactive: true, list);
		m_MainRaycastBlockers = list.Select((DoNotDisableRaycasts c) => c.GetComponent<Graphic>()).ToList();
		GetComponentsInChildren(includeInactive: true, m_AdditionalRaycastBlockers);
		m_AdditionalRaycastBlockers = m_AdditionalRaycastBlockers.Where((Graphic g) => g.raycastTarget && g.GetComponent<DoNotDisableRaycasts>() == null).ToList();
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
	}

	private void EnableMainRaycasts(bool enable)
	{
		foreach (Graphic mainRaycastBlocker in m_MainRaycastBlockers)
		{
			mainRaycastBlocker.raycastTarget = enable;
		}
	}

	private void EnableAdditionalRaycasts(bool enable)
	{
		foreach (Graphic additionalRaycastBlocker in m_AdditionalRaycastBlockers)
		{
			additionalRaycastBlocker.raycastTarget = enable;
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.gameObject.name = base.ViewModel.LocatorEntity.View.GameObjectName + "_LocatorOvertip";
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		AddDisposable(base.ViewModel.IsBarkActive.CombineLatest(base.ViewModel.CameraDistance, (bool _, Vector3 _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		AddDisposable(m_Visibility.Subscribe(DoVisibility));
		EnableMainRaycasts(enable: true);
	}

	private void DoVisibility(UnitOvertipVisibility unitOvertipVisibility)
	{
		if (unitOvertipVisibility == UnitOvertipVisibility.NotFull || unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized)
		{
			base.transform.SetAsLastSibling();
		}
		PositionCorrectionFromView = new Vector2(0f, 0f - m_StandardOvertipPositionYCorrection);
	}

	private void UpdateVisibility()
	{
		if (!CheckVisibility || base.ViewModel.IsCutscene || base.ViewModel.IsInDialog)
		{
			m_Visibility.Value = UnitOvertipVisibility.Invisible;
			return;
		}
		bool flag = base.ViewModel.CameraDistance.Value.sqrMagnitude < m_FarDistance;
		if (CheckVisibleTrigger)
		{
			m_Visibility.Value = (flag ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
		}
		else
		{
			m_Visibility.Value = ((!flag) ? UnitOvertipVisibility.Far : UnitOvertipVisibility.Near);
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator?.Kill();
		m_FadeAnimator = null;
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = null;
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
		base.DestroyViewImplementation();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.IsBinded)
		{
			m_HoverDelay?.Dispose();
			m_HoverDelay = DelayedInvoker.InvokeInTime(delegate
			{
				m_Visibility.Value = UnitOvertipVisibility.Maximized;
			}, 0.2f);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.IsBinded)
		{
			m_HoverDelay?.Dispose();
			m_HoverDelay = DelayedInvoker.InvokeInTime(UpdateVisibility, 1f);
		}
	}
}
