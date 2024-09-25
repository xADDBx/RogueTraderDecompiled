using System;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class CreaturesHitEffect
{
	public PrefabLink BloodPuddleLink;

	public PrefabLink DismemberLink;

	public PrefabLink DismemberLootLink;

	public PrefabLink RipLimbsApartLink;

	public HitCollection Billboard;

	public HitCollection Directional;

	public HitCollection BillboardAdditive;

	public Color BloodColor;

	public Color DeadBloodColor;

	public Texture2D BloodTexture;

	public Vector2 DefaultTileSize = Vector2.one;

	[Tooltip("How TRANSPARENT blood splatter is depending on health percent. 0% health = left-side value. \nDefault: [0.09]..[0.2224]")]
	public AnimationCurve BloodFadeout = AnimationCurve.Linear(0f, 0.09f, 1f, 0.2224f);

	public PrefabLink BleedingLink;

	public void PreloadResources()
	{
		BloodPuddleLink.Preload();
		DismemberLink.Preload();
		DismemberLootLink.Preload();
		RipLimbsApartLink.Preload();
		Billboard.PreloadResources();
		Directional.PreloadResources();
		BillboardAdditive.PreloadResources();
		BleedingLink?.Preload();
	}
}
