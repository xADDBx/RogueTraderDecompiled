using System.Collections.Generic;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class FxCoreHelper
{
	public static void Stop(GameObject fx)
	{
		if (!fx)
		{
			return;
		}
		List<FxDestroyOnStop> list = ListPool<FxDestroyOnStop>.Claim();
		fx.GetComponentsInChildren(list);
		foreach (FxDestroyOnStop item in list)
		{
			item.Stop();
		}
		ListPool<FxDestroyOnStop>.Release(list);
		List<ParticleSystem> list2 = ListPool<ParticleSystem>.Claim();
		fx.GetComponentsInChildren(list2);
		foreach (ParticleSystem item2 in list2)
		{
			item2.Stop();
		}
		ListPool<ParticleSystem>.Release(list2);
		List<ObjectRotator> list3 = ListPool<ObjectRotator>.Claim();
		fx.GetComponentsInChildren(list3);
		foreach (ObjectRotator item3 in list3)
		{
			item3.Stop();
		}
		ListPool<ObjectRotator>.Release(list3);
		List<RecursiveFx> list4 = ListPool<RecursiveFx>.Claim();
		fx.GetComponentsInChildren(list4);
		foreach (RecursiveFx item4 in list4)
		{
			item4.Stop();
		}
		ListPool<RecursiveFx>.Release(list4);
	}

	public static void Destroy(GameObject fx, bool immediate = false)
	{
		if (!(fx != null))
		{
			return;
		}
		FxFadeOut fxFadeOut = null;
		fxFadeOut = fx.GetComponent<FxFadeOut>();
		if (immediate || fxFadeOut == null || fxFadeOut.Duration == 0f)
		{
			GameObjectsPool.Release(fx);
		}
		else
		{
			fxFadeOut.StartFadeOut();
		}
		List<RecursiveFx> list = ListPool<RecursiveFx>.Claim();
		fx.GetComponentsInChildren(list);
		foreach (RecursiveFx item in list)
		{
			item.OnDestroyed(immediate);
		}
		ListPool<RecursiveFx>.Release(list);
	}
}
