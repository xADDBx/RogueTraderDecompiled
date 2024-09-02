using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

[AddComponentMenu("")]
public class OcclusionGeometryClipLinkEntityProxy : OcclusionGeometryClipLinkProxy
{
	private DissolveSettings m_FadeAnimation;

	private bool m_FadeAnimationActive;

	public override void SetOpacity(float value)
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
		m_FadeAnimation.Reset();
		if (!base.gameObject.TryGetComponent<StandardMaterialController>(out var component))
		{
			component = base.gameObject.AddComponent<StandardMaterialController>();
		}
		component.DissolveController.Animations.Add(m_FadeAnimation);
	}

	private void RemoveFadeAnimation()
	{
		if (base.gameObject.TryGetComponent<StandardMaterialController>(out var component))
		{
			component.DissolveController.Animations.Remove(m_FadeAnimation);
		}
	}
}
