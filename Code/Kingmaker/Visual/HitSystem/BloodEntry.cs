using System;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class BloodEntry
{
	public BloodType Type;

	public PrefabLink BloodPuddleLink;

	public PrefabLink DismemberLink;

	public PrefabLink InPowerDismemberLink;

	public PrefabLink DismemberLootLink;

	public PrefabLink RipLimbsApartLink;

	public HitCollection Billboard;

	public HitCollection Directional;

	public HitCollection BillboardAdditive;

	public HitCollection DirectionalAdditive;

	public Color BloodColor;

	public Texture2D BloodTexture;

	public void PreloadResources()
	{
		BloodPuddleLink.Preload();
		DismemberLink.Preload();
		InPowerDismemberLink.Preload();
		DismemberLootLink.Preload();
		RipLimbsApartLink.Preload();
		Billboard.PreloadResources();
		Directional.PreloadResources();
		BillboardAdditive.PreloadResources();
		DirectionalAdditive.PreloadResources();
	}
}
