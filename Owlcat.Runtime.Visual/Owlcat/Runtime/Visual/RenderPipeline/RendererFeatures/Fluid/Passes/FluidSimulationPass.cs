using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid.Passes;

public class FluidSimulationPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fluid Simulation";

	private const float kUpdatePeriod = 0.033f;

	private const int kDrawIndexedBatchSize = 250;

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fluid Simulation");

	private FluidArea m_ActiveArea;

	private Material m_Material;

	private int m_PassAdvectVelocity;

	private int m_PassApplyAmbientWindForce;

	private int m_PassVelocityDivergence;

	private int m_PassSolvePressure;

	private int m_PassSubsctractPressureGradient;

	private int m_PassAmbientFogDye;

	private int m_PassAdvectColor;

	private int m_PassApplyWindInteractionParticles;

	private List<Matrix4x4[]> m_InstancedLocalToWorld;

	private List<Vector4[]> m_InstancedParameters;

	private MaterialPropertyBlock m_InstancedProperties;

	private float m_FluidGlobalTime;

	private float m_TimeAccumulator;

	private FluidFeature m_Feature;

	public override bool IsOncePerFrame => true;

	public FluidSimulationPass(RenderPassEvent evt, Material fluidMaterial)
	{
		base.RenderPassEvent = evt;
		m_Material = fluidMaterial;
		m_Material.enableInstancing = true;
		m_PassAdvectVelocity = m_Material.FindPass("Advect Velocity");
		m_PassApplyAmbientWindForce = m_Material.FindPass("Apply Ambient Wind Force");
		m_PassVelocityDivergence = m_Material.FindPass("Velocity Divergence");
		m_PassSolvePressure = m_Material.FindPass("Solve Pressure");
		m_PassSubsctractPressureGradient = m_Material.FindPass("Substract Pressure Gradient");
		m_PassAmbientFogDye = m_Material.FindPass("Ambient Fog Dye");
		m_PassAdvectColor = m_Material.FindPass("Advect Color");
		m_PassApplyWindInteractionParticles = m_Material.FindPass("Apply Wind Interaction Particles");
		m_InstancedLocalToWorld = new List<Matrix4x4[]>();
		m_InstancedParameters = new List<Vector4[]>();
		m_InstancedProperties = new MaterialPropertyBlock();
	}

	public void Setup(FluidFeature feature, FluidArea activeArea)
	{
		m_Feature = feature;
		m_ActiveArea = activeArea;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			if (m_ActiveArea != null)
			{
				if (m_Feature.DebugSettings.Enabled && m_Feature.DebugSettings.ForceTickEveryFrame)
				{
					m_TimeAccumulator = 0.033f;
				}
				else
				{
					m_TimeAccumulator += Time.deltaTime;
				}
				if (Time.timeScale > 0f && m_TimeAccumulator >= 0.033f)
				{
					while (m_TimeAccumulator >= 0.033f)
					{
						Tick(0.033f, commandBuffer);
						m_TimeAccumulator -= 0.033f;
					}
				}
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				commandBuffer.SetRenderTarget(m_ActiveArea.GpuBuffer.StencilBuffer);
				commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, default(Color));
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private void Tick(float dt, CommandBuffer cmd)
	{
		m_FluidGlobalTime += dt;
		Vector4 value = m_ActiveArea.CalculateMaskST();
		cmd.SetGlobalFloat(FluidConstantBuffer._FluidGlobalTime, m_FluidGlobalTime);
		cmd.SetGlobalFloat(FluidConstantBuffer._Dt, dt);
		FluidAreaGpuBuffer gpuBuffer = m_ActiveArea.GpuBuffer;
		cmd.SetGlobalTexture(FluidConstantBuffer._Velocity, gpuBuffer.VelocityBuffer.Rt0);
		cmd.SetRenderTarget(gpuBuffer.VelocityBuffer.Rt1, gpuBuffer.StencilBuffer);
		Vector2 vector = new Vector2(m_ActiveArea.Bounds.size.x, m_ActiveArea.Bounds.size.z);
		Vector2 vector2 = new Vector2(1f, 1f);
		if (m_ActiveArea.AmbientWindSettings.NoiseMap != null)
		{
			vector2.x = m_Feature.TextureDensity / (float)m_ActiveArea.AmbientWindSettings.NoiseMap.width;
			vector2.y = m_Feature.TextureDensity / (float)m_ActiveArea.AmbientWindSettings.NoiseMap.height;
		}
		Vector4 value2 = m_ActiveArea.AmbientWindSettings.NoiseMapTiling;
		value2.x *= vector.x * vector2.x;
		value2.y *= vector.y * vector2.y;
		cmd.SetGlobalTexture(FluidConstantBuffer._AmbientWindNoiseMap, m_ActiveArea.AmbientWindSettings.NoiseMap);
		cmd.SetGlobalVector(FluidConstantBuffer._AmbientWindNoiseMap_ST, value2);
		Vector4 value3 = m_ActiveArea.AmbientWindSettings.Direction;
		cmd.SetGlobalVector(FluidConstantBuffer._AmbientWindParams, value3);
		cmd.DrawProcedural(Matrix4x4.identity, m_Material, m_PassApplyAmbientWindForce, MeshTopology.Triangles, 3);
		gpuBuffer.VelocityBuffer.Swap();
		cmd.Blit(gpuBuffer.VelocityBuffer.Rt0, gpuBuffer.VelocityBuffer.Rt1);
		gpuBuffer.VelocityBuffer.Swap();
		DrawInstancedWindParticles(cmd, gpuBuffer);
		cmd.SetGlobalTexture(FluidConstantBuffer._Target, gpuBuffer.VelocityBuffer.Rt0);
		cmd.SetGlobalTexture(FluidConstantBuffer._Velocity, gpuBuffer.VelocityBuffer.Rt0);
		cmd.SetGlobalFloat(FluidConstantBuffer._Decay, m_Feature.Decay);
		cmd.SetRenderTarget(gpuBuffer.VelocityBuffer.Rt1, gpuBuffer.StencilBuffer);
		cmd.DrawProcedural(Matrix4x4.identity, m_Material, m_PassAdvectVelocity, MeshTopology.Triangles, 3);
		gpuBuffer.VelocityBuffer.Swap();
		cmd.SetGlobalTexture(FluidConstantBuffer._Velocity, gpuBuffer.VelocityBuffer.Rt0);
		cmd.SetRenderTarget(gpuBuffer.DivergenceBuffer.Rt0, gpuBuffer.StencilBuffer);
		cmd.DrawProcedural(Matrix4x4.identity, m_Material, m_PassVelocityDivergence, MeshTopology.Triangles, 3);
		cmd.SetGlobalTexture(FluidConstantBuffer._Divergence, gpuBuffer.DivergenceBuffer.Rt0);
		cmd.SetGlobalFloat(FluidConstantBuffer._Alpha, -(gpuBuffer.PressureBuffer.Rt0.width * gpuBuffer.PressureBuffer.Rt0.height));
		for (int i = 0; i < m_Feature.Iterations; i++)
		{
			cmd.SetGlobalTexture(FluidConstantBuffer._Pressure, gpuBuffer.PressureBuffer.Rt0);
			cmd.SetRenderTarget(gpuBuffer.PressureBuffer.Rt1, gpuBuffer.StencilBuffer);
			cmd.DrawProcedural(Matrix4x4.identity, m_Material, m_PassSolvePressure, MeshTopology.Triangles, 3);
			gpuBuffer.PressureBuffer.Swap();
		}
		cmd.SetGlobalTexture(FluidConstantBuffer._Pressure, gpuBuffer.PressureBuffer.Rt0);
		cmd.SetGlobalTexture(FluidConstantBuffer._Velocity, gpuBuffer.VelocityBuffer.Rt0);
		cmd.SetRenderTarget(gpuBuffer.VelocityBuffer.Rt1, gpuBuffer.StencilBuffer);
		cmd.DrawProcedural(Matrix4x4.identity, m_Material, m_PassSubsctractPressureGradient, MeshTopology.Triangles, 3);
		gpuBuffer.VelocityBuffer.Swap();
		if (m_ActiveArea.FluidFogSettings.Enabled)
		{
			Bounds bounds = m_ActiveArea.Bounds;
			FluidFogSettings fluidFogSettings = m_ActiveArea.FluidFogSettings;
			vector = new Vector2(bounds.size.x, bounds.size.z);
			float num = ((fluidFogSettings.FogTex0 == null) ? 1f : (m_Feature.TextureDensity / (float)fluidFogSettings.FogTex0.width));
			float num2 = ((fluidFogSettings.FogTex1 == null) ? 1f : (m_Feature.TextureDensity / (float)fluidFogSettings.FogTex1.width));
			cmd.SetGlobalTexture(FluidConstantBuffer._ColorBuffer, gpuBuffer.FluidFogColorBuffer.Rt0);
			cmd.SetGlobalVector(FluidConstantBuffer._ColorBuffer_ST, value);
			cmd.SetGlobalTexture(FluidConstantBuffer._Velocity, gpuBuffer.VelocityBuffer.Rt0);
			cmd.SetGlobalTexture(FluidConstantBuffer._FogTex0, fluidFogSettings.FogTex0);
			cmd.SetGlobalTexture(FluidConstantBuffer._FogTex1, fluidFogSettings.FogTex1);
			value2 = fluidFogSettings.FogTex0TilingScrollSpeed;
			value2.x *= vector.x * num;
			value2.y *= vector.y * num;
			cmd.SetGlobalVector(FluidConstantBuffer._FogTex0_ST, value2);
			value2 = fluidFogSettings.FogTex1TilingScrollSpeed;
			value2.x *= vector.x * num2;
			value2.y *= vector.y * num2;
			cmd.SetGlobalVector(FluidConstantBuffer._FogTex1_ST, value2);
			cmd.SetGlobalFloat(FluidConstantBuffer._FogIntensityScale, fluidFogSettings.FogUpdateFactor);
			cmd.SetRenderTarget(gpuBuffer.FluidFogColorBuffer.Rt1, gpuBuffer.StencilBuffer);
			cmd.DrawProcedural(Matrix4x4.identity, m_Material, m_PassAmbientFogDye, MeshTopology.Triangles, 3);
			gpuBuffer.FluidFogColorBuffer.Swap();
			cmd.SetGlobalTexture(FluidConstantBuffer._Velocity, gpuBuffer.VelocityBuffer.Rt0);
			cmd.SetGlobalFloat(FluidConstantBuffer._FadeColor, fluidFogSettings.FogFadeoutFactor);
			cmd.SetGlobalVector(FluidConstantBuffer._VelocityMask, new Vector4(1f, 1f, 0f, 0f));
			cmd.SetGlobalTexture(FluidConstantBuffer._Target, gpuBuffer.FluidFogColorBuffer.Rt0);
			cmd.SetRenderTarget(gpuBuffer.FluidFogColorBuffer.Rt1, gpuBuffer.StencilBuffer);
			cmd.DrawProcedural(Matrix4x4.identity, m_Material, m_PassAdvectColor, MeshTopology.Triangles, 3);
			gpuBuffer.FluidFogColorBuffer.Swap();
		}
		cmd.SetGlobalTexture(FluidConstantBuffer._WindVelocityRT, gpuBuffer.VelocityBuffer.Rt0);
		cmd.SetGlobalVector(FluidConstantBuffer._WindVelocityRT_ST, value);
		cmd.SetGlobalTexture(FluidConstantBuffer._FluidFogMask, gpuBuffer.FluidFogColorBuffer.Rt0);
		cmd.SetGlobalVector(FluidConstantBuffer._FluidFogMask_ST, value);
	}

	private void DrawInstancedWindParticles(CommandBuffer cmd, FluidAreaGpuBuffer gpuBuffer)
	{
		int count = FluidInteraction.All.Count;
		int num = 0;
		if (count <= 0)
		{
			return;
		}
		for (int num2 = m_InstancedLocalToWorld.Count * 250; num2 < count; num2 = m_InstancedLocalToWorld.Count * 250)
		{
			m_InstancedLocalToWorld.Add(new Matrix4x4[250]);
			m_InstancedParameters.Add(new Vector4[250]);
		}
		int num3 = 0;
		foreach (FluidInteraction item in FluidInteraction.All)
		{
			Vector2 movementForce = item.GetMovementForce();
			Quaternion q = Quaternion.FromToRotation(Vector3.forward, movementForce.To3D().normalized);
			m_InstancedLocalToWorld[num][num3] = Matrix4x4.TRS(item.transform.position, q, Vector3.one * item.Size);
			Vector4 vector = item.GetMovementForce();
			vector.z = item.LerpFromSideDirToRadialDir;
			vector.w = item.LerpToMovementDir;
			m_InstancedParameters[num][num3] = vector;
			num3++;
			if (num3 >= 250)
			{
				num3 = 0;
				num++;
			}
		}
		cmd.SetGlobalTexture(FluidConstantBuffer._Velocity, gpuBuffer.VelocityBuffer.Rt0);
		Matrix4x4 proj = m_ActiveArea.CalculateProjMatrix(convertToGpu: false);
		Matrix4x4 view = m_ActiveArea.CalculateViewMatrix();
		cmd.SetRenderTarget(gpuBuffer.VelocityBuffer.Rt1, gpuBuffer.StencilBuffer);
		cmd.SetViewProjectionMatrices(view, proj);
		num = 0;
		int num4 = count;
		for (; num < count / 250; num++)
		{
			m_InstancedProperties.SetVectorArray(FluidConstantBuffer._Parameters, m_InstancedParameters[num]);
			cmd.DrawMeshInstanced(RenderingUtils.InteractionQuad, 0, m_Material, m_PassApplyWindInteractionParticles, m_InstancedLocalToWorld[num], 250, m_InstancedProperties);
			num4 -= 250;
		}
		if (num4 > 0)
		{
			m_InstancedProperties.SetVectorArray(FluidConstantBuffer._Parameters, m_InstancedParameters[num]);
			cmd.DrawMeshInstanced(RenderingUtils.InteractionQuad, 0, m_Material, m_PassApplyWindInteractionParticles, m_InstancedLocalToWorld[num], num4, m_InstancedProperties);
		}
		gpuBuffer.VelocityBuffer.Swap();
	}
}
