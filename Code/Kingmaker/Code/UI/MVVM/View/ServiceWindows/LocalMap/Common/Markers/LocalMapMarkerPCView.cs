using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Visual.LocalMap;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.Markers;

public class LocalMapMarkerPCView : ViewBase<LocalMapMarkerVM>
{
	private Vector2 m_Size;

	private Action m_DestroyAction;

	[NonSerialized]
	public Vector2 RealPosition;

	[SerializeField]
	private CanvasGroup m_Arrow;

	[SerializeField]
	private FadeAnimator m_TargetPingEntity;

	private IDisposable m_PingDelay;

	public void Initialize(Vector2 size, Action destroyAction)
	{
		m_Size = size;
		m_DestroyAction = destroyAction;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsVisible.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.Position.Subscribe(SetPosition));
		if (m_Arrow != null)
		{
			m_Arrow.alpha = 0f;
		}
		if (m_TargetPingEntity != null)
		{
			if (m_TargetPingEntity.CanvasGroup != null)
			{
				m_TargetPingEntity.CanvasGroup.alpha = 0f;
			}
			m_TargetPingEntity.DisappearAnimation();
		}
		AddDisposable(base.ViewModel.CoopPingEntity.Subscribe(delegate((NetPlayer player, Entity entity) value)
		{
			PingEntity(value.player, value.entity);
		}));
		AddDisposable(this.SetHint(base.ViewModel.Description.Value));
	}

	protected override void DestroyViewImplementation()
	{
		m_DestroyAction?.Invoke();
		m_DestroyAction = null;
		m_PingDelay?.Dispose();
		m_PingDelay = null;
	}

	private void SetPosition(Vector3 value)
	{
		Vector3 vector = WarhammerLocalMapRenderer.Instance.WorldToViewportPoint(value);
		base.transform.localPosition = new Vector2(m_Size.x * vector.x, m_Size.y * vector.y);
		RealPosition = base.transform.position;
	}

	public void LocalMapMarkersAlwaysInside()
	{
		SetPosition(base.ViewModel.Position.Value);
	}

	public void ShowHideArrow(bool state, Vector2 targetPos, Vector2 actualPos)
	{
		if (!(m_Arrow == null))
		{
			if (!state)
			{
				m_Arrow.alpha = 0f;
				return;
			}
			m_Arrow.alpha = 1f;
			Vector2 vector = actualPos - targetPos;
			Quaternion rotation = Quaternion.AngleAxis(Mathf.Atan2(vector.x, vector.y) * 57.29578f, Vector3.forward);
			m_Arrow.gameObject.transform.rotation = Quaternion.Inverse(rotation);
		}
	}

	public Entity GetEntity()
	{
		return base.ViewModel.GetEntity();
	}

	private void PingEntity(NetPlayer player, Entity entity)
	{
		if (!(m_TargetPingEntity == null) && entity == base.ViewModel.GetEntity())
		{
			m_PingDelay?.Dispose();
			int index = player.Index - 1;
			Image component = m_TargetPingEntity.GetComponent<Image>();
			if (component != null)
			{
				component.color = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
			}
			m_TargetPingEntity.AppearAnimation();
			m_PingDelay = DelayedInvoker.InvokeInTime(delegate
			{
				m_TargetPingEntity.DisappearAnimation();
			}, 7.5f);
		}
	}
}
