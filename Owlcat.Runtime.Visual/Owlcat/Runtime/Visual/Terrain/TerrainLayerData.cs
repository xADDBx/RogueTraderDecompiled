using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Terrain;

[Serializable]
[GenerateHLSL(PackingRules.Exact, false, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.205-hotfix.1\\Runtime\\Terrain\\TerrainLayerData.cs")]
public struct TerrainLayerData
{
	public Vector4 masksScale;

	public Vector4 uvMatrix;

	public float uvScale;

	public int diffuseTexIndex;

	public int normalTexIndex;

	public int masksTexIndex;
}
