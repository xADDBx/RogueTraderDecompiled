using System;
using System.Collections.Generic;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.UI.MVVM.VM.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMap;

public class SystemMapShipTrajectoryView : ViewBase<SystemMapShipTrajectoryVM>
{
	[Header("Target Position")]
	[SerializeField]
	private CanvasGroup m_TargetPositionPoint;

	[SerializeField]
	private CanvasGroup m_TargetPingPositionPoint;

	[SerializeField]
	private Vector2 m_CorrectTargetPositionPoint;

	private RectTransform m_TargetRectTransform;

	private RectTransform m_TargetPingRectTransform;

	private RectTransform m_ParentRect;

	private IDisposable m_PingDelay;

	protected override void BindViewImplementation()
	{
		m_TargetPingPositionPoint.gameObject.SetActive(value: false);
		m_TargetRectTransform = m_TargetPositionPoint.transform as RectTransform;
		m_TargetPingRectTransform = m_TargetPingPositionPoint.transform as RectTransform;
		m_ParentRect = (RectTransform)base.transform;
		AddDisposable(base.ViewModel.IsSystemMap.Subscribe(base.gameObject.SetActive));
		AddDisposable(base.ViewModel.ShipIsMoving.Subscribe(SetTargetPoint));
		AddDisposable(base.ViewModel.ShowPingPosition.Skip(1).Subscribe(PingPosition));
	}

	private void SetTargetPoint(bool move)
	{
		m_TargetPositionPoint.alpha = (move ? 1 : 0);
		if (move && !StarSystemMapMoveController.Path.Empty())
		{
			CameraRig instance = CameraRig.Instance;
			List<Vector3> path = StarSystemMapMoveController.Path;
			Vector3 vector = instance.WorldToViewport(path[path.Count - 1]);
			Rect rect = m_ParentRect.rect;
			float x = vector.x * rect.width;
			float y = vector.y * rect.height;
			m_TargetRectTransform.anchoredPosition = new Vector2(x, y) - m_CorrectTargetPositionPoint;
		}
	}

	private void PingPosition(Vector3 position)
	{
		m_PingDelay?.Dispose();
		Vector3 vector = CameraRig.Instance.WorldToViewport(position);
		Rect rect = m_ParentRect.rect;
		float x = vector.x * rect.width;
		float y = vector.y * rect.height;
		m_TargetPingRectTransform.anchoredPosition = new Vector2(x, y) - m_CorrectTargetPositionPoint;
		m_TargetPingPositionPoint.gameObject.SetActive(value: true);
		m_PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingPositionPoint.gameObject.SetActive(value: false);
		}, 7.5f);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
