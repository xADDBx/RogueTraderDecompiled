using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SystemMap;

public class SystemMapShipTrajectoryView : ViewBase<SystemMapShipTrajectoryVM>
{
	[Serializable]
	private class TargetPingPositionPoint
	{
		public CanvasGroup MainCanvasGroup;

		public List<Image> AdditionalImages;
	}

	private class PingData
	{
		public IDisposable PingDelay { get; set; }
	}

	[Header("Target Position")]
	[SerializeField]
	private CanvasGroup m_TargetPositionPoint;

	[SerializeField]
	private List<TargetPingPositionPoint> m_TargetPingPositionPoints = new List<TargetPingPositionPoint>();

	[SerializeField]
	private Vector2 m_CorrectTargetPositionPoint;

	private RectTransform m_TargetRectTransform;

	private readonly RectTransform[] m_TargetPingRectTransforms = new RectTransform[6];

	private RectTransform m_ParentRect;

	private readonly Dictionary<NetPlayer, PingData> m_PlayerPingData = new Dictionary<NetPlayer, PingData>();

	protected override void BindViewImplementation()
	{
		m_TargetPingPositionPoints.ForEach(delegate(TargetPingPositionPoint p)
		{
			p.MainCanvasGroup.gameObject.SetActive(value: false);
		});
		m_TargetRectTransform = m_TargetPositionPoint.transform as RectTransform;
		for (int i = 0; i < m_TargetPingRectTransforms.Length; i++)
		{
			m_TargetPingRectTransforms[i] = m_TargetPingPositionPoints[i].MainCanvasGroup.transform as RectTransform;
		}
		for (int j = 0; j < m_TargetPingPositionPoints.Count && BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors.Count >= j; j++)
		{
			Image component = m_TargetPingPositionPoints[j].MainCanvasGroup.GetComponent<Image>();
			if (component != null)
			{
				component.color = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[j];
			}
			foreach (Image additionalImage in m_TargetPingPositionPoints[j].AdditionalImages)
			{
				additionalImage.color = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[j];
			}
		}
		m_ParentRect = (RectTransform)base.transform;
		AddDisposable(base.ViewModel.IsSystemMap.Subscribe(base.gameObject.SetActive));
		AddDisposable(base.ViewModel.ShipIsMoving.Subscribe(SetTargetPoint));
		AddDisposable(base.ViewModel.ShowPingPosition.Subscribe(delegate((NetPlayer player, Vector3 position) value)
		{
			PingPosition(value.player, value.position);
		}));
	}

	protected override void DestroyViewImplementation()
	{
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

	private void PingPosition(NetPlayer player, Vector3 position)
	{
		int playerIndex = player.Index - 1;
		if (m_PlayerPingData.ContainsKey(player))
		{
			m_PlayerPingData[player].PingDelay?.Dispose();
		}
		else
		{
			m_PlayerPingData[player] = new PingData();
		}
		Vector3 vector = CameraRig.Instance.WorldToViewport(position);
		PingData pingData = m_PlayerPingData[player];
		Rect rect = m_ParentRect.rect;
		float x = vector.x * rect.width;
		float y = vector.y * rect.height;
		m_TargetPingRectTransforms[playerIndex].anchoredPosition = new Vector2(x, y) - m_CorrectTargetPositionPoint;
		m_TargetPingPositionPoints[playerIndex].MainCanvasGroup.gameObject.SetActive(value: true);
		EventBus.RaiseEvent(delegate(INetPingPosition h)
		{
			h.HandlePingPositionSound(m_TargetPingPositionPoints[playerIndex].MainCanvasGroup.gameObject);
		});
		pingData.PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingPositionPoints[playerIndex].MainCanvasGroup.gameObject.SetActive(value: false);
		}, 7.5f);
	}
}
