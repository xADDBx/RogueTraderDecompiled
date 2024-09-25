using System;
using Kingmaker.Sound;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class HitEntry
{
	public SurfaceType Type;

	[SerializeField]
	private AkSwitchReference m_Switch;

	public StaticHitEffects StaticHitEffects;

	public CreaturesHitEffect CreaturesHitEffect;

	public AkSwitchReference Switch => m_Switch;

	public void PreloadResources()
	{
		StaticHitEffects.PreloadResources();
		CreaturesHitEffect.PreloadResources();
	}
}
