using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Owlcat.Runtime.Visual;

public static class RenderingUtils
{
	private static int s_PostProcessingTemporaryTargetId = Shader.PropertyToID("_TemporaryColorTexture");

	private static List<ShaderTagId> s_LegacyShaderPassNames = new List<ShaderTagId>
	{
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

	private static Mesh s_LineMesh = null;

	private static Mesh s_FullscreenMesh = null;

	private static Mesh s_QuadMesh = null;

	private static Mesh s_QuadFlippedMesh = null;

	private static Mesh s_InteractionQuad;

	private static Mesh s_CubeMesh = null;

	private static Mesh s_CubeMeshWithUvAndNormals = null;

	private static Mesh s_SphereMesh = null;

	private static Material s_ErrorMaterial;

	private static Dictionary<RenderTextureFormat, bool> m_RenderTextureFormatSupport = new Dictionary<RenderTextureFormat, bool>();

	private static Dictionary<GraphicsFormat, Dictionary<FormatUsage, bool>> m_GraphicsFormatSupport = new Dictionary<GraphicsFormat, Dictionary<FormatUsage, bool>>();

	private const int kDrawIndexedBatchSize = 250;

	private static Matrix4x4[] s_InstancedLocalToWorld = new Matrix4x4[250];

	private static Vector4[] s_InstancedParameters = new Vector4[250];

	private static MaterialPropertyBlock s_InstancedProperties = new MaterialPropertyBlock();

	private static int _InstancedParameters0 = Shader.PropertyToID("_InstancedParameters0");

	private static int[] _idSHA = new int[3]
	{
		Shader.PropertyToID("unity_SHAr"),
		Shader.PropertyToID("unity_SHAg"),
		Shader.PropertyToID("unity_SHAb")
	};

	private static int[] _idSHB = new int[3]
	{
		Shader.PropertyToID("unity_SHBr"),
		Shader.PropertyToID("unity_SHBg"),
		Shader.PropertyToID("unity_SHBb")
	};

	private static int _idSHC = Shader.PropertyToID("unity_SHC");

	private static Dictionary<GraphicsFormat, int> graphicsFormatSizeCache = new Dictionary<GraphicsFormat, int>
	{
		{
			GraphicsFormat.R8G8B8A8_UNorm,
			4
		},
		{
			GraphicsFormat.R16G16B16A16_SFloat,
			8
		},
		{
			GraphicsFormat.RGB_BC6H_SFloat,
			1
		}
	};

	public static Mesh LineMesh
	{
		get
		{
			if (s_LineMesh != null)
			{
				return s_LineMesh;
			}
			s_LineMesh = new Mesh();
			s_LineMesh.vertices = new Vector3[2]
			{
				Vector3.zero,
				Vector3.up
			};
			s_LineMesh.uv = new Vector2[2]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f)
			};
			s_LineMesh.SetIndices(new int[2] { 0, 1 }, MeshTopology.Lines, 0);
			return s_LineMesh;
		}
	}

	public static Mesh FullscreenMesh
	{
		get
		{
			if (s_FullscreenMesh != null)
			{
				return s_FullscreenMesh;
			}
			float y = 1f;
			float y2 = 0f;
			s_FullscreenMesh = new Mesh
			{
				name = "Owlcat Default Fullscreen Quad"
			};
			s_FullscreenMesh.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 1f, 0f),
				new Vector3(1f, -1f, 0f),
				new Vector3(1f, 1f, 0f)
			});
			s_FullscreenMesh.SetUVs(0, new List<Vector2>
			{
				new Vector2(0f, y2),
				new Vector2(0f, y),
				new Vector2(1f, y2),
				new Vector2(1f, y)
			});
			s_FullscreenMesh.SetIndices(new int[6] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, calculateBounds: false);
			s_FullscreenMesh.UploadMeshData(markNoLongerReadable: true);
			return s_FullscreenMesh;
		}
	}

	public static Mesh QuadMesh
	{
		get
		{
			if (s_QuadMesh != null)
			{
				return s_QuadMesh;
			}
			Vector3[] array = new Vector3[4];
			float num = 1f * 0.5f;
			float num2 = 1f * 0.5f;
			array[0] = new Vector3(0f - num2, 0f - num, 0f);
			array[1] = new Vector3(0f - num2, num, 0f);
			array[2] = new Vector3(num2, 0f - num, 0f);
			array[3] = new Vector3(num2, num, 0f);
			Vector2[] array2 = new Vector2[array.Length];
			array2[0] = new Vector2(0f, 0f);
			array2[1] = new Vector2(0f, 1f);
			array2[2] = new Vector2(1f, 0f);
			array2[3] = new Vector2(1f, 1f);
			int[] triangles = new int[6] { 0, 1, 2, 3, 2, 1 };
			Vector3[] array3 = new Vector3[array.Length];
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i] = Vector3.back;
			}
			s_QuadMesh = new Mesh();
			s_QuadMesh.name = "Owlcat Default Quad Mesh";
			s_QuadMesh.vertices = array;
			s_QuadMesh.uv = array2;
			s_QuadMesh.triangles = triangles;
			s_QuadMesh.normals = array3;
			return s_QuadMesh;
		}
	}

	public static Mesh QuadFlippedMesh
	{
		get
		{
			if (s_QuadFlippedMesh != null)
			{
				return s_QuadFlippedMesh;
			}
			Vector3[] array = new Vector3[4];
			float num = 1f * 0.5f;
			float num2 = 1f * 0.5f;
			array[0] = new Vector3(0f - num2, 0f - num, 0f);
			array[1] = new Vector3(0f - num2, num, 0f);
			array[2] = new Vector3(num2, 0f - num, 0f);
			array[3] = new Vector3(num2, num, 0f);
			Vector2[] array2 = new Vector2[array.Length];
			array2[0] = new Vector2(1f, 0f);
			array2[1] = new Vector2(1f, 1f);
			array2[2] = new Vector2(0f, 0f);
			array2[3] = new Vector2(0f, 1f);
			int[] triangles = new int[6] { 0, 2, 1, 3, 1, 2 };
			Vector3[] array3 = new Vector3[array.Length];
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i] = Vector3.forward;
			}
			s_QuadFlippedMesh = new Mesh();
			s_QuadFlippedMesh.name = "Owlcat Default Quad Flipped Mesh";
			s_QuadFlippedMesh.vertices = array;
			s_QuadFlippedMesh.uv = array2;
			s_QuadFlippedMesh.triangles = triangles;
			s_QuadFlippedMesh.normals = array3;
			return s_QuadFlippedMesh;
		}
	}

	public static Mesh InteractionQuad
	{
		get
		{
			if (s_InteractionQuad != null)
			{
				return s_InteractionQuad;
			}
			Vector3[] array = new Vector3[4];
			float num = 1f * 0.5f;
			float num2 = 1f * 0.5f;
			array[0] = new Vector3(0f - num2, 0f, 0f - num);
			array[1] = new Vector3(0f - num2, 0f, num);
			array[2] = new Vector3(num2, 0f, 0f - num);
			array[3] = new Vector3(num2, 0f, num);
			Vector2[] array2 = new Vector2[array.Length];
			array2[0] = new Vector2(0f, 0f);
			array2[1] = new Vector2(0f, 1f);
			array2[2] = new Vector2(1f, 0f);
			array2[3] = new Vector2(1f, 1f);
			int[] triangles = new int[6] { 0, 1, 2, 3, 2, 1 };
			s_InteractionQuad = new Mesh();
			s_InteractionQuad.name = "Owlcat Interaction Quad Mesh";
			s_InteractionQuad.vertices = array;
			s_InteractionQuad.uv = array2;
			s_InteractionQuad.triangles = triangles;
			return s_InteractionQuad;
		}
	}

	public static Mesh CubeMesh
	{
		get
		{
			if (s_CubeMesh != null)
			{
				return s_CubeMesh;
			}
			s_CubeMesh = CoreUtils.CreateCubeMesh(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
			s_CubeMesh.name = "Owlcat Defatult Cube Mesh";
			return s_CubeMesh;
		}
	}

	public static Mesh CubeMeshWithUvAndNormals
	{
		get
		{
			if (s_CubeMeshWithUvAndNormals != null)
			{
				return s_CubeMeshWithUvAndNormals;
			}
			float num = 1f;
			float num2 = 1f;
			float num3 = 1f;
			Vector3[] array = new Vector3[8]
			{
				new Vector3((0f - num) * 0.5f, (0f - num2) * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, (0f - num2) * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, (0f - num2) * 0.5f, (0f - num3) * 0.5f),
				new Vector3((0f - num) * 0.5f, (0f - num2) * 0.5f, (0f - num3) * 0.5f),
				new Vector3((0f - num) * 0.5f, num2 * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, num2 * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, num2 * 0.5f, (0f - num3) * 0.5f),
				new Vector3((0f - num) * 0.5f, num2 * 0.5f, (0f - num3) * 0.5f)
			};
			Vector3[] vertices = new Vector3[24]
			{
				array[0],
				array[1],
				array[2],
				array[3],
				array[7],
				array[4],
				array[0],
				array[3],
				array[4],
				array[5],
				array[1],
				array[0],
				array[6],
				array[7],
				array[3],
				array[2],
				array[5],
				array[6],
				array[2],
				array[1],
				array[7],
				array[6],
				array[5],
				array[4]
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
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(1f, 0f);
			Vector2 vector3 = new Vector2(0f, 1f);
			Vector2 vector4 = new Vector2(1f, 1f);
			Vector2[] uv = new Vector2[24]
			{
				vector4, vector3, vector, vector2, vector4, vector3, vector, vector2, vector4, vector3,
				vector, vector2, vector4, vector3, vector, vector2, vector4, vector3, vector, vector2,
				vector4, vector3, vector, vector2
			};
			int[] triangles = new int[36]
			{
				3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
				6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
				12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
				23, 21, 20, 23, 22, 21
			};
			s_CubeMeshWithUvAndNormals = new Mesh();
			s_CubeMeshWithUvAndNormals.name = "Owlcat Cube Mesh With Uv And Normals";
			s_CubeMeshWithUvAndNormals.vertices = vertices;
			s_CubeMeshWithUvAndNormals.uv = uv;
			s_CubeMeshWithUvAndNormals.normals = normals;
			s_CubeMeshWithUvAndNormals.triangles = triangles;
			return s_CubeMeshWithUvAndNormals;
		}
	}

	public static Mesh SphereMesh
	{
		get
		{
			if (s_SphereMesh != null)
			{
				return s_SphereMesh;
			}
			s_SphereMesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
			return s_SphereMesh;
		}
	}

	private static Material ErrorMaterial
	{
		get
		{
			if (s_ErrorMaterial == null)
			{
				s_ErrorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
			}
			return s_ErrorMaterial;
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal static void RenderObjectsWithError(ScriptableRenderContext context, ref CullingResults cullResults, Camera camera, FilteringSettings filterSettings, SortingCriteria sortFlags)
	{
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = sortFlags;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(s_LegacyShaderPassNames[0], sortingSettings2);
		drawingSettings.perObjectData = PerObjectData.None;
		drawingSettings.overrideMaterial = ErrorMaterial;
		drawingSettings.overrideMaterialPassIndex = 0;
		DrawingSettings drawingSettings2 = drawingSettings;
		for (int i = 1; i < s_LegacyShaderPassNames.Count; i++)
		{
			drawingSettings2.SetShaderPassName(i, s_LegacyShaderPassNames[i]);
		}
		context.DrawRenderers(cullResults, ref drawingSettings2, ref filterSettings);
	}

	public static Matrix4x4 WorldToCamera(Camera camera)
	{
		return Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * camera.worldToCameraMatrix;
	}

	public static int DivRoundUp(int x, int y)
	{
		return (x + y - 1) / y;
	}

	public static Vector2 CalculateFrustumSizeOnGivenDistance(Camera camera, float distance)
	{
		float num = ((!camera.orthographic) ? (2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * (MathF.PI / 180f))) : (camera.orthographicSize * 2f));
		return new Vector2(num * camera.aspect, num);
	}

	public static bool ConeInsidePlane(Vector3 lightPos, float lightRange, Vector3 lightSpotDir, float lightSpotRange, Plane plane)
	{
		Vector3 vector = Vector3.Cross(Vector3.Cross(plane.normal, lightSpotDir), lightSpotDir);
		Vector3 rhs = lightPos + lightSpotDir * lightRange - vector * lightSpotRange;
		if (Vector3.Dot(plane.normal, lightPos) - plane.distance < 0f)
		{
			return Vector3.Dot(plane.normal, rhs) - plane.distance < 0f;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBakedShadowMaskLight(in LightBakingOutput bakingOutput)
	{
		if (bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && bakingOutput.mixedLightingMode == MixedLightingMode.Shadowmask)
		{
			return bakingOutput.occlusionMaskChannel != -1;
		}
		return false;
	}

	internal static void ClearSystemInfoCache()
	{
		m_RenderTextureFormatSupport.Clear();
		m_GraphicsFormatSupport.Clear();
	}

	internal static bool SupportsRenderTextureFormat(RenderTextureFormat format)
	{
		if (!m_RenderTextureFormatSupport.ContainsKey(format))
		{
			m_RenderTextureFormatSupport.Add(format, SystemInfo.SupportsRenderTextureFormat(format));
		}
		return m_RenderTextureFormatSupport[format];
	}

	public static void DrawInstanced(CommandBuffer cmd, Mesh mesh, Material material, int pass, List<Matrix4x4> matrices, List<Vector4> parameters)
	{
		int num = matrices.Count;
		while (num > 0)
		{
			int num2 = ((num > 250) ? 250 : num);
			if (num2 > 0)
			{
				num -= num2;
				matrices.CopyTo(num, s_InstancedLocalToWorld, 0, num2);
				parameters.CopyTo(num, s_InstancedParameters, 0, num2);
				s_InstancedProperties.SetVectorArray(_InstancedParameters0, s_InstancedParameters);
				cmd.DrawMeshInstanced(mesh, 0, material, pass, s_InstancedLocalToWorld, num2, s_InstancedProperties);
				continue;
			}
			break;
		}
	}

	internal static void SetLightProbe(CommandBuffer cmd, SphericalHarmonicsL2 sh)
	{
		for (int i = 0; i < 3; i++)
		{
			cmd.SetGlobalVector(_idSHA[i], new Vector4(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6]));
		}
		for (int j = 0; j < 3; j++)
		{
			cmd.SetGlobalVector(_idSHB[j], new Vector4(sh[j, 4], sh[j, 6], sh[j, 5] * 3f, sh[j, 7]));
		}
		cmd.SetGlobalVector(_idSHC, new Vector4(sh[0, 8], sh[2, 8], sh[1, 8], 1f));
	}

	internal static void SetLightProbe(CommandBuffer cmd, Vector3 pos)
	{
		LightProbes.GetInterpolatedProbe(pos, null, out var probe);
		SetLightProbe(cmd, probe);
	}

	internal static void SetDefaultReflectionProbe(MaterialPropertyBlock mpb)
	{
		mpb.SetTexture("unity_SpecCube0", ReflectionProbe.defaultTexture);
		mpb.SetVector("unity_SpecCube0_HDR", ReflectionProbe.defaultTextureHDRDecodeValues);
	}

	public static TextureDesc CreateTextureDesc(string name, RenderTextureDescriptor input)
	{
		TextureDesc result = new TextureDesc(input.width, input.height);
		result.colorFormat = input.graphicsFormat;
		result.depthBufferBits = (DepthBits)input.depthBufferBits;
		result.dimension = input.dimension;
		result.slices = input.volumeDepth;
		result.name = name;
		return result;
	}

	public static bool SupportsGraphicsFormat(GraphicsFormat format, FormatUsage usage)
	{
		bool value = false;
		if (!m_GraphicsFormatSupport.TryGetValue(format, out var value2))
		{
			value2 = new Dictionary<FormatUsage, bool>();
			value = SystemInfo.IsFormatSupported(format, usage);
			value2.Add(usage, value);
			m_GraphicsFormatSupport.Add(format, value2);
		}
		else if (!value2.TryGetValue(usage, out value))
		{
			value = SystemInfo.IsFormatSupported(format, usage);
			value2.Add(usage, value);
		}
		return value;
	}

	public static Matrix4x4 GetJitteredPerspectiveProjectionMatrix(Camera camera, Vector2Int viewportSize, Vector2 offset)
	{
		float nearClipPlane = camera.nearClipPlane;
		_ = camera.farClipPlane;
		float num = Mathf.Tan(MathF.PI / 360f * camera.fieldOfView) * nearClipPlane;
		float num2 = num * camera.aspect;
		offset.x *= num2 / (0.5f * (float)viewportSize.x);
		offset.y *= num / (0.5f * (float)viewportSize.y);
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		projectionMatrix[0, 2] += offset.x / num2;
		projectionMatrix[1, 2] += offset.y / num;
		return projectionMatrix;
	}

	public static Matrix4x4 GetJitteredOrthographicProjectionMatrix(Camera camera, Vector2Int viewportSize, Vector2 offset)
	{
		float orthographicSize = camera.orthographicSize;
		float num = orthographicSize * camera.aspect;
		offset.x *= num / (0.5f * (float)viewportSize.x);
		offset.y *= orthographicSize / (0.5f * (float)viewportSize.y);
		float left = offset.x - num;
		float right = offset.x + num;
		float top = offset.y + orthographicSize;
		float bottom = offset.y - orthographicSize;
		return Matrix4x4.Ortho(left, right, bottom, top, camera.nearClipPlane, camera.farClipPlane);
	}

	internal static RendererListDesc CreateRendererListDesc(CullingResults cull, Camera camera, ShaderTagId passName, PerObjectData rendererConfiguration = PerObjectData.None, RenderQueueRange? renderQueueRange = null, SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque, RenderStateBlock? stateBlock = null, Material overrideMaterial = null, bool excludeObjectMotionVectors = false)
	{
		RendererListDesc result = new RendererListDesc(passName, cull, camera);
		result.rendererConfiguration = rendererConfiguration;
		result.renderQueueRange = (renderQueueRange.HasValue ? renderQueueRange.Value : RenderQueueRange.opaque);
		result.sortingCriteria = sortingCriteria;
		result.stateBlock = stateBlock;
		result.overrideMaterial = overrideMaterial;
		result.excludeObjectMotionVectors = excludeObjectMotionVectors;
		return result;
	}

	internal static RendererListDesc CreateRendererListDesc(CullingResults cull, Camera camera, ShaderTagId[] passNames, PerObjectData rendererConfiguration = PerObjectData.None, RenderQueueRange? renderQueueRange = null, SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque, RenderStateBlock? stateBlock = null, Material overrideMaterial = null, bool excludeObjectMotionVectors = false)
	{
		RendererListDesc result = new RendererListDesc(passNames, cull, camera);
		result.rendererConfiguration = rendererConfiguration;
		result.renderQueueRange = (renderQueueRange.HasValue ? renderQueueRange.Value : RenderQueueRange.opaque);
		result.sortingCriteria = sortingCriteria;
		result.stateBlock = stateBlock;
		result.overrideMaterial = overrideMaterial;
		result.excludeObjectMotionVectors = excludeObjectMotionVectors;
		return result;
	}

	internal static RendererListDesc CreateRendererListDesc(CullingResults cull, Camera camera, ShaderTagId passName, LayerMask layerMask, PerObjectData rendererConfiguration = PerObjectData.None, RenderQueueRange? renderQueueRange = null, SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque, RenderStateBlock? stateBlock = null, Material overrideMaterial = null, bool excludeObjectMotionVectors = false)
	{
		RendererListDesc result = new RendererListDesc(passName, cull, camera);
		result.rendererConfiguration = rendererConfiguration;
		result.renderQueueRange = (renderQueueRange.HasValue ? renderQueueRange.Value : RenderQueueRange.opaque);
		result.sortingCriteria = sortingCriteria;
		result.stateBlock = stateBlock;
		result.overrideMaterial = overrideMaterial;
		result.excludeObjectMotionVectors = excludeObjectMotionVectors;
		result.layerMask = layerMask;
		return result;
	}

	internal static RendererListDesc CreateRendererListDesc(CullingResults cull, Camera camera, ShaderTagId[] passNames, LayerMask layerMask, PerObjectData rendererConfiguration = PerObjectData.None, RenderQueueRange? renderQueueRange = null, SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque, RenderStateBlock? stateBlock = null, Material overrideMaterial = null, bool excludeObjectMotionVectors = false)
	{
		RendererListDesc result = new RendererListDesc(passNames, cull, camera);
		result.rendererConfiguration = rendererConfiguration;
		result.renderQueueRange = (renderQueueRange.HasValue ? renderQueueRange.Value : RenderQueueRange.opaque);
		result.sortingCriteria = sortingCriteria;
		result.stateBlock = stateBlock;
		result.overrideMaterial = overrideMaterial;
		result.excludeObjectMotionVectors = excludeObjectMotionVectors;
		result.layerMask = layerMask;
		return result;
	}

	internal static int GetFormatSizeInBytes(GraphicsFormat format)
	{
		if (graphicsFormatSizeCache.TryGetValue(format, out var value))
		{
			return value;
		}
		string text = format.ToString();
		int num = text.IndexOf('_');
		text = text.Substring(0, (num == -1) ? text.Length : num);
		int num2 = 0;
		foreach (Match item in Regex.Matches(text, "\\d+"))
		{
			num2 += int.Parse(item.Value);
		}
		value = num2 / 8;
		graphicsFormatSizeCache[format] = value;
		return value;
	}

	public static int FirstBitLow(int v)
	{
		if (v == 0)
		{
			return -1;
		}
		int num = 0;
		while ((v & 1) == 0)
		{
			v >>= 1;
			num++;
		}
		return num;
	}

	public static int FirstBitLow(uint v)
	{
		if (v == 0)
		{
			return -1;
		}
		int num = 0;
		while ((v & 1) == 0)
		{
			v >>= 1;
			num++;
		}
		return num;
	}
}
