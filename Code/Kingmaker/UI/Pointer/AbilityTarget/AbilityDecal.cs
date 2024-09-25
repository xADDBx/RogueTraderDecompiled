using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Kingmaker.Visual.Decals;
using UnityEngine;

namespace Kingmaker.UI.Pointer.AbilityTarget;

public static class AbilityDecal
{
	public static void SetVisible(this ScreenSpaceDecalGroup group, bool visible, float duration)
	{
		DOTween.Pause(group);
		DOTween.Kill(group);
		TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(() => group.GroupAlpha, delegate(float a)
		{
			group.GroupAlpha = a;
		}, visible ? 1 : 0, duration);
		tweenerCore.SetAutoKill();
		if (visible)
		{
			group.SetActive(active: true);
		}
		else
		{
			tweenerCore.OnComplete(delegate
			{
				group.SetActive(active: false);
			});
		}
		tweenerCore.SetTarget(group);
		tweenerCore.SetUpdate(isIndependentUpdate: true);
	}

	public static void SetActive(this ScreenSpaceDecalGroup group, bool active)
	{
		if ((bool)group.gameObject)
		{
			group.gameObject.SetActive(active);
		}
	}

	public static void Kill(this ScreenSpaceDecalGroup group, float duration, Action onKill)
	{
		DOTween.Pause(group);
		DOTween.Kill(group);
		TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(() => group.GroupAlpha, delegate(float a)
		{
			group.GroupAlpha = a;
		}, 0f, duration);
		tweenerCore.SetAutoKill();
		if ((bool)group.gameObject)
		{
			tweenerCore.OnComplete(delegate
			{
				UnityEngine.Object.Destroy(group.gameObject);
				if (onKill != null)
				{
					onKill();
				}
			});
		}
		tweenerCore.SetTarget(group);
		tweenerCore.SetUpdate(isIndependentUpdate: false);
	}
}
