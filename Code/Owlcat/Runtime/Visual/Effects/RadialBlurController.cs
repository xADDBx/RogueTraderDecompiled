using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.RenderPipeline;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects;

[RequireComponent(typeof(Camera))]
public class RadialBlurController : MonoBehaviour
{
	private Camera m_Camera;

	private bool m_Resetted;

	public List<RadialBlurSettings> Animations = new List<RadialBlurSettings>();

	public static RadialBlurController Instance { get; private set; }

	private void OnEnable()
	{
		Instance = this;
		m_Camera = GetComponent<Camera>();
		OwlcatRenderPipeline.VolumeManagerUpdated = (Action<Camera>)Delegate.Combine(OwlcatRenderPipeline.VolumeManagerUpdated, new Action<Camera>(DoUpdate));
	}

	private void OnDisable()
	{
		OwlcatRenderPipeline.VolumeManagerUpdated = (Action<Camera>)Delegate.Remove(OwlcatRenderPipeline.VolumeManagerUpdated, new Action<Camera>(DoUpdate));
	}

	private void DoUpdate(Camera camera)
	{
		RadialBlurSettings radialBlurSettings = null;
		for (int i = 0; i < Animations.Count; i++)
		{
			RadialBlurSettings radialBlurSettings2 = Animations[i];
			if (!radialBlurSettings2.IsFinished)
			{
				UpdateAnimation(radialBlurSettings2);
			}
			if (radialBlurSettings2.IsFinished)
			{
				Animations.RemoveAt(i);
				i--;
			}
			else if (!radialBlurSettings2.IsDelayed && radialBlurSettings2.Layer >= (radialBlurSettings?.Layer ?? int.MinValue))
			{
				radialBlurSettings = radialBlurSettings2;
			}
		}
		if (radialBlurSettings != null)
		{
			RadialBlur component = VolumeManager.instance.stack.GetComponent<RadialBlur>();
			component.Center.overrideState = true;
			component.Center.value = radialBlurSettings.CurrentCenter;
			component.Width.overrideState = true;
			component.Width.value = radialBlurSettings.CurrentWidth;
			component.Strength.overrideState = true;
			component.Strength.value = radialBlurSettings.CurrentStrength;
			m_Resetted = false;
		}
		else if (!m_Resetted)
		{
			m_Resetted = true;
			RadialBlur component2 = VolumeManager.instance.stack.GetComponent<RadialBlur>();
			component2.Center.overrideState = false;
			component2.Width.value = 0f;
			component2.Width.overrideState = false;
			component2.Strength.value = 0f;
			component2.Strength.overrideState = false;
		}
	}

	private void UpdateAnimation(RadialBlurSettings animation)
	{
		if (!animation.IsStarted)
		{
			animation.IsStarted = true;
			animation.StartTime = Time.time + animation.Delay;
		}
		if (animation.Lifetime <= 0f || (!animation.LoopAnimation && animation.NormalizedTime > 1f))
		{
			animation.IsFinished = true;
			return;
		}
		animation.NormalizedTime = (Time.time - animation.StartTime) / animation.Lifetime;
		if (!animation.IsDelayed)
		{
			if (animation.LoopAnimation)
			{
				animation.NormalizedTime -= Mathf.Floor(animation.NormalizedTime);
			}
			Vector3 position = animation.TargetPosition;
			if (animation.TargetObject != null)
			{
				position = animation.TargetObject.position;
			}
			position = m_Camera.WorldToViewportPoint(position);
			float value = Vector2.Distance(position, new Vector2(0.5f, 0.5f)) * 2f;
			float time = Mathf.Clamp(value, 0f, animation.StrengthOverDistance.GetDuration());
			float time2 = Mathf.Clamp(value, 0f, animation.WidthOverDistance.GetDuration());
			animation.CurrentStrength = animation.StrengthOverLifetime.Evaluate(animation.NormalizedTime) * animation.StrengthMultiplier * animation.StrengthOverDistance.Evaluate(time);
			animation.CurrentWidth = animation.WidthOverLifetime.Evaluate(animation.NormalizedTime) * animation.WidthMultiplier * animation.WidthOverDistance.Evaluate(time2);
			animation.CurrentCenter = position;
		}
	}
}
