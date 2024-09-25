using System;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

[Serializable]
public class WarpPassageVisualParameters
{
	public Gradient SafeColor;

	public Gradient UnsafeColor;

	public Gradient DangerousColor;

	public Gradient DeadlyColor;

	public Material SafeMaterial;

	public Material UnsafeMaterial;

	public Material DangerousMaterial;

	public Material DeadlyMaterial;

	public Gradient GetColor(SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		return difficulty switch
		{
			SectorMapPassageEntity.PassageDifficulty.Safe => SafeColor, 
			SectorMapPassageEntity.PassageDifficulty.Unsafe => UnsafeColor, 
			SectorMapPassageEntity.PassageDifficulty.Dangerous => DangerousColor, 
			SectorMapPassageEntity.PassageDifficulty.Deadly => DeadlyColor, 
			_ => null, 
		};
	}

	public Material GetMaterial(SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		return difficulty switch
		{
			SectorMapPassageEntity.PassageDifficulty.Safe => SafeMaterial, 
			SectorMapPassageEntity.PassageDifficulty.Unsafe => UnsafeMaterial, 
			SectorMapPassageEntity.PassageDifficulty.Dangerous => DangerousMaterial, 
			SectorMapPassageEntity.PassageDifficulty.Deadly => DeadlyMaterial, 
			_ => null, 
		};
	}
}
