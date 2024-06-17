using System;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class ShieldHitEntry
{
	public ShieldType Type;

	public bool ShowEntityBlood;

	public bool ShowSpecialEffect;

	public GameObject SpecialEffect;

	public bool ShowImpact;

	public bool HitInSphere;

	public float SphereRadius;

	public bool ShowHitAnimation;
}
