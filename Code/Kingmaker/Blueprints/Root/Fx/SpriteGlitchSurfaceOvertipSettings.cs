using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Fx;

[Serializable]
public class SpriteGlitchSurfaceOvertipSettings
{
	[SerializeField]
	private Material m_MaterialSource;

	public static List<Material> MaterialsPool;

	public const int NumberOfMaterials = 25;

	private const int LowGlitchIndex = 10;

	public SpriteGlitchSurfaceOvertipProperty GlitchIntervalTime = new SpriteGlitchSurfaceOvertipProperty
	{
		PropertyName = "_GlitchInterval"
	};

	public SpriteGlitchSurfaceOvertipProperty DisplacementGlitchProbability = new SpriteGlitchSurfaceOvertipProperty
	{
		PropertyName = "_DispProbability"
	};

	public SpriteGlitchSurfaceOvertipProperty DisplacementGlitchIntensity = new SpriteGlitchSurfaceOvertipProperty
	{
		PropertyName = "_DispIntensity"
	};

	public SpriteGlitchSurfaceOvertipProperty ColorGlitchProbability = new SpriteGlitchSurfaceOvertipProperty
	{
		PropertyName = "_ColorProbability"
	};

	public SpriteGlitchSurfaceOvertipProperty ColorGlitchIntensity = new SpriteGlitchSurfaceOvertipProperty
	{
		PropertyName = "_ColorIntensity"
	};

	private void TryCreateMaterials()
	{
		if (MaterialsPool == null)
		{
			MaterialsPool = new List<Material>();
			for (int i = 0; i < 25; i++)
			{
				Material material = UnityEngine.Object.Instantiate(m_MaterialSource);
				material.name = $"{material.name}_{i}";
				SetRandomFloat(GlitchIntervalTime, material, i);
				SetRandomFloat(DisplacementGlitchProbability, material, i);
				SetRandomFloat(DisplacementGlitchIntensity, material, i);
				SetRandomFloat(ColorGlitchProbability, material, i);
				SetRandomFloat(ColorGlitchIntensity, material, i);
				MaterialsPool.Add(material);
			}
		}
	}

	private void SetRandomFloat(SpriteGlitchSurfaceOvertipProperty property, Material newMat, int i)
	{
		newMat.SetFloat(property.PropertyName, property.MinValue + (property.MaxValue - property.MinValue) * (float)i / 25f);
	}

	public Material GetRandomMaterial()
	{
		TryCreateMaterials();
		return MaterialsPool[UnityEngine.Random.Range(0, 24)];
	}

	public Material GetRandomLowGlitchMaterial()
	{
		TryCreateMaterials();
		return MaterialsPool[UnityEngine.Random.Range(0, 10)];
	}

	public Material GetGlitchByIntensivity(float percent)
	{
		TryCreateMaterials();
		return MaterialsPool[Convert.ToInt16(Mathf.Round(percent / 100f * 24f))];
	}
}
