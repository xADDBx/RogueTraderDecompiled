using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.GameModes;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View;

public class EntityFader : LateUpdateableBehaviour
{
	[SerializeField]
	[Tooltip("Those Renderers will be unaffected by Fader logic. Use with caution!")]
	private List<Renderer> m_ExcludeRenderersList = new List<Renderer>(0);

	public Action<bool> OnVisibleChangedEvent = delegate
	{
	};

	private bool m_Visible;

	private StandardMaterialController m_MaterialController;

	private DissolveSettings m_FadeAnimation;

	private EntityViewBase m_CachedEntity;

	private bool m_NeedHideRenderers;

	private Coroutine m_CompleteEventRoutine;

	public bool AnimationDisabled { get; private set; }

	public EntityViewBase Entity
	{
		get
		{
			if ((bool)m_CachedEntity)
			{
				return m_CachedEntity;
			}
			return m_CachedEntity = GetComponent<EntityViewBase>();
		}
	}

	public bool Visible
	{
		get
		{
			return m_Visible;
		}
		set
		{
			if (m_Visible != value)
			{
				if (Game.Instance.CurrentMode == GameModeType.Cutscene)
				{
					DisableAnimation();
				}
				else
				{
					AnimationDisabled = false;
				}
				if (AnimationDisabled)
				{
					m_Visible = value;
					SetRendererVisibility(value);
					OnVisibleChangedEvent(value);
					return;
				}
				if (m_FadeAnimation.IsFinished)
				{
					m_FadeAnimation.Reset();
					m_FadeAnimation.Lifetime = Game.Instance.BlueprintRoot.Prefabs.FogOfWarDissolveSettings.Lifetime;
				}
				else if (m_FadeAnimation.PlayBackwards != value)
				{
					m_FadeAnimation.StartTime = Time.unscaledTime - (1f - m_FadeAnimation.NormalizedTime) * m_FadeAnimation.Lifetime;
					m_FadeAnimation.NormalizedTime = 1f - m_FadeAnimation.NormalizedTime;
				}
				m_FadeAnimation.PlayBackwards = value;
				if (!m_MaterialController.DissolveController.Animations.Contains(m_FadeAnimation))
				{
					m_MaterialController.DissolveController.Animations.Add(m_FadeAnimation);
				}
				m_Visible = value;
				SetRendererVisibility(visible: true);
				m_NeedHideRenderers = !m_Visible;
				float num = m_FadeAnimation.Lifetime + 0.5f;
				ClearRoutine();
				m_CompleteEventRoutine = CoroutineRunner.Start(CompleteCallback(num));
				(Entity as UnitEntityView)?.EntityData.Wake(num);
			}
			else
			{
				ClearRoutine();
				OnVisibleChangedEvent(value);
			}
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		if (m_MaterialController == null)
		{
			m_MaterialController = base.gameObject.EnsureComponent<StandardMaterialController>();
		}
		DissolveSettings fogOfWarDissolveSettings = Game.Instance.BlueprintRoot.Prefabs.FogOfWarDissolveSettings;
		m_FadeAnimation = m_FadeAnimation ?? new DissolveSettings
		{
			Texture = fogOfWarDissolveSettings.Texture,
			TilingScale = fogOfWarDissolveSettings.TilingScale,
			TilingOverride = fogOfWarDissolveSettings.TilingOverride,
			OffsetSpeed = fogOfWarDissolveSettings.OffsetSpeed,
			Lifetime = fogOfWarDissolveSettings.Lifetime,
			DissolveOverLifetime = fogOfWarDissolveSettings.DissolveOverLifetime,
			DissolveWidthOverLifetime = fogOfWarDissolveSettings.DissolveWidthOverLifetime,
			DissolveWidthScale = fogOfWarDissolveSettings.DissolveWidthScale,
			ColorOverLifetime = fogOfWarDissolveSettings.ColorOverLifetime,
			HdrColorScale = fogOfWarDissolveSettings.HdrColorScale,
			DissolveEmission = false,
			Layer = 10000,
			UseUnscaledTime = true
		};
	}

	public void DisableAnimation()
	{
		AnimationDisabled = true;
		ClearRoutine();
		m_NeedHideRenderers = false;
	}

	private void ClearRoutine()
	{
		if (m_CompleteEventRoutine != null)
		{
			CoroutineRunner.Stop(m_CompleteEventRoutine);
			m_CompleteEventRoutine = null;
		}
	}

	private IEnumerator CompleteCallback(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_CompleteEventRoutine = null;
		OnVisibleChangedEvent(m_Visible);
	}

	public void FastForward()
	{
		m_MaterialController.DissolveController.Animations.Remove(m_FadeAnimation);
		m_FadeAnimation.IsFinished = true;
		m_MaterialController.DissolveController.Update();
		SetRendererVisibility(Visible);
	}

	public void Force()
	{
		if (m_FadeAnimation == null || (m_FadeAnimation.IsStarted && !m_FadeAnimation.IsFinished))
		{
			SetRendererVisibility(visible: true);
			return;
		}
		m_MaterialController.DissolveController.Animations.Remove(m_FadeAnimation);
		SetRendererVisibility(m_Visible);
		m_NeedHideRenderers = false;
	}

	private void SetRendererVisibility(bool visible)
	{
		foreach (Renderer renderer in Entity.Renderers)
		{
			if ((m_ExcludeRenderersList.Count <= 0 || !m_ExcludeRenderersList.Contains(renderer)) && (bool)renderer)
			{
				renderer.enabled = visible;
			}
		}
		foreach (Behaviour vfxBehaviour in Entity.VfxBehaviours)
		{
			if ((bool)vfxBehaviour)
			{
				vfxBehaviour.enabled = visible;
			}
		}
	}

	public override void DoLateUpdate()
	{
		if (!AnimationDisabled && m_FadeAnimation.IsFinished && m_NeedHideRenderers)
		{
			SetRendererVisibility(m_Visible);
			m_NeedHideRenderers = false;
		}
	}
}
