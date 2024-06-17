using System;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Fow Of War")]
public class FogOfWarFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/FogOfWar/Shaders/FogOfWar.shader", ReloadAttribute.Package.Root)]
		public Shader FogOfWarShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/FogOfWar/Shaders/ScreenSpaceFogOfWar.shader", ReloadAttribute.Package.Root)]
		public Shader ScreenSpaceFogOfWarShader;

		[SerializeField]
		[Reload("Shaders/Utils/MobileBlur.shader", ReloadAttribute.Package.Root)]
		public Shader BlurShader;

		[SerializeField]
		[Reload("Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
		public Shader BlitShader;
	}

	public ShaderResources Shaders;

	private Material m_FowMat;

	private Material m_ScreenSpaceMat;

	private Material m_BlurMat;

	private Material m_BlitMat;

	private Mesh m_QuadMesh;

	private FogOfWarShadowmapPass m_ShadowmapPass;

	private FogOfWarSetupPass m_SetupPass;

	private FogOfWarCleanupPass m_CleanupPass;

	private FogOfWarPostProcessPass m_PostProcessPass;

	private int m_LastFrameId;

	private Texture2D m_DefaultFogOfWarMask;

	[SerializeField]
	private FogOfWarSettings m_Settings;

	public FogOfWarSettings Settings => m_Settings;

	public Texture2D DefaultFogOfWarMask => m_DefaultFogOfWarMask;

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

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		CameraType cameraType = renderingData.CameraData.Camera.cameraType;
		if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
		{
			return;
		}
		FogOfWarArea active = FogOfWarArea.Active;
		FogOfWarSettings instance = FogOfWarSettings.Instance;
		if (active != null && active.isActiveAndEnabled && active.FogOfWarMapRT != null)
		{
			if (m_LastFrameId != renderingData.TimeData.FrameId)
			{
				m_LastFrameId = renderingData.TimeData.FrameId;
				m_ShadowmapPass.Init(active, this, instance);
				renderer.EnqueuePass(m_ShadowmapPass);
			}
			m_PostProcessPass.Init(active, this, instance);
			renderer.EnqueuePass(m_PostProcessPass);
		}
		m_SetupPass.Init(active, this, instance);
		renderer.EnqueuePass(m_SetupPass);
		renderer.EnqueuePass(m_CleanupPass);
	}

	public override void Create()
	{
		if (m_Settings != null)
		{
			m_Settings.InitSingleton(m_Settings);
		}
		m_FowMat = CoreUtils.CreateEngineMaterial(Shaders.FogOfWarShader);
		m_ScreenSpaceMat = CoreUtils.CreateEngineMaterial(Shaders.ScreenSpaceFogOfWarShader);
		m_BlurMat = CoreUtils.CreateEngineMaterial(Shaders.BlurShader);
		m_BlitMat = CoreUtils.CreateEngineMaterial(Shaders.BlitShader);
		m_ShadowmapPass = new FogOfWarShadowmapPass(RenderPassEvent.BeforeRendering, m_FowMat, m_BlurMat);
		m_SetupPass = new FogOfWarSetupPass(RenderPassEvent.BeforeRendering);
		m_CleanupPass = new FogOfWarCleanupPass(RenderPassEvent.AfterRendering);
		m_PostProcessPass = new FogOfWarPostProcessPass(RenderPassEvent.BeforeRenderingTransparents, m_ScreenSpaceMat);
		m_LastFrameId = -1;
		m_DefaultFogOfWarMask = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		m_DefaultFogOfWarMask.name = "DefaultFowMask";
		m_DefaultFogOfWarMask.SetPixel(0, 0, new Color(1f, 1f, 0f, 0f));
		m_DefaultFogOfWarMask.Apply();
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

	protected override void Dispose(bool disposing)
	{
		CoreUtils.Destroy(m_FowMat);
		CoreUtils.Destroy(m_ScreenSpaceMat);
		CoreUtils.Destroy(m_BlurMat);
		CoreUtils.Destroy(m_BlitMat);
		if (m_QuadMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(m_QuadMesh);
		}
		if (m_DefaultFogOfWarMask != null)
		{
			UnityEngine.Object.DestroyImmediate(m_DefaultFogOfWarMask);
			m_DefaultFogOfWarMask = null;
		}
	}
}
