using System;
using Kingmaker.ResourceLinks;
using Kingmaker.Sound;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class StaticHitEffects
{
	public PrefabLink HitEffectLink;

	public PrefabLink HitFXLink;

	public PrefabLink HitDecalLink;

	[SerializeField]
	private AkSwitchReference m_Switch;

	public AkSwitchReference Switch => m_Switch;

	public void PreloadResources()
	{
		HitEffectLink.Preload();
		HitFXLink?.Preload();
		HitDecalLink?.Preload();
	}
}
