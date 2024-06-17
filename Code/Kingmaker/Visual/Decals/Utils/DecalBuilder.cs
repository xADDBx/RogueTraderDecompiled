using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.Decals.Utils;

public static class DecalBuilder
{
	private static readonly MeshBuilder s_Builder = new MeshBuilder();

	private static Material s_BakerMaterial;

	private static readonly List<Vector3> s_TempVerts = new List<Vector3>();

	private static readonly List<Vector3> s_TempNormals = new List<Vector3>();

	private static readonly List<Vector2> s_TempLmapUv = new List<Vector2>();

	public static GameObject[] BuildAndSetDirty(DecalGeometryHolder decalHolder, bool bakeTextureAndMaterial = false)
	{
		return Build(s_Builder, decalHolder, bakeTextureAndMaterial);
	}

	private static GameObject[] Build(MeshBuilder builder, DecalGeometryHolder decalHolder, bool bakeTextureAndMaterial = false)
	{
		MeshRenderer[] componentsInChildren = decalHolder.gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(meshRenderer.gameObject);
			}
			else
			{
				Object.DestroyImmediate(meshRenderer.gameObject);
			}
		}
		decalHolder.ClearLightmapIndices();
		List<MeshRenderer> affectedRenderers = DecalUtils.GetAffectedRenderers(decalHolder.Decal);
		foreach (MeshRenderer item in affectedRenderers)
		{
			GameObject gameObject = new GameObject("Decal_(" + item.name + ")");
			gameObject.transform.parent = decalHolder.transform;
			gameObject.transform.localPosition = default(Vector3);
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			MeshRenderer meshRenderer2 = gameObject.AddComponent<MeshRenderer>();
			meshRenderer2.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer2.lightProbeUsage = ((item.lightmapIndex == -1) ? LightProbeUsage.BlendProbes : LightProbeUsage.Off);
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			Mesh sharedMesh = new Mesh
			{
				name = "Decal_Mesh_(" + item.name + ")"
			};
			meshFilter.sharedMesh = sharedMesh;
			gameObject.AddComponent<DecalGeometryGuard>();
			if (item.isPartOfStaticBatch)
			{
				MeshCollider component = item.GetComponent<MeshCollider>();
				if ((bool)component)
				{
					Build(builder, decalHolder.Decal, item.gameObject, component.sharedMesh, item.lightmapIndex, item.lightmapScaleOffset);
				}
			}
			else
			{
				MeshFilter component2 = item.GetComponent<MeshFilter>();
				Build(builder, decalHolder.Decal, item.gameObject, component2.sharedMesh, item.lightmapIndex, item.lightmapScaleOffset);
			}
			builder.Push(decalHolder.Decal.PushDistance / decalHolder.transform.localScale.z);
			builder.ToMesh(meshFilter.sharedMesh, decalHolder.Decal.transform.localToWorldMatrix);
			meshRenderer2.sharedMaterial = decalHolder.Material;
			meshRenderer2.lightmapIndex = item.lightmapIndex;
			meshRenderer2.lightmapScaleOffset = item.lightmapScaleOffset;
			decalHolder.AddLightmapIndex(meshRenderer2, item);
		}
		return affectedRenderers.Select((MeshRenderer i) => i.gameObject).ToArray();
	}

	private static Matrix4x4 CalculateProjMatrix(Decal decal)
	{
		Vector3 vector = decal.transform.localScale * 0.5f;
		return GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(0f - vector.x, vector.x, 0f - vector.y, vector.y, 0.1f, vector.z * 2f), renderIntoTexture: true);
	}

	private static Matrix4x4 CalculateViewMatrix(Decal decal)
	{
		return MathHelper.WorldToCameraMatrix(Matrix4x4.TRS(decal.transform.position - decal.transform.forward * decal.transform.localScale.z * 0.5f, decal.transform.rotation, Vector3.one));
	}

	private static void Build(MeshBuilder builder, Decal decal, GameObject @object, Mesh mesh, int lightmapIndex, Vector4 lmapTransform)
	{
		Matrix4x4 localToWorldMatrix = @object.transform.localToWorldMatrix;
		Matrix4x4 matrix4x = decal.transform.worldToLocalMatrix * localToWorldMatrix;
		Vector3[] vertices = mesh.vertices;
		Vector2[] array = mesh.uv2;
		Vector3[] normals = mesh.normals;
		if (lightmapIndex > -1 && array.Length == 0)
		{
			array = mesh.uv;
		}
		int[] triangles = mesh.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int num = triangles[i];
			int num2 = triangles[i + 1];
			int num3 = triangles[i + 2];
			Vector3 item = matrix4x.MultiplyPoint(vertices[num]);
			Vector3 item2 = matrix4x.MultiplyPoint(vertices[num2]);
			Vector3 item3 = matrix4x.MultiplyPoint(vertices[num3]);
			Vector3 normalized = localToWorldMatrix.MultiplyVector(normals[num]).normalized;
			Vector3 normalized2 = localToWorldMatrix.MultiplyVector(normals[num2]).normalized;
			Vector3 normalized3 = localToWorldMatrix.MultiplyVector(normals[num3]).normalized;
			s_TempVerts.Clear();
			s_TempNormals.Clear();
			s_TempLmapUv.Clear();
			s_TempVerts.Add(item);
			s_TempVerts.Add(item2);
			s_TempVerts.Add(item3);
			s_TempNormals.Add(normalized);
			s_TempNormals.Add(normalized2);
			s_TempNormals.Add(normalized3);
			if (array.Length != 0)
			{
				s_TempLmapUv.Add(array[num]);
				s_TempLmapUv.Add(array[num2]);
				s_TempLmapUv.Add(array[num3]);
			}
			AddTriangle(builder, decal);
		}
	}

	private static Vector2 TranformLmap(Vector2 lmapUv, Vector4 lmapTransform)
	{
		Vector2 result = lmapUv;
		result.x *= lmapTransform.x;
		result.y *= lmapTransform.y;
		result.x += lmapTransform.z;
		result.y += lmapTransform.w;
		return result;
	}

	private static void AddTriangle(MeshBuilder builder, Decal decal)
	{
		Rect uvRect = new Rect(0f, 0f, 1f, 1f);
		Vector3 normalized = Vector3.Cross(s_TempVerts[1] - s_TempVerts[0], s_TempVerts[2] - s_TempVerts[0]).normalized;
		if (Vector3.Angle(Vector3.forward, -normalized) <= decal.MaxAngle)
		{
			PolygonClippingUtils.Clip(s_TempVerts, s_TempNormals, s_TempLmapUv, normalized);
			if (s_TempVerts.Count > 0)
			{
				builder.AddPolygon(s_TempVerts, s_TempLmapUv, s_TempNormals, normalized, uvRect);
			}
		}
	}

	private static Rect To01(Rect rect, Texture2D texture)
	{
		rect.x /= texture.width;
		rect.y /= texture.height;
		rect.width /= texture.width;
		rect.height /= texture.height;
		return rect;
	}
}
