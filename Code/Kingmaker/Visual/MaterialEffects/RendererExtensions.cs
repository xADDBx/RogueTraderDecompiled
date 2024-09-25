using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects;

public static class RendererExtensions
{
	private const int kMaxPreAllocatedMaterialArraySize = 32;

	private static readonly Dictionary<int, Material[]> s_DynamicallyAllocatedArrays;

	private static readonly Material[][] s_PreallocatedArrays;

	static RendererExtensions()
	{
		s_DynamicallyAllocatedArrays = new Dictionary<int, Material[]>(32);
		s_PreallocatedArrays = new Material[32][];
		for (int i = 0; i < s_PreallocatedArrays.Length; i++)
		{
			s_PreallocatedArrays[i] = new Material[i + 1];
		}
	}

	public static int GetSubMeshCount(this SkinnedMeshRenderer renderer)
	{
		Mesh sharedMesh = renderer.sharedMesh;
		if (!(sharedMesh != null))
		{
			return 0;
		}
		return sharedMesh.subMeshCount;
	}

	public static int GetSubMeshCount(this MeshRenderer renderer)
	{
		if (renderer.TryGetComponent<MeshFilter>(out var component))
		{
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh != null)
			{
				return sharedMesh.subMeshCount;
			}
		}
		return 0;
	}

	public static void SetSharedMaterialsExt(this Renderer renderer, List<Material> materialList)
	{
		if (materialList == null)
		{
			renderer.sharedMaterials = Array.Empty<Material>();
			return;
		}
		int count = materialList.Count;
		if (count == 0)
		{
			renderer.sharedMaterials = Array.Empty<Material>();
			return;
		}
		Material[] value;
		if (count <= 32)
		{
			value = s_PreallocatedArrays[count - 1];
		}
		else if (!s_DynamicallyAllocatedArrays.TryGetValue(count, out value))
		{
			value = new Material[count];
			s_DynamicallyAllocatedArrays.Add(count, value);
		}
		try
		{
			materialList.CopyTo(0, value, 0, count);
			renderer.sharedMaterials = value;
		}
		finally
		{
			Array.Fill(value, null);
		}
	}
}
