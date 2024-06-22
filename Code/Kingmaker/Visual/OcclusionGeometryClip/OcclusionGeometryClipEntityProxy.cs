using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

[AddComponentMenu("")]
public class OcclusionGeometryClipEntityProxy : MonoBehaviour
{
	private StandardMaterialController m_StandardMaterialController;

	private DissolveSettings m_FadeAnimation;

	private bool m_FadeAnimationActive;

	internal OcclusionGeometryClipEntityAreaProxy LinkedArea;

	private void Start()
	{
		OcclusionGeometryClipEntitySystem.AddEntity(this);
	}

	private void OnDestroy()
	{
		OcclusionGeometryClipEntitySystem.RemoveEntity(this);
	}

	public void SetOpacity(float value)
	{
		SetFadeAnimationActive(value < 0.5f);
	}

	private void SetFadeAnimationActive(bool value)
	{
		if (m_FadeAnimationActive != value)
		{
			if (value)
			{
				AddFadeAnimation();
			}
			else
			{
				RemoveFadeAnimation();
			}
			m_FadeAnimationActive = value;
		}
	}

	private void AddFadeAnimation()
	{
		if (m_FadeAnimation == null)
		{
			DissolveSettings fogOfWarDissolveSettings = Game.Instance.BlueprintRoot.Prefabs.FogOfWarDissolveSettings;
			m_FadeAnimation = new DissolveSettings
			{
				Texture = fogOfWarDissolveSettings.Texture,
				TilingScale = fogOfWarDissolveSettings.TilingScale,
				TilingOverride = fogOfWarDissolveSettings.TilingOverride,
				DissolveWidthOverLifetime = fogOfWarDissolveSettings.DissolveWidthOverLifetime,
				DissolveWidthScale = fogOfWarDissolveSettings.DissolveWidthScale,
				ColorOverLifetime = fogOfWarDissolveSettings.ColorOverLifetime,
				HdrColorScale = fogOfWarDissolveSettings.HdrColorScale,
				Lifetime = 1f,
				DissolveOverLifetime = AnimationCurve.Constant(0f, 1f, 1f),
				DissolveEmission = false,
				Layer = 100000,
				UseUnscaledTime = true,
				LoopAnimation = true
			};
		}
		if (m_StandardMaterialController == null)
		{
			m_StandardMaterialController = base.gameObject.AddComponent<StandardMaterialController>();
		}
		m_FadeAnimation.Reset();
		m_StandardMaterialController.DissolveController.Animations.Add(m_FadeAnimation);
	}

	private void RemoveFadeAnimation()
	{
		if ((bool)m_StandardMaterialController)
		{
			m_StandardMaterialController.DissolveController.Animations.Remove(m_FadeAnimation);
		}
	}
}
