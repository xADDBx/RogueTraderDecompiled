using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.Passes.Base;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh;

public abstract class ScriptableRenderer : IDisposable
{
	private List<ScriptableRenderPass> m_ActiveRenderPassQueue = new List<ScriptableRenderPass>(32);

	private List<RendererList> m_ActiveRendererLists = new List<RendererList>(32);

	private List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	private RenderGraphResources m_Resources;

	private BasePasses m_BasePasses;

	private static Dictionary<int, ProfilingSampler> s_HashSamplerCache = new Dictionary<int, ProfilingSampler>();

	private static readonly ProfilingSampler s_UnknownSampler = new ProfilingSampler("Unknown");

	internal static ScriptableRenderer Current = null;

	public List<ScriptableRenderPass> ActiveRenderPassQueue => m_ActiveRenderPassQueue;

	public List<ScriptableRendererFeature> RendererFeatures => m_RendererFeatures;

	public RenderGraphResources RenderGraphResources => m_Resources;

	public DebugHandler DebugHandler { get; }

	public bool IsVolumetricLightingEnabled
	{
		get
		{
			foreach (ScriptableRendererFeature rendererFeature in m_RendererFeatures)
			{
				if (rendererFeature is VolumetricLightingFeature)
				{
					return true;
				}
			}
			return false;
		}
	}

	public ScriptableRenderer(ScriptableRendererData data)
	{
		if (Debug.isDebugBuild)
		{
			DebugHandler = new DebugHandler(data, this);
		}
		foreach (ScriptableRendererFeature rendererFeature in data.RendererFeatures)
		{
			if (!(rendererFeature == null))
			{
				rendererFeature.Create();
				m_RendererFeatures.Add(rendererFeature);
			}
		}
		m_Resources = new RenderGraphResources();
		m_ActiveRenderPassQueue.Clear();
	}

	public static ProfilingSampler TryGetOrAddCameraSampler(Camera camera)
	{
		ProfilingSampler value = null;
		int hashCode = camera.GetHashCode();
		if (!s_HashSamplerCache.TryGetValue(hashCode, out value))
		{
			value = new ProfilingSampler(camera.name ?? "");
			s_HashSamplerCache.Add(hashCode, value);
		}
		return value;
	}

	public void Dispose()
	{
		for (int i = 0; i < m_RendererFeatures.Count; i++)
		{
			if (!(m_RendererFeatures[i] == null))
			{
				m_RendererFeatures[i].Dispose();
			}
		}
		m_Resources.Cleanup();
		if (Debug.isDebugBuild && DebugHandler != null)
		{
			DebugHandler.Dispose();
		}
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	public virtual void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
	}

	public void EnqueuePass(ScriptableRenderPass pass)
	{
		m_ActiveRenderPassQueue.Add(pass);
	}

	private void SortStable(List<ScriptableRenderPass> list)
	{
		for (int i = 1; i < list.Count; i++)
		{
			ScriptableRenderPass scriptableRenderPass = list[i];
			int num = i - 1;
			while (num >= 0 && scriptableRenderPass < list[num])
			{
				list[num + 1] = list[num];
				num--;
			}
			list[num + 1] = scriptableRenderPass;
		}
	}

	internal void SetupInternal(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		SetupBasePasses(ref renderingData);
		Setup(context, ref renderingData);
	}

	private void SetupBasePasses(ref RenderingData renderingData)
	{
		if (m_BasePasses == null)
		{
			m_BasePasses = new BasePasses();
		}
		m_BasePasses.Setup(this, ref renderingData);
	}

	private void InitRenderGraphResources(ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		ref ShadowData shadowData = ref renderingData.ShadowData;
		RenderGraph renderGraph = renderingData.RenderGraph;
		m_Resources.SetRenderGraph(renderGraph);
		m_Resources.ImportCameraData(ref cameraData);
		m_Resources.ImportShadowmap(ref shadowData);
		InitRenderGraphResources(ref renderingData, m_Resources);
	}

	protected abstract void InitRenderGraphResources(ref RenderingData renderingData, RenderGraphResources resources);

	protected abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);

	private void ConfigureRendererLists(ref ScriptableRenderContext context, ref RenderingData renderingData)
	{
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererConfigureRendererLists)))
		{
			foreach (ScriptableRenderPass item in m_ActiveRenderPassQueue)
			{
				item.ConfigureRendererLists(ref renderingData, m_Resources);
				m_ActiveRendererLists.AddRange(item.UsedRendererLists);
			}
		}
		context.PrepareRendererListsAsync(m_ActiveRendererLists);
	}

	public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		ref TimeData timeData = ref renderingData.TimeData;
		Camera camera = cameraData.Camera;
		RenderGraph renderGraph = renderingData.RenderGraph;
		using (new ProfilingScope(ProfilingSampler.Get(WaaaghProfileId.RendererSortRenderPasses)))
		{
			SortStable(m_ActiveRenderPassQueue);
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		RenderGraphParameters renderGraphParameters = default(RenderGraphParameters);
		renderGraphParameters.commandBuffer = commandBuffer;
		renderGraphParameters.currentFrameIndex = timeData.FrameId;
		renderGraphParameters.executionName = GetExecutionName(ref cameraData);
		renderGraphParameters.rendererListCulling = true;
		renderGraphParameters.scriptableRenderContext = context;
		RenderGraphParameters parameters = renderGraphParameters;
		renderGraph.BeginRecording(in parameters);
		renderGraph.BeginProfilingSampler(TryGetOrAddCameraSampler(camera), ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\ScriptableRenderer.cs", 264);
		InitRenderGraphResources(ref renderingData);
		ConfigureRendererLists(ref context, ref renderingData);
		foreach (ScriptableRenderPass item in m_ActiveRenderPassQueue)
		{
			if (!item.AreRendererListsEmpty(context))
			{
				item.Execute(ref renderingData);
			}
			item.ClearRendererLists();
		}
		m_ActiveRenderPassQueue.Clear();
		renderGraph.EndProfilingSampler(TryGetOrAddCameraSampler(camera), ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\ScriptableRenderer.cs", 281);
		renderGraph.EndRecordingAndExecute();
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
		m_Resources.Cleanup();
		m_ActiveRendererLists.Clear();
	}

	private string GetExecutionName(ref CameraData cameraData)
	{
		switch (cameraData.CameraType)
		{
		case CameraType.Game:
			if (cameraData.RenderType == CameraRenderType.Base)
			{
				return "GameBase";
			}
			return "GameOverlay";
		case CameraType.SceneView:
			return "SceneView";
		case CameraType.Preview:
			return "Preview";
		case CameraType.VR:
			return "VR";
		case CameraType.Reflection:
			return "Reflection";
		default:
			return "Unknown";
		}
	}
}
