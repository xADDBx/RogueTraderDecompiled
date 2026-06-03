using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public static class AugmentationBodyPartReplacer
{
	public static string GetBonePrefixForArmSide(EquipmentEntity.AugmentArmSide side)
	{
		return side switch
		{
			EquipmentEntity.AugmentArmSide.Left => "L_", 
			EquipmentEntity.AugmentArmSide.Right => "R_", 
			_ => null, 
		};
	}

	public static bool RequiresVertexCutting(EquipmentEntity ee)
	{
		if (ee != null && ee.IsAugmentation)
		{
			return ee.AugmentationArmSide != EquipmentEntity.AugmentArmSide.None;
		}
		return false;
	}

	public static GameObject CutVerticesByBonePrefix(BodyPart bodyPart, EquipmentEntity equipmentEntity, string bonePrefix)
	{
		if (bodyPart.SkinnedRenderer == null)
		{
			PFLog.TechArt.Error($"[AugmentBodyPartReplacer] No SkinnedMeshRenderer on {bodyPart.Type}");
			return null;
		}
		Mesh mesh = Object.Instantiate(bodyPart.SkinnedRenderer.sharedMesh);
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		int[] triangles = mesh.triangles;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		BoneWeight[] boneWeights = mesh.boneWeights;
		Matrix4x4[] bindposes = mesh.bindposes;
		Transform[] bones = bodyPart.SkinnedRenderer.bones;
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null && bones[i].name.StartsWith(bonePrefix))
			{
				hashSet.Add(i);
			}
		}
		if (hashSet.Count == 0)
		{
			PFLog.TechArt.Warning("[AugmentBodyPartReplacer] No bones with prefix '" + bonePrefix + "' found");
			Object.Destroy(mesh);
			return null;
		}
		bool[] array = new bool[vertices.Length];
		int num = 0;
		for (int j = 0; j < vertices.Length; j++)
		{
			BoneWeight boneWeight = boneWeights[j];
			bool flag = (hashSet.Contains(boneWeight.boneIndex0) && boneWeight.weight0 > 0.01f) || (hashSet.Contains(boneWeight.boneIndex1) && boneWeight.weight1 > 0.01f) || (hashSet.Contains(boneWeight.boneIndex2) && boneWeight.weight2 > 0.01f) || (hashSet.Contains(boneWeight.boneIndex3) && boneWeight.weight3 > 0.01f);
			array[j] = !flag;
			if (!flag)
			{
				num++;
			}
		}
		int[] array2 = new int[vertices.Length];
		Vector3[] array3 = new Vector3[num];
		Vector2[] array4 = new Vector2[num];
		Vector3[] array5 = new Vector3[num];
		Vector4[] array6 = new Vector4[num];
		BoneWeight[] array7 = new BoneWeight[num];
		int num2 = 0;
		for (int k = 0; k < vertices.Length; k++)
		{
			if (array[k])
			{
				array2[k] = num2;
				array3[num2] = vertices[k];
				array4[num2] = uv[k];
				array5[num2] = normals[k];
				array6[num2] = tangents[k];
				array7[num2] = boneWeights[k];
				num2++;
			}
			else
			{
				array2[k] = -1;
			}
		}
		List<int> list = new List<int>();
		for (int l = 0; l < triangles.Length; l += 3)
		{
			int num3 = array2[triangles[l]];
			int num4 = array2[triangles[l + 1]];
			int num5 = array2[triangles[l + 2]];
			if (num3 != -1 && num4 != -1 && num5 != -1)
			{
				list.Add(num3);
				list.Add(num4);
				list.Add(num5);
			}
		}
		if (array3.Length == 0 || list.Count == 0)
		{
			PFLog.TechArt.Log($"[AugmentBodyPartReplacer] Empty mesh after cutting {bodyPart.Type} with prefix '{bonePrefix}' — nothing left on this side");
			Object.Destroy(mesh);
			return null;
		}
		mesh.Clear();
		mesh.vertices = array3;
		mesh.uv = array4;
		mesh.normals = array5;
		mesh.tangents = array6;
		mesh.boneWeights = array7;
		mesh.bindposes = bindposes;
		mesh.triangles = list.ToArray();
		mesh.RecalculateBounds();
		GameObject gameObject = Object.Instantiate(bodyPart.RendererPrefab);
		gameObject.name = $"Modified_{bodyPart.Type}_{bonePrefix}Prefab";
		SkinnedMeshRenderer componentInChildren = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		componentInChildren.sharedMesh = mesh;
		componentInChildren.rootBone = bodyPart.SkinnedRenderer.rootBone;
		componentInChildren.bones = bodyPart.SkinnedRenderer.bones;
		PFLog.TechArt.Log($"[AugmentBodyPartReplacer] Cut {vertices.Length - num} vertices from {bodyPart.Type} (prefix '{bonePrefix}')");
		return gameObject;
	}
}
