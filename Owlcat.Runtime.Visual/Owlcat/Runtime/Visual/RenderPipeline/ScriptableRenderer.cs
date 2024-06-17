using System;
using System.Collections.Generic;
using System.Diagnostics;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public abstract class ScriptableRenderer
{
	private static class RenderPassBlock
	{
		public static readonly int BeforeRendering = 0;

		public static readonly int MainRendering = 1;

		public static readonly int AfterRendering = 2;
	}

	private int m_LastFrameId;

	private List<ScriptableRenderPass> m_ActiveRenderPassQueue = new List<ScriptableRenderPass>(32);

	private List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	private HashSet<ScriptableRenderPass> m_OncePerFrameRenderPasses = new HashSet<ScriptableRenderPass>();

	private const int k_RenderPassBlockCount = 3;

	private const string k_ReleaseResourcesTag = "Release Resources";

	private RenderPassEvent[] m_BlockEventLimits = new RenderPassEvent[3];

	private int[] m_BlockRanges = new int[4];

	private static bool m_InsideStereoRenderBlock;

	protected List<ScriptableRendererFeature> RendererFeatures => m_RendererFeatures;

	protected List<ScriptableRenderPass> ActiveRenderPassQueue => m_ActiveRenderPassQueue;

	public ScriptableRenderer(ScriptableRendererData data)
	{
		foreach (ScriptableRendererFeature rendererFeature in data.rendererFeatures)
		{
			if (!(rendererFeature == null))
			{
				rendererFeature.Create();
				m_RendererFeatures.Add(rendererFeature);
			}
		}
		Clear();
	}

	public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);

	public virtual void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
	{
	}

	public virtual void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
	}

	public virtual void FinishRendering(CommandBuffer cmd)
	{
	}

	public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		try
		{
			if (m_LastFrameId != FrameId.FrameCount)
			{
				m_LastFrameId = FrameId.FrameCount;
				ResetOncePerFramePasses();
			}
			Camera camera = renderingData.CameraData.Camera;
			ClearRenderState(context);
			ScriptableRenderPass.SortStable(m_ActiveRenderPassQueue);
			float time = Time.time;
			float deltaTime = Time.deltaTime;
			float smoothDeltaTime = Time.smoothDeltaTime;
			float unscaledTime = Time.unscaledTime;
			SetShaderTimeValues(time, deltaTime, smoothDeltaTime, unscaledTime);
			m_BlockEventLimits[RenderPassBlock.BeforeRendering] = RenderPassEvent.BeforeRenderingPrepasses;
			m_BlockEventLimits[RenderPassBlock.MainRendering] = RenderPassEvent.AfterRenderingPostProcessing;
			m_BlockEventLimits[RenderPassBlock.AfterRendering] = (RenderPassEvent)2147483647;
			FillBlockRanges(m_BlockEventLimits, m_BlockRanges);
			ExecuteBlock(RenderPassBlock.BeforeRendering, m_BlockRanges, context, ref renderingData);
			bool isStereoEnabled = renderingData.CameraData.IsStereoEnabled;
			context.SetupCameraProperties(camera, isStereoEnabled);
			SetupLights(context, ref renderingData);
			SetShaderTimeValues(time, deltaTime, smoothDeltaTime, unscaledTime);
			if (isStereoEnabled)
			{
				BeginXRRendering(context, camera);
			}
			if (renderingData.CameraData.IsVfxEnabled)
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get(string.Empty);
				VFXManager.ProcessCameraCommand(camera, commandBuffer, default(VFXCameraXRSettings), renderingData.CullResults);
				context.ExecuteCommandBuffer(commandBuffer);
				CommandBufferPool.Release(commandBuffer);
			}
			ExecuteBlock(RenderPassBlock.MainRendering, m_BlockRanges, context, ref renderingData);
			ExecuteBlock(RenderPassBlock.AfterRendering, m_BlockRanges, context, ref renderingData);
			if (isStereoEnabled)
			{
				EndXRRendering(context, camera);
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		finally
		{
			try
			{
				InternalFinishRendering(context);
			}
			catch (Exception exception2)
			{
				UnityEngine.Debug.LogException(exception2);
			}
			finally
			{
			}
		}
	}

	private void ResetOncePerFramePasses()
	{
		m_OncePerFrameRenderPasses.Clear();
	}

	private void FillBlockRanges(RenderPassEvent[] blockEventLimits, int[] blockRanges)
	{
		int num = 0;
		int i = 0;
		blockRanges[num++] = 0;
		for (int j = 0; j < blockEventLimits.Length - 1; j++)
		{
			for (; i < m_ActiveRenderPassQueue.Count && m_ActiveRenderPassQueue[i].RenderPassEvent < blockEventLimits[j]; i++)
			{
			}
			blockRanges[num++] = i;
		}
		blockRanges[num] = m_ActiveRenderPassQueue.Count;
	}

	public void EnqueuePass(ScriptableRenderPass pass)
	{
		if (pass != null)
		{
			m_ActiveRenderPassQueue.Add(pass);
		}
	}

	private void ClearRenderState(ScriptableRenderContext context)
	{
	}

	internal void Clear()
	{
		m_InsideStereoRenderBlock = false;
		m_ActiveRenderPassQueue.Clear();
	}

	private void ExecuteBlock(int blockIndex, int[] blockRanges, ScriptableRenderContext context, ref RenderingData renderingData, bool submit = false)
	{
		int num = blockRanges[blockIndex + 1];
		for (int i = blockRanges[blockIndex]; i < num; i++)
		{
			ScriptableRenderPass scriptableRenderPass = m_ActiveRenderPassQueue[i];
			if (scriptableRenderPass.IsOncePerFrame)
			{
				if (!m_OncePerFrameRenderPasses.Contains(scriptableRenderPass))
				{
					scriptableRenderPass.Execute(context, ref renderingData);
					m_OncePerFrameRenderPasses.Add(scriptableRenderPass);
				}
			}
			else
			{
				scriptableRenderPass.Execute(context, ref renderingData);
			}
		}
		if (submit)
		{
			context.Submit();
		}
	}

	private void BeginXRRendering(ScriptableRenderContext context, Camera camera)
	{
		context.StartMultiEye(camera);
		m_InsideStereoRenderBlock = true;
	}

	private void EndXRRendering(ScriptableRenderContext context, Camera camera)
	{
		context.StopMultiEye(camera);
		context.StereoEndRender(camera);
		m_InsideStereoRenderBlock = false;
	}

	internal static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier colorAttachment, RenderTargetIdentifier depthAttachment, ClearFlag clearFlag, Color clearColor)
	{
		RenderBufferLoadAction colorLoadAction = ((clearFlag != 0) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		RenderBufferLoadAction depthLoadAction = (((clearFlag & ClearFlag.Depth) != 0) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		TextureDimension dimension = (m_InsideStereoRenderBlock ? XRGraphics.eyeTextureDesc.dimension : TextureDimension.Tex2D);
		SetRenderTarget(cmd, colorAttachment, colorLoadAction, RenderBufferStoreAction.Store, depthAttachment, depthLoadAction, RenderBufferStoreAction.Store, clearFlag, clearColor, dimension);
	}

	private static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier colorAttachment, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, ClearFlag clearFlags, Color clearColor, TextureDimension dimension)
	{
		if (dimension == TextureDimension.Tex2DArray)
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, clearFlags, clearColor);
		}
		else
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, clearFlags, clearColor);
		}
	}

	private static void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier colorAttachment, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderTargetIdentifier depthAttachment, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction, ClearFlag clearFlags, Color clearColor, TextureDimension dimension)
	{
		if (depthAttachment == BuiltinRenderTextureType.CameraTarget)
		{
			SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, clearFlags, clearColor, dimension);
		}
		else if (dimension == TextureDimension.Tex2DArray)
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, depthAttachment, clearFlags, clearColor);
		}
		else
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, depthAttachment, depthLoadAction, depthStoreAction, clearFlags, clearColor);
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void DrawGizmos(ScriptableRenderContext context, Camera camera, GizmoSubset gizmoSubset)
	{
	}

	private void InternalFinishRendering(ScriptableRenderContext context)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get("Release Resources");
		for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
		{
			m_ActiveRenderPassQueue[i].FrameCleanup(commandBuffer);
		}
		FinishRendering(commandBuffer);
		Clear();
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	internal void Dispose()
	{
		foreach (ScriptableRendererFeature rendererFeature in m_RendererFeatures)
		{
			if (!(rendererFeature == null))
			{
				rendererFeature.Dispose();
			}
		}
		DisposeInternal();
	}

	protected internal virtual void DisposeInternal()
	{
	}

	private void SetShaderTimeValues(float time, float deltaTime, float smoothDeltaTime, float unscaledTime, CommandBuffer cmd = null)
	{
		float f = time / 8f;
		float f2 = time / 4f;
		float f3 = time / 2f;
		Vector4 value = time * new Vector4(0.05f, 1f, 2f, 3f);
		Vector4 value2 = new Vector4(Mathf.Sin(f), Mathf.Sin(f2), Mathf.Sin(f3), Mathf.Sin(time));
		Vector4 value3 = new Vector4(Mathf.Cos(f), Mathf.Cos(f2), Mathf.Cos(f3), Mathf.Cos(time));
		Vector4 value4 = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
		Vector4 value5 = unscaledTime * new Vector4(0.05f, 1f, 2f, 3f);
		if (cmd == null)
		{
			Shader.SetGlobalVector(PerFrameBuffer._Time, value);
			Shader.SetGlobalVector(PerFrameBuffer._SinTime, value2);
			Shader.SetGlobalVector(PerFrameBuffer._CosTime, value3);
			Shader.SetGlobalVector(PerFrameBuffer.unity_DeltaTime, value4);
			Shader.SetGlobalVector(PerFrameBuffer._UnscaledTime, value5);
		}
		else
		{
			cmd.SetGlobalVector(PerFrameBuffer._Time, value);
			cmd.SetGlobalVector(PerFrameBuffer._SinTime, value2);
			cmd.SetGlobalVector(PerFrameBuffer._CosTime, value3);
			cmd.SetGlobalVector(PerFrameBuffer.unity_DeltaTime, value4);
			cmd.SetGlobalVector(PerFrameBuffer._UnscaledTime, value5);
		}
	}
}
