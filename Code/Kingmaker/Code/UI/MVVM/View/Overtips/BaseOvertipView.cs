using Cinemachine;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips;

public abstract class BaseOvertipView<TViewModel> : ViewBase<TViewModel> where TViewModel : OvertipEntityVM
{
	protected bool IsOnScreen;

	private static Rect s_CanvasRect = new Rect(-0.2f, -0.2f, 1.2f, 1.2f);

	private RectTransform m_OwnRectTransform;

	private Vector2 m_OvertipPosition;

	protected Vector2 PositionCorrectionFromView;

	protected Vector2 PositionCorrection;

	private CanvasGroup m_CanvasGroup;

	private Rect m_ParentRect;

	private int m_LastScreenWidth = -1;

	private int m_LastScreenHeight = -1;

	protected abstract bool CheckVisibility { get; }

	protected CanvasGroup CanvasGroup
	{
		get
		{
			if (!(m_CanvasGroup == null))
			{
				return m_CanvasGroup;
			}
			return m_CanvasGroup = this.EnsureComponent<CanvasGroup>();
		}
	}

	private void OnEnable()
	{
		SetCanvasGroupVisible(isVisible: false);
	}

	protected override void BindViewImplementation()
	{
		UpdateParentRect();
		SetCanvasGroupVisible(isVisible: false);
		m_OwnRectTransform = (RectTransform)base.transform;
		RectTransform ownRectTransform = m_OwnRectTransform;
		Vector2 anchorMin = (m_OwnRectTransform.anchorMax = Vector2.zero);
		ownRectTransform.anchorMin = anchorMin;
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			InternalUpdate();
		}));
		CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineUpdate);
	}

	protected override void DestroyViewImplementation()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineUpdate);
		base.gameObject.name = base.gameObject.name + " [deactivated]";
		if (m_CanvasGroup != null)
		{
			SetActiveInternal(isActive: false);
		}
	}

	private void OnCinemachineUpdate(CinemachineBrain arg0)
	{
		InternalUpdate();
	}

	protected virtual void UpdateActive(bool isActive)
	{
	}

	private void InternalUpdate()
	{
		if (base.ViewModel == null)
		{
			PFLog.UI.Error(base.gameObject.name + ": ViewModel == null, but View are still not Destroyed");
			return;
		}
		if (!CheckVisibility)
		{
			SetCanvasGroupVisible(isVisible: false);
			return;
		}
		Vector3? currentCanvasPosition = GetCurrentCanvasPosition();
		IsOnScreen = currentCanvasPosition.HasValue;
		bool flag = IsOnScreen && CheckVisibility;
		SetActiveInternal(flag);
		UpdateActive(flag);
		if (flag)
		{
			UpdatePosition(currentCanvasPosition.Value);
		}
	}

	protected virtual void SetActiveInternal(bool isActive)
	{
		SetCanvasGroupVisible(isActive);
	}

	private void SetCanvasGroupVisible(bool isVisible)
	{
		CanvasGroup.alpha = (isVisible ? 1f : 0f);
		CanvasGroup.blocksRaycasts = isVisible;
	}

	private void UpdateParentRect()
	{
		m_ParentRect = ((RectTransform)base.transform.parent.parent).rect;
		m_LastScreenWidth = Screen.width;
		m_LastScreenHeight = Screen.height;
	}

	private void UpdatePosition(Vector3 canvasPosition)
	{
		if (Screen.width != m_LastScreenWidth || Screen.height != m_LastScreenHeight)
		{
			UpdateParentRect();
		}
		PositionCorrection = PositionCorrectionFromView + new Vector2(0f, base.ViewModel.OvertipVerticalCorrection);
		m_OvertipPosition = UIUtilityGetRect.PixelPositionInRect(m_ParentRect, canvasPosition, m_OwnRectTransform.parent);
		m_OwnRectTransform.anchoredPosition = m_OvertipPosition + PositionCorrection;
	}

	private Vector3? GetCurrentCanvasPosition()
	{
		Vector3 objectPositionInCamera = UIUtilityGetRect.GetObjectPositionInCamera(base.ViewModel.Position.Value);
		if (s_CanvasRect.Contains(objectPositionInCamera))
		{
			return objectPositionInCamera;
		}
		return null;
	}
}
