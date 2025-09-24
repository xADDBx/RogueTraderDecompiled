using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Terrain;

[Serializable]
[GenerateHLSL(PackingRules.Exact, false, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Terrain\\TerrainLayerData.cs")]
public struct TerrainLayerData
{
	public Vector4 masksScale;

	public Vector4 uvMatrix;

	public float uvScale;

	public int diffuseTexIndex;

	public int normalTexIndex;

	public int masksTexIndex;
}
