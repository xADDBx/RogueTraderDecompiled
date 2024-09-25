using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.Decals;

[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class ScreenSpaceDecalRenderer : MonoBehaviour
{
	private enum IntersectionType
	{
		Disjoint,
		Intersects,
		Contains
	}

	private CommandBuffer m_CbGuiDraw;

	private CommandBuffer m_CbGuiResolve;

	private CommandBuffer m_CbDecalsPrepass;

	private Mesh m_Mesh;

	private Material m_UtilsMaterial;

	private CameraEvent m_CeGuiDraw = CameraEvent.AfterForwardOpaque;

	private CameraEvent m_CeGuiResolve = CameraEvent.AfterImageEffects;

	private CameraEvent m_CeDecalsPrepass = CameraEvent.AfterDepthTexture;

	private RenderTexture m_DecalsRenderTexture;

	private RenderTexture m_NormalsRenderTexture;

	private Camera m_Camera;

	private List<ScreenSpaceDecal> m_VisibleDecals = new List<ScreenSpaceDecal>();

	private Plane[] m_CameraPlanes = new Plane[6];

	[HideInInspector]
	public NormalsReconstructionMethod NormalsReconstructionMethod;

	public static bool IsGUIDecalsVisible { get; set; } = true;


	private void OnEnable()
	{
		EnsureData();
	}

	private void OnPreCull()
	{
		EnsureData();
		DoCullingAndSort();
	}

	private void OnDisable()
	{
		CleanUp();
	}

	private void OnDestroy()
	{
		CleanUp();
	}

	private void DoCullingAndSort()
	{
		m_VisibleDecals.Clear();
		GeometryUtility.CalculateFrustumPlanes(m_Camera, m_CameraPlanes);
		foreach (ScreenSpaceDecal item in ScreenSpaceDecal.All)
		{
			if (item.IsFullScreen)
			{
				m_VisibleDecals.Add(item);
			}
			else if (GetSphereIntersection(ref item.BoundingSphere.position, item.BoundingSphere.radius) != 0)
			{
				m_VisibleDecals.Add(item);
			}
		}
		m_VisibleDecals.Sort(SortDecals);
	}

	private IntersectionType GetSphereIntersection(ref Vector3 center, float radius, float frustumPadding = 0f)
	{
		bool flag = false;
		for (int i = 0; i < 6; i++)
		{
			Vector3 normal = m_CameraPlanes[i].normal;
			float distance = m_CameraPlanes[i].distance;
			float num = normal.x * center.x + normal.y * center.y + normal.z * center.z + distance;
			if (num < 0f - radius - frustumPadding)
			{
				return IntersectionType.Disjoint;
			}
			flag = flag || num <= radius;
		}
		if (!flag)
		{
			return IntersectionType.Contains;
		}
		return IntersectionType.Intersects;
	}

	private int SortDecals(ScreenSpaceDecal x, ScreenSpaceDecal y)
	{
		if (x.IsFullScreen)
		{
			return -1;
		}
		if (y.IsFullScreen)
		{
			return 1;
		}
		if (x.Layer == y.Layer)
		{
			return x.CreationTime.CompareTo(y.CreationTime);
		}
		return x.Layer.CompareTo(y.Layer);
	}

	private void EnsureData()
	{
		EnsureMesh();
		if (m_UtilsMaterial == null)
		{
			m_UtilsMaterial = new Material(Shader.Find("Hidden/ScreenSpaceDecalsUtils"));
		}
		m_Camera = GetComponent<Camera>();
		if (m_CbGuiDraw == null)
		{
			m_CbGuiDraw = new CommandBuffer();
			m_CbGuiDraw.name = "GUI Decals Draw";
			m_Camera.AddCommandBuffer(m_CeGuiDraw, m_CbGuiDraw);
		}
		if (m_CbGuiResolve == null)
		{
			m_CbGuiResolve = new CommandBuffer();
			m_CbGuiResolve.name = "GUI Decals Resolve";
			m_Camera.AddCommandBuffer(m_CeGuiResolve, m_CbGuiResolve);
		}
		if (m_CbDecalsPrepass == null)
		{
			m_CbDecalsPrepass = new CommandBuffer();
			m_CbDecalsPrepass.name = "Screen Space Decal Pre Pass";
			m_Camera.AddCommandBuffer(m_CeDecalsPrepass, m_CbDecalsPrepass);
		}
	}

	private void CleanUp()
	{
		if (m_CbGuiDraw != null)
		{
			m_Camera.RemoveCommandBuffer(m_CeGuiDraw, m_CbGuiDraw);
			m_CbGuiDraw.Dispose();
			m_CbGuiDraw = null;
		}
		if (m_CbGuiResolve != null)
		{
			m_Camera.RemoveCommandBuffer(m_CeGuiResolve, m_CbGuiResolve);
			m_CbGuiResolve.Dispose();
			m_CbGuiResolve = null;
		}
		if (m_CbDecalsPrepass != null)
		{
			m_Camera.RemoveCommandBuffer(m_CeDecalsPrepass, m_CbDecalsPrepass);
			m_CbDecalsPrepass.Dispose();
			m_CbDecalsPrepass = null;
		}
		if (m_DecalsRenderTexture != null)
		{
			m_DecalsRenderTexture.Release();
			UnityEngine.Object.DestroyImmediate(m_DecalsRenderTexture);
		}
		if (m_NormalsRenderTexture != null)
		{
			m_NormalsRenderTexture.Release();
			UnityEngine.Object.DestroyImmediate(m_NormalsRenderTexture);
		}
		if (m_Mesh != null)
		{
			UnityEngine.Object.DestroyImmediate(m_Mesh);
		}
		ResetParameters();
	}

	private static void ResetParameters()
	{
		Shader.SetGlobalFloat("_ScreenSpaceDecalsGlobalFlag", 0f);
	}

	private void OnPreRender()
	{
		Shader.SetGlobalFloat("_ScreenSpaceDecalsGlobalFlag", 1f);
		UpdateRT();
		if (m_CbGuiDraw != null)
		{
			m_CbGuiDraw.Clear();
			if (IsGUIDecalsVisible)
			{
				m_CbGuiDraw.SetRenderTarget(m_DecalsRenderTexture, BuiltinRenderTextureType.CameraTarget);
				m_CbGuiDraw.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
				foreach (ScreenSpaceDecal visibleDecal in m_VisibleDecals)
				{
					if (!(visibleDecal == null) && visibleDecal.isActiveAndEnabled && visibleDecal.Type == ScreenSpaceDecal.DecalType.GUI)
					{
						Material sharedMaterial = visibleDecal.SharedMaterial;
						if (sharedMaterial != null && m_Mesh != null)
						{
							m_CbGuiDraw.DrawMesh(m_Mesh, visibleDecal.transform.localToWorldMatrix, sharedMaterial, 0, 0, visibleDecal.MaterialProperties);
						}
					}
				}
			}
		}
		if (m_CbGuiResolve != null)
		{
			m_CbGuiResolve.Clear();
			m_CbGuiResolve.Blit(m_DecalsRenderTexture, BuiltinRenderTextureType.CurrentActive, m_UtilsMaterial, 0);
		}
		if (m_CbDecalsPrepass == null)
		{
			return;
		}
		m_CbDecalsPrepass.Clear();
		NormalsReconstructionMethod = NormalsReconstructionMethod.Fetch2x2;
		switch (NormalsReconstructionMethod)
		{
		case NormalsReconstructionMethod.Fetch2x2:
			m_UtilsMaterial.EnableKeyword("FETCH2x2");
			m_UtilsMaterial.DisableKeyword("FETCH3x3");
			break;
		case NormalsReconstructionMethod.Fetch3x3:
			m_UtilsMaterial.EnableKeyword("FETCH3x3");
			m_UtilsMaterial.DisableKeyword("FETCH2x2");
			break;
		case NormalsReconstructionMethod.Derivatives:
			m_UtilsMaterial.DisableKeyword("FETCH2x2");
			m_UtilsMaterial.DisableKeyword("FETCH3x3");
			break;
		}
		float num = Mathf.Tan(0.5f * m_Camera.fieldOfView * (MathF.PI / 180f));
		float num2 = 1f / (1f / num * ((float)m_NormalsRenderTexture.height / (float)m_NormalsRenderTexture.width));
		float num3 = 1f / (1f / num);
		m_CbDecalsPrepass.SetGlobalVector("_UVToView", new Vector4(2f * num2, -2f * num3, -1f * num2, 1f * num3));
		m_CbDecalsPrepass.Blit(null, m_NormalsRenderTexture, m_UtilsMaterial, 1);
		m_CbDecalsPrepass.SetGlobalTexture("_CameraNormalsTex", m_NormalsRenderTexture);
		m_CbDecalsPrepass.SetRenderTarget(m_DecalsRenderTexture);
		m_CbDecalsPrepass.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
		foreach (ScreenSpaceDecal visibleDecal2 in m_VisibleDecals)
		{
			if (visibleDecal2 == null || !visibleDecal2.isActiveAndEnabled || visibleDecal2.Type != 0 || !visibleDecal2.IsVisible)
			{
				continue;
			}
			Material sharedMaterial2 = visibleDecal2.SharedMaterial;
			if (sharedMaterial2 != null)
			{
				if (visibleDecal2.IsFullScreen)
				{
					m_CbDecalsPrepass.Blit(null, m_DecalsRenderTexture, sharedMaterial2);
				}
				else if (m_Mesh != null)
				{
					m_CbDecalsPrepass.DrawMesh(m_Mesh, visibleDecal2.transform.localToWorldMatrix, sharedMaterial2, 0, 0, visibleDecal2.MaterialProperties);
				}
			}
		}
		m_CbDecalsPrepass.SetGlobalTexture("_ScreenSpaceDecalsTex", m_DecalsRenderTexture);
	}

	private void UpdateRT()
	{
		if (m_DecalsRenderTexture == null)
		{
			CreateRT();
			return;
		}
		int num = m_Camera.pixelWidth;
		int num2 = m_Camera.pixelHeight;
		if (m_Camera.activeTexture != null)
		{
			num = m_Camera.activeTexture.width;
			num2 = m_Camera.activeTexture.height;
		}
		int num3 = Mathf.Max(1, QualitySettings.antiAliasing);
		if (num3 > 1 && num >= 3840)
		{
			num3 = 1;
		}
		if (m_DecalsRenderTexture.width != num || m_DecalsRenderTexture.height != num2 || m_DecalsRenderTexture.antiAliasing != num3)
		{
			m_DecalsRenderTexture.Release();
			m_NormalsRenderTexture.Release();
			UnityEngine.Object.DestroyImmediate(m_DecalsRenderTexture);
			UnityEngine.Object.DestroyImmediate(m_NormalsRenderTexture);
			CreateRT();
		}
	}

	private void CreateRT()
	{
		int num = m_Camera.pixelWidth;
		int height = m_Camera.pixelHeight;
		if (m_Camera.activeTexture != null)
		{
			num = m_Camera.activeTexture.width;
			height = m_Camera.activeTexture.height;
		}
		int num2 = Mathf.Max(1, QualitySettings.antiAliasing);
		if (num2 > 1 && num >= 3840)
		{
			num2 = 1;
		}
		m_DecalsRenderTexture = new RenderTexture(num, height, 0, RenderTextureFormat.ARGB32);
		m_DecalsRenderTexture.antiAliasing = num2;
		m_DecalsRenderTexture.name = "Screen Space Decals RT";
		m_DecalsRenderTexture.hideFlags = HideFlags.DontSave;
		m_DecalsRenderTexture.useMipMap = false;
		m_DecalsRenderTexture.Create();
		m_NormalsRenderTexture = new RenderTexture(num, height, 0, RenderTextureFormat.ARGB32);
		m_NormalsRenderTexture.name = "Screen Space Normals For Decals";
		m_NormalsRenderTexture.hideFlags = HideFlags.DontSave;
		m_NormalsRenderTexture.useMipMap = false;
		m_NormalsRenderTexture.Create();
	}

	private void EnsureMesh()
	{
		if (m_Mesh == null)
		{
			m_Mesh = new Mesh
			{
				name = "ScreenSpaceDecals"
			};
			float num = 1f;
			float num2 = 1f;
			float num3 = 1f;
			Vector3 vector = new Vector3((0f - num) * 0.5f, (0f - num2) * 0.5f, num3 * 0.5f);
			Vector3 vector2 = new Vector3(num * 0.5f, (0f - num2) * 0.5f, num3 * 0.5f);
			Vector3 vector3 = new Vector3(num * 0.5f, (0f - num2) * 0.5f, (0f - num3) * 0.5f);
			Vector3 vector4 = new Vector3((0f - num) * 0.5f, (0f - num2) * 0.5f, (0f - num3) * 0.5f);
			Vector3 vector5 = new Vector3((0f - num) * 0.5f, num2 * 0.5f, num3 * 0.5f);
			Vector3 vector6 = new Vector3(num * 0.5f, num2 * 0.5f, num3 * 0.5f);
			Vector3 vector7 = new Vector3(num * 0.5f, num2 * 0.5f, (0f - num3) * 0.5f);
			Vector3 vector8 = new Vector3((0f - num) * 0.5f, num2 * 0.5f, (0f - num3) * 0.5f);
			Vector3[] vertices = new Vector3[24]
			{
				vector, vector2, vector3, vector4, vector8, vector5, vector, vector4, vector5, vector6,
				vector2, vector, vector7, vector8, vector4, vector3, vector6, vector7, vector3, vector2,
				vector8, vector7, vector6, vector5
			};
			Vector3 up = Vector3.up;
			Vector3 down = Vector3.down;
			Vector3 forward = Vector3.forward;
			Vector3 back = Vector3.back;
			Vector3 left = Vector3.left;
			Vector3 right = Vector3.right;
			Vector3[] normals = new Vector3[24]
			{
				down, down, down, down, left, left, left, left, forward, forward,
				forward, forward, back, back, back, back, right, right, right, right,
				up, up, up, up
			};
			Vector2 vector9 = new Vector2(0f, 0f);
			Vector2 vector10 = new Vector2(1f, 0f);
			Vector2 vector11 = new Vector2(0f, 1f);
			Vector2 vector12 = new Vector2(1f, 1f);
			Vector2[] uv = new Vector2[24]
			{
				vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11,
				vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10,
				vector12, vector11, vector9, vector10
			};
			int[] triangles = new int[36]
			{
				3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
				6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
				12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
				23, 21, 20, 23, 22, 21
			};
			m_Mesh.vertices = vertices;
			m_Mesh.normals = normals;
			m_Mesh.uv = uv;
			m_Mesh.triangles = triangles;
			m_Mesh.RecalculateBounds();
		}
	}
}
