using System;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;

[CreateAssetMenu(menuName = "Renderer Features/Fow Of War")]
public class FogOfWarFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/FogOfWar/Shaders/FogOfWar.shader", ReloadAttribute.Package.Root)]
		public Shader FogOfWarShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/Shaders/Utils/MobileBlur.shader", ReloadAttribute.Package.Root)]
		public Shader BlurShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
		public Shader BlitShader;
	}

	private static FogOfWarFeature s_Instance;

	public ShaderResources Shaders;

	[SerializeField]
	private FogOfWarSettings m_Settings;

	[SerializeField]
	private bool m_ShowDebug;

	[SerializeField]
	[Range(128f, 4096f)]
	private int m_DebugSize = 256;

	private Mesh m_QuadMesh;

	private FogOfWarShadowmapPass m_ShadowmapPass;

	private FogOfWarSetupPass m_SetupPass;

	private FogOfWarPostProcessPass m_PostProcessPass;

	private FogOfWarDebugPass m_DebugPass;

	public FogOfWarSettings Settings => m_Settings;

	public bool ShowDebug
	{
		get
		{
			return m_ShowDebug;
		}
		set
		{
			m_ShowDebug = value;
		}
	}

	public int DebugSize
	{
		get
		{
			return m_DebugSize;
		}
		set
		{
			m_DebugSize = value;
		}
	}

	public Mesh QuadMesh
	{
		get
		{
			if (m_QuadMesh == null)
			{
				CreateQuadMesh();
			}
			return m_QuadMesh;
		}
	}

	public static FogOfWarFeature Instance => s_Instance;

	public static bool IsActive
	{
		get
		{
			if (Instance != null && FogOfWarArea.Active != null)
			{
				return FogOfWarArea.Active.isActiveAndEnabled;
			}
			return false;
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		CameraType cameraType = renderingData.CameraData.Camera.cameraType;
		if (cameraType != CameraType.Preview && cameraType != CameraType.Reflection)
		{
			FogOfWarArea active = FogOfWarArea.Active;
			FogOfWarSettings instance = FogOfWarSettings.Instance;
			m_ShadowmapPass.Setup(instance, this, active);
			renderer.EnqueuePass(m_ShadowmapPass);
			m_SetupPass.Setup(instance, active);
			renderer.EnqueuePass(m_SetupPass);
			if (renderer is ClusteredRenderer clusteredRenderer && active != null && !active.ApplyShaderManually)
			{
				m_PostProcessPass.Setup(clusteredRenderer.GetCurrentCameraColorTexture());
				renderer.EnqueuePass(m_PostProcessPass);
			}
			if (m_ShowDebug)
			{
				m_DebugPass.Setup(active);
				renderer.EnqueuePass(m_DebugPass);
			}
		}
	}

	public override void Create()
	{
		s_Instance = this;
		if (m_Settings != null)
		{
			m_Settings.InitSingleton(m_Settings);
		}
		Material material = CoreUtils.CreateEngineMaterial(Shaders.FogOfWarShader);
		Material blurMaterial = CoreUtils.CreateEngineMaterial(Shaders.BlurShader);
		Material blitMaterial = CoreUtils.CreateEngineMaterial(Shaders.BlitShader);
		m_ShadowmapPass = new FogOfWarShadowmapPass(RenderPassEvent.BeforeRendering, material, blurMaterial);
		m_SetupPass = new FogOfWarSetupPass(RenderPassEvent.BeforeRendering);
		m_PostProcessPass = new FogOfWarPostProcessPass(RenderPassEvent.BeforeRenderingTransparents, material);
		m_DebugPass = new FogOfWarDebugPass(RenderPassEvent.AfterRendering, blitMaterial);
	}

	protected override void Dispose(bool disposing)
	{
		if (m_QuadMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(m_QuadMesh);
		}
	}

	private void CreateQuadMesh()
	{
		m_QuadMesh = new Mesh
		{
			name = "FOW_Quad"
		};
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(1f, 0f, 1f),
			new Vector3(1f, 0f, -1f),
			new Vector3(-1f, 0f, 1f),
			new Vector3(-1f, 0f, -1f)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		m_QuadMesh.vertices = vertices;
		m_QuadMesh.uv = uv;
		m_QuadMesh.triangles = triangles;
	}

	public override string GetFeatureIdentifier()
	{
		return "FogOfWarFeature";
	}

	public override void DisableFeature()
	{
		Shader.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, 0f);
	}
}
