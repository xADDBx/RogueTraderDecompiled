using System.Collections.Generic;
using Owlcat.Runtime.Visual.Lighting;
using UnityEngine;

namespace Owlcat.Runtime.Visual.IndirectRendering.Details;

public class DetailsProxyMesh : IIndirectMesh
{
	public Dictionary<Vector3Int, int> InstanceMap = new Dictionary<Vector3Int, int>();

	public List<IndirectInstanceData> GpuInstances;

	public List<DetailInstanceData> Instances = new List<DetailInstanceData>();

	public int Count;

	public bool IsDynamic => true;

	public bool Hidden { get; set; }

	public Mesh Mesh { get; set; }

	public Vector3 Position => default(Vector3);

	public List<Material> Materials { get; set; } = new List<Material>();


	public int MaxDynamicInstances { get; private set; }

	public Vector2 ScaleRange { get; set; }

	public Vector2 RotationRange { get; set; }

	public LightLayerEnum RenderingLayerMask { get; set; } = LightLayerEnum.LightLayerDefault;


	public DetailsProxyMesh(Mesh mesh, List<Material> materials, int maxDynamicInstances)
	{
		Mesh = mesh;
		Materials = materials;
		MaxDynamicInstances = maxDynamicInstances;
	}

	public void UpdateInstances()
	{
	}
}
