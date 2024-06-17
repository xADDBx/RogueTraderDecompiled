using System;
using Owlcat.Runtime.Visual.Overrides.HBAO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class HbaoPass : ScriptableRenderPass
{
	protected static class Pass
	{
		public const int AO_LowestQuality = 0;

		public const int AO_LowQuality = 1;

		public const int AO_MediumQuality = 2;

		public const int AO_HighQuality = 3;

		public const int AO_HighestQuality = 4;

		public const int AO_Deinterleaved_LowestQuality = 5;

		public const int AO_Deinterleaved_LowQuality = 6;

		public const int AO_Deinterleaved_MediumQuality = 7;

		public const int AO_Deinterleaved_HighQuality = 8;

		public const int AO_Deinterleaved_HighestQuality = 9;

		public const int Depth_Deinterleaving_2x2 = 10;

		public const int Depth_Deinterleaving_4x4 = 11;

		public const int Normals_Deinterleaving_2x2 = 12;

		public const int Normals_Deinterleaving_4x4 = 13;

		public const int Atlas = 14;

		public const int Reinterleaving_2x2 = 15;

		public const int Reinterleaving_4x4 = 16;

		public const int Blur_X_Narrow = 17;

		public const int Blur_X_Medium = 18;

		public const int Blur_X_Wide = 19;

		public const int Blur_X_ExtraWide = 20;

		public const int Blur_Y_Narrow = 21;

		public const int Blur_Y_Medium = 22;

		public const int Blur_Y_Wide = 23;

		public const int Blur_Y_ExtraWide = 24;

		public const int Composite = 25;

		public const int Composite_MultiBounce = 26;

		public const int Debug_AO_Only = 27;

		public const int Debug_AO_Only_MultiBounce = 28;

		public const int Debug_ColorBleeding_Only = 29;

		public const int Debug_Split_WithoutAO_WithAO = 30;

		public const int Debug_Split_WithoutAO_WithAO_MultiBounce = 31;

		public const int Debug_Split_WithAO_AOOnly = 32;

		public const int Debug_Split_WithAO_AOOnly_MultiBounce = 33;

		public const int Debug_Split_WithoutAO_AOOnly = 34;

		public const int Debug_Split_WithoutAO_AOOnly_MultiBounce = 35;

		public const int Combine_Deffered = 36;

		public const int Combine_Deffered_Multiplicative = 37;

		public const int Combine_Integrated = 38;

		public const int Combine_Integrated_MultiBounce = 39;

		public const int Combine_Integrated_Multiplicative = 40;

		public const int Combine_Integrated_Multiplicative_MultiBounce = 41;

		public const int Combine_ColorBleeding = 42;

		public const int Debug_Split_Additive = 43;

		public const int Debug_Split_Additive_MultiBounce = 44;

		public const int Debug_Split_Multiplicative = 45;

		public const int Debug_Split_Multiplicative_MultiBounce = 46;
	}

	protected class RenderTarget
	{
		public bool orthographic;

		public RenderingPath renderingPath;

		public bool hdr;

		public int width;

		public int height;

		public int fullWidth;

		public int fullHeight;

		public int layerWidth;

		public int layerHeight;

		public int downsamplingFactor;

		public int deinterleavingFactor;

		public int blurDownsamplingFactor;
	}

	protected static class ShaderProperties
	{
		public static int mainTex;

		public static int hbaoTex;

		public static int noiseTex;

		public static int rt0Tex;

		public static int rt3Tex;

		public static int depthTex;

		public static int normalsTex;

		public static int _CameraGBufferTexture2;

		public static int[] mrtDepthTex;

		public static int[] mrtNrmTex;

		public static int[] mrtHBAOTex;

		public static int[] deinterleavingOffset;

		public static int layerOffset;

		public static int jitter;

		public static int uvToView;

		public static int worldToCameraMatrix;

		public static int fullResTexelSize;

		public static int layerResTexelSize;

		public static int targetScale;

		public static int noiseTexSize;

		public static int radius;

		public static int maxRadiusPixels;

		public static int negInvRadius2;

		public static int angleBias;

		public static int aoMultiplier;

		public static int intensity;

		public static int intensityGlobal;

		public static int multiBounceInfluence;

		public static int offscreenSamplesContrib;

		public static int maxDistance;

		public static int distanceFalloff;

		public static int baseColor;

		public static int colorBleedSaturation;

		public static int albedoMultiplier;

		public static int colorBleedBrightnessMask;

		public static int colorBleedBrightnessMaskRange;

		public static int blurSharpness;

		static ShaderProperties()
		{
			mainTex = Shader.PropertyToID("_MainTex");
			hbaoTex = Shader.PropertyToID("_HBAOTex");
			noiseTex = Shader.PropertyToID("_NoiseTex");
			rt0Tex = Shader.PropertyToID("_rt0Tex");
			rt3Tex = Shader.PropertyToID("_rt3Tex");
			depthTex = Shader.PropertyToID("_DepthTex");
			normalsTex = Shader.PropertyToID("_NormalsTex");
			mrtDepthTex = new int[16];
			mrtNrmTex = new int[16];
			mrtHBAOTex = new int[16];
			for (int i = 0; i < 16; i++)
			{
				mrtDepthTex[i] = Shader.PropertyToID("_DepthLayerTex" + i);
				mrtNrmTex[i] = Shader.PropertyToID("_NormalLayerTex" + i);
				mrtHBAOTex[i] = Shader.PropertyToID("_HBAOLayerTex" + i);
			}
			deinterleavingOffset = new int[4]
			{
				Shader.PropertyToID("_Deinterleaving_Offset00"),
				Shader.PropertyToID("_Deinterleaving_Offset10"),
				Shader.PropertyToID("_Deinterleaving_Offset01"),
				Shader.PropertyToID("_Deinterleaving_Offset11")
			};
			layerOffset = Shader.PropertyToID("_LayerOffset");
			jitter = Shader.PropertyToID("_Jitter");
			uvToView = Shader.PropertyToID("_UVToView");
			worldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");
			fullResTexelSize = Shader.PropertyToID("_FullRes_TexelSize");
			layerResTexelSize = Shader.PropertyToID("_LayerRes_TexelSize");
			targetScale = Shader.PropertyToID("_TargetScale");
			noiseTexSize = Shader.PropertyToID("_NoiseTexSize");
			radius = Shader.PropertyToID("_Radius");
			maxRadiusPixels = Shader.PropertyToID("_MaxRadiusPixels");
			negInvRadius2 = Shader.PropertyToID("_NegInvRadius2");
			angleBias = Shader.PropertyToID("_AngleBias");
			aoMultiplier = Shader.PropertyToID("_AOmultiplier");
			intensity = Shader.PropertyToID("_Intensity");
			intensityGlobal = Shader.PropertyToID("_HbaoIntensity");
			multiBounceInfluence = Shader.PropertyToID("_MultiBounceInfluence");
			offscreenSamplesContrib = Shader.PropertyToID("_OffscreenSamplesContrib");
			maxDistance = Shader.PropertyToID("_MaxDistance");
			distanceFalloff = Shader.PropertyToID("_DistanceFalloff");
			baseColor = Shader.PropertyToID("_BaseColor");
			colorBleedSaturation = Shader.PropertyToID("_ColorBleedSaturation");
			albedoMultiplier = Shader.PropertyToID("_AlbedoMultiplier");
			colorBleedBrightnessMask = Shader.PropertyToID("_ColorBleedBrightnessMask");
			colorBleedBrightnessMaskRange = Shader.PropertyToID("_ColorBleedBrightnessMaskRange");
			blurSharpness = Shader.PropertyToID("_BlurSharpness");
			_CameraGBufferTexture2 = Shader.PropertyToID("_CameraGBufferTexture2");
		}
	}

	private static class MersenneTwister
	{
		public static float[] Numbers = new float[32]
		{
			0.463937f, 0.340042f, 0.223035f, 0.468465f, 0.322224f, 0.979269f, 0.031798f, 0.973392f, 0.778313f, 0.456168f,
			0.258593f, 0.330083f, 0.387332f, 0.380117f, 0.179842f, 0.910755f, 0.511623f, 0.092933f, 0.180794f, 0.620153f,
			0.101348f, 0.556342f, 0.642479f, 0.442008f, 0.215115f, 0.475218f, 0.157357f, 0.568868f, 0.501241f, 0.629229f,
			0.699218f, 0.707733f
		};
	}

	private const string kProfilerTag = "Draw HBAO";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw HBAO");

	private Material m_Material;

	private Hbao m_Settings;

	private GBuffer m_GBuffer;

	protected RenderTarget _renderTarget;

	private Texture2D noiseTex;

	private int[] _numSampleDirections = new int[5] { 3, 4, 6, 8, 8 };

	protected const int NUM_MRTS = 4;

	protected Vector4[] _jitter = new Vector4[16];

	private string[] _hbaoShaderKeywords = new string[4];

	private Quality _quality;

	private NoiseType _noiseType;

	private Mesh m_QuadMesh;

	public HbaoPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
		_renderTarget = new RenderTarget();
		m_QuadMesh = new Mesh();
		m_QuadMesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f)
		};
		m_QuadMesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f)
		};
		m_QuadMesh.triangles = new int[6] { 0, 1, 2, 1, 0, 3 };
	}

	internal void Setup(GBuffer gBuffer)
	{
		m_GBuffer = gBuffer;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Settings = stack.GetComponent<Hbao>();
		if (!m_Settings.IsActive())
		{
			return;
		}
		UpdateShaderProperties(ref renderingData);
		UpdateShaderKeywords();
		CommandBuffer cmd = CommandBufferPool.Get();
		using (new ProfilingScope(cmd, m_ProfilingSampler))
		{
			if (renderingData.CameraData.IsNeedDepthPyramid)
			{
				cmd.SetGlobalTexture(CommonTextureId.Unity_CameraDepthTexture, m_GBuffer.CameraDepthRt.Identifier());
			}
			switch (m_Settings.Deinterleaving.value)
			{
			case Deinterleaving.Disabled:
				PrepareCommandBufferHBAO(ref cmd);
				break;
			case Deinterleaving._2x:
				PrepareCommandBufferHBAODeinterleaved2x(ref cmd);
				break;
			case Deinterleaving._4x:
				PrepareCommandBufferHBAODeinterleaved4x(ref cmd);
				break;
			default:
				PrepareCommandBufferHBAO(ref cmd);
				break;
			}
			if (renderingData.CameraData.IsNeedDepthPyramid)
			{
				cmd.SetGlobalTexture(CommonTextureId.Unity_CameraDepthTexture, m_GBuffer.CameraDepthCopyRt.Identifier());
			}
		}
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	private void PrepareCommandBufferHBAODeinterleaved4x(ref CommandBuffer cmd)
	{
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.mainTex);
		RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(ShaderProperties.hbaoTex);
		RenderTargetIdentifier[] array = new RenderTargetIdentifier[16];
		RenderTargetIdentifier[] array2 = new RenderTargetIdentifier[16];
		RenderTargetIdentifier[] array3 = new RenderTargetIdentifier[16];
		cmd.SetGlobalTexture(ShaderProperties._CameraGBufferTexture2, m_GBuffer.CameraNormalsRt.Identifier());
		for (int i = 0; i < 16; i++)
		{
			cmd.GetTemporaryRT(ShaderProperties.mrtDepthTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.RFloat);
			cmd.GetTemporaryRT(ShaderProperties.mrtNrmTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
			cmd.GetTemporaryRT(ShaderProperties.mrtHBAOTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);
			array[i] = ShaderProperties.mrtDepthTex[i];
			array2[i] = ShaderProperties.mrtNrmTex[i];
			array3[i] = ShaderProperties.mrtHBAOTex[i];
		}
		for (int j = 0; j < 4; j++)
		{
			int num = (j & 1) << 1;
			int num2 = j >> 1 << 1;
			cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[0], new Vector2(num, num2));
			cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[1], new Vector2(num + 1, num2));
			cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[2], new Vector2(num, num2 + 1));
			cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[3], new Vector2(num + 1, num2 + 1));
			RenderTargetIdentifier[] array4 = new RenderTargetIdentifier[4]
			{
				array[j << 2],
				array[(j << 2) + 1],
				array[(j << 2) + 2],
				array[(j << 2) + 3]
			};
			RenderTargetIdentifier[] array5 = new RenderTargetIdentifier[4]
			{
				array2[j << 2],
				array2[(j << 2) + 1],
				array2[(j << 2) + 2],
				array2[(j << 2) + 3]
			};
			cmd.SetRenderTarget(array4, array4[0]);
			cmd.DrawMesh(m_QuadMesh, Matrix4x4.identity, m_Material, 0, 11);
			cmd.SetRenderTarget(array5, array5[0]);
			cmd.DrawMesh(m_QuadMesh, Matrix4x4.identity, m_Material, 0, 13);
		}
		for (int k = 0; k < 16; k++)
		{
			cmd.SetGlobalTexture(ShaderProperties.depthTex, array[k]);
			cmd.SetGlobalTexture(ShaderProperties.normalsTex, array2[k]);
			cmd.SetGlobalVector(ShaderProperties.jitter, _jitter[k]);
			cmd.SetRenderTarget(array3[k]);
			cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			cmd.Blit(BuiltinRenderTextureType.CameraTarget, array3[k], m_Material, GetAoDeinterleavedPass());
		}
		cmd.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		for (int l = 0; l < 16; l++)
		{
			cmd.SetGlobalVector(ShaderProperties.layerOffset, new Vector2(((l & 1) + ((l & 7) >> 2 << 1)) * _renderTarget.layerWidth, (((l & 3) >> 1) + (l >> 3 << 1)) * _renderTarget.layerHeight));
			cmd.Blit(array3[l], renderTargetIdentifier, m_Material, 14);
		}
		cmd.GetTemporaryRT(ShaderProperties.hbaoTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		cmd.Blit(renderTargetIdentifier, renderTargetIdentifier2, m_Material, 16);
		if (m_Settings.BlurAmount.value != 0)
		{
			cmd.ReleaseTemporaryRT(ShaderProperties.mainTex);
			cmd.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.blurDownsamplingFactor);
			cmd.Blit(renderTargetIdentifier2, renderTargetIdentifier, m_Material, GetBlurXPass());
			cmd.Blit(renderTargetIdentifier, renderTargetIdentifier2, m_Material, GetBlurYPass());
		}
		cmd.ReleaseTemporaryRT(ShaderProperties.mainTex);
		for (int m = 0; m < 16; m++)
		{
			cmd.ReleaseTemporaryRT(ShaderProperties.mrtHBAOTex[m]);
			cmd.ReleaseTemporaryRT(ShaderProperties.mrtNrmTex[m]);
			cmd.ReleaseTemporaryRT(ShaderProperties.mrtDepthTex[m]);
		}
		cmd.SetGlobalTexture(ShaderProperties.hbaoTex, renderTargetIdentifier2);
		cmd.SetGlobalFloat(ShaderProperties.intensityGlobal, m_Settings.Intensity.value);
		RenderHBAO(ref cmd);
	}

	private void PrepareCommandBufferHBAODeinterleaved2x(ref CommandBuffer cmd)
	{
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.mainTex);
		RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(ShaderProperties.hbaoTex);
		RenderTargetIdentifier[] array = new RenderTargetIdentifier[4]
		{
			ShaderProperties.mrtDepthTex[0],
			ShaderProperties.mrtDepthTex[1],
			ShaderProperties.mrtDepthTex[2],
			ShaderProperties.mrtDepthTex[3]
		};
		RenderTargetIdentifier[] array2 = new RenderTargetIdentifier[4]
		{
			ShaderProperties.mrtNrmTex[0],
			ShaderProperties.mrtNrmTex[1],
			ShaderProperties.mrtNrmTex[2],
			ShaderProperties.mrtNrmTex[3]
		};
		RenderTargetIdentifier[] array3 = new RenderTargetIdentifier[4]
		{
			ShaderProperties.mrtHBAOTex[0],
			ShaderProperties.mrtHBAOTex[1],
			ShaderProperties.mrtHBAOTex[2],
			ShaderProperties.mrtHBAOTex[3]
		};
		cmd.SetGlobalTexture(ShaderProperties._CameraGBufferTexture2, m_GBuffer.CameraNormalsRt.Identifier());
		for (int i = 0; i < 4; i++)
		{
			cmd.GetTemporaryRT(ShaderProperties.mrtDepthTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.RFloat);
			cmd.GetTemporaryRT(ShaderProperties.mrtNrmTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
			cmd.GetTemporaryRT(ShaderProperties.mrtHBAOTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);
		}
		cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[0], new Vector2(0f, 0f));
		cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[1], new Vector2(1f, 0f));
		cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[2], new Vector2(0f, 1f));
		cmd.SetGlobalVector(ShaderProperties.deinterleavingOffset[3], new Vector2(1f, 1f));
		cmd.SetRenderTarget(array, array[0]);
		cmd.DrawMesh(m_QuadMesh, Matrix4x4.identity, m_Material, 0, 10);
		cmd.SetRenderTarget(array2, array2[0]);
		cmd.DrawMesh(m_QuadMesh, Matrix4x4.identity, m_Material, 0, 12);
		for (int j = 0; j < 4; j++)
		{
			cmd.SetGlobalTexture(ShaderProperties.depthTex, array[j]);
			cmd.SetGlobalTexture(ShaderProperties.normalsTex, array2[j]);
			cmd.SetGlobalVector(ShaderProperties.jitter, _jitter[j]);
			cmd.SetRenderTarget(array3[j]);
			cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			cmd.Blit(BuiltinRenderTextureType.CameraTarget, array3[j], m_Material, GetAoDeinterleavedPass());
		}
		cmd.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		for (int k = 0; k < 4; k++)
		{
			cmd.SetGlobalVector(ShaderProperties.layerOffset, new Vector2((k & 1) * _renderTarget.layerWidth, (k >> 1) * _renderTarget.layerHeight));
			cmd.Blit(array3[k], renderTargetIdentifier, m_Material, 14);
		}
		cmd.GetTemporaryRT(ShaderProperties.hbaoTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		cmd.Blit(renderTargetIdentifier, renderTargetIdentifier2, m_Material, 15);
		if (m_Settings.BlurAmount.value != 0)
		{
			cmd.ReleaseTemporaryRT(ShaderProperties.mainTex);
			cmd.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.blurDownsamplingFactor);
			cmd.Blit(renderTargetIdentifier2, renderTargetIdentifier, m_Material, GetBlurXPass());
			cmd.Blit(renderTargetIdentifier, renderTargetIdentifier2, m_Material, GetBlurYPass());
		}
		cmd.ReleaseTemporaryRT(ShaderProperties.mainTex);
		for (int l = 0; l < 4; l++)
		{
			cmd.ReleaseTemporaryRT(ShaderProperties.mrtHBAOTex[l]);
			cmd.ReleaseTemporaryRT(ShaderProperties.mrtNrmTex[l]);
			cmd.ReleaseTemporaryRT(ShaderProperties.mrtDepthTex[l]);
		}
		cmd.SetGlobalTexture(ShaderProperties.hbaoTex, renderTargetIdentifier2);
		cmd.SetGlobalFloat(ShaderProperties.intensityGlobal, m_Settings.Intensity.value);
		RenderHBAO(ref cmd);
	}

	private void PrepareCommandBufferHBAO(ref CommandBuffer cmd)
	{
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.mainTex);
		RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(ShaderProperties.hbaoTex);
		cmd.GetTemporaryRT(ShaderProperties.hbaoTex, _renderTarget.fullWidth / _renderTarget.downsamplingFactor, _renderTarget.fullHeight / _renderTarget.downsamplingFactor, 0, FilterMode.Bilinear);
		cmd.SetRenderTarget(renderTargetIdentifier2);
		cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
		cmd.SetGlobalTexture(ShaderProperties._CameraGBufferTexture2, m_GBuffer.CameraNormalsRt.Identifier());
		cmd.Blit(BuiltinRenderTextureType.None, renderTargetIdentifier2, m_Material, GetAoPass());
		if (m_Settings.BlurAmount.value != 0)
		{
			cmd.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor, 0, FilterMode.Bilinear);
			cmd.Blit(renderTargetIdentifier2, renderTargetIdentifier, m_Material, GetBlurXPass());
			cmd.Blit(renderTargetIdentifier, renderTargetIdentifier2, m_Material, GetBlurYPass());
			cmd.ReleaseTemporaryRT(ShaderProperties.mainTex);
		}
		cmd.SetGlobalTexture(ShaderProperties.hbaoTex, renderTargetIdentifier2);
		cmd.SetGlobalFloat(ShaderProperties.intensityGlobal, m_Settings.Intensity.value);
		RenderHBAO(ref cmd);
	}

	private void RenderHBAO(ref CommandBuffer cmd)
	{
		if (_renderTarget.hdr)
		{
			if (m_Settings.UseMultiBounce.value)
			{
				RenderTargetIdentifier dest = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
				cmd.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
				cmd.Blit(m_GBuffer.CameraColorRt.Identifier(), dest);
			}
			cmd.Blit(BuiltinRenderTextureType.None, m_GBuffer.CameraColorRt.Identifier(), m_Material, m_Settings.UseMultiBounce.value ? 41 : 40);
			if (m_Settings.ColorBleedingEnabled.value)
			{
				cmd.Blit(BuiltinRenderTextureType.None, m_GBuffer.CameraColorRt.Identifier(), m_Material, 42);
			}
			if (m_Settings.UseMultiBounce.value)
			{
				cmd.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
			}
		}
		else
		{
			RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
			cmd.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);
			cmd.Blit(m_GBuffer.CameraColorRt.Identifier(), renderTargetIdentifier);
			cmd.Blit(renderTargetIdentifier, m_GBuffer.CameraColorRt.Identifier(), m_Material, m_Settings.UseMultiBounce.value ? 39 : 38);
			cmd.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
		}
	}

	protected void UpdateShaderProperties(ref RenderingData renderingData)
	{
		Camera camera = renderingData.CameraData.Camera;
		_renderTarget.orthographic = camera.orthographic;
		_renderTarget.renderingPath = camera.actualRenderingPath;
		_renderTarget.hdr = renderingData.CameraData.IsHdrEnabled;
		if (XRSettings.enabled)
		{
			_renderTarget.width = XRSettings.eyeTextureDesc.width;
			_renderTarget.height = XRSettings.eyeTextureDesc.height;
		}
		else
		{
			_renderTarget.width = camera.pixelWidth;
			_renderTarget.height = camera.pixelHeight;
		}
		_renderTarget.downsamplingFactor = ((m_Settings.Resolution.value == Owlcat.Runtime.Visual.Overrides.HBAO.Resolution.Full) ? 1 : ((m_Settings.Resolution.value == Owlcat.Runtime.Visual.Overrides.HBAO.Resolution.Half) ? 2 : 4));
		_renderTarget.deinterleavingFactor = GetDeinterleavingFactor();
		_renderTarget.blurDownsamplingFactor = ((!m_Settings.Downsample.value) ? 1 : 2);
		float num = Mathf.Tan(0.5f * camera.fieldOfView * (MathF.PI / 180f));
		float num2 = 1f / (1f / num * ((float)_renderTarget.height / (float)_renderTarget.width));
		float num3 = 1f / (1f / num);
		m_Material.SetVector(ShaderProperties.uvToView, new Vector4(2f * num2, -2f * num3, -1f * num2, 1f * num3));
		m_Material.SetMatrix(ShaderProperties.worldToCameraMatrix, camera.worldToCameraMatrix);
		if (m_Settings.Deinterleaving != Deinterleaving.Disabled)
		{
			_renderTarget.fullWidth = _renderTarget.width + ((_renderTarget.width % _renderTarget.deinterleavingFactor != 0) ? (_renderTarget.deinterleavingFactor - _renderTarget.width % _renderTarget.deinterleavingFactor) : 0);
			_renderTarget.fullHeight = _renderTarget.height + ((_renderTarget.height % _renderTarget.deinterleavingFactor != 0) ? (_renderTarget.deinterleavingFactor - _renderTarget.height % _renderTarget.deinterleavingFactor) : 0);
			_renderTarget.layerWidth = _renderTarget.fullWidth / _renderTarget.deinterleavingFactor;
			_renderTarget.layerHeight = _renderTarget.fullHeight / _renderTarget.deinterleavingFactor;
			m_Material.SetVector(ShaderProperties.fullResTexelSize, new Vector4(1f / (float)_renderTarget.fullWidth, 1f / (float)_renderTarget.fullHeight, _renderTarget.fullWidth, _renderTarget.fullHeight));
			m_Material.SetVector(ShaderProperties.layerResTexelSize, new Vector4(1f / (float)_renderTarget.layerWidth, 1f / (float)_renderTarget.layerHeight, _renderTarget.layerWidth, _renderTarget.layerHeight));
			m_Material.SetVector(ShaderProperties.targetScale, new Vector4((float)_renderTarget.fullWidth / (float)_renderTarget.width, (float)_renderTarget.fullHeight / (float)_renderTarget.height, 1f / ((float)_renderTarget.fullWidth / (float)_renderTarget.width), 1f / ((float)_renderTarget.fullHeight / (float)_renderTarget.height)));
		}
		else
		{
			_renderTarget.fullWidth = _renderTarget.width;
			_renderTarget.fullHeight = _renderTarget.height;
			m_Material.SetVector(ShaderProperties.targetScale, new Vector4(1f, 1f, 1f, 1f));
		}
		if (noiseTex == null || _quality != m_Settings.Quality.value || _noiseType != m_Settings.NoiseType.value)
		{
			if (noiseTex != null)
			{
				UnityEngine.Object.DestroyImmediate(noiseTex);
			}
			float num4 = ((m_Settings.NoiseType.value == NoiseType.Dither) ? 4 : 64);
			CreateRandomTexture((int)num4);
		}
		_quality = m_Settings.Quality.value;
		_noiseType = m_Settings.NoiseType.value;
		m_Material.SetTexture(ShaderProperties.noiseTex, noiseTex);
		m_Material.SetFloat(ShaderProperties.noiseTexSize, (_noiseType == NoiseType.Dither) ? 4 : 64);
		m_Material.SetFloat(ShaderProperties.radius, m_Settings.Radius.value * 0.5f * ((float)_renderTarget.height / (num * 2f)) / (float)_renderTarget.deinterleavingFactor);
		m_Material.SetFloat(ShaderProperties.maxRadiusPixels, m_Settings.MaxRadiusPixels.value / (float)_renderTarget.deinterleavingFactor);
		m_Material.SetFloat(ShaderProperties.negInvRadius2, -1f / (m_Settings.Radius.value * m_Settings.Radius.value));
		m_Material.SetFloat(ShaderProperties.angleBias, m_Settings.Bias.value);
		m_Material.SetFloat(ShaderProperties.aoMultiplier, 2f * (1f / (1f - m_Settings.Bias.value)));
		m_Material.SetFloat(ShaderProperties.intensity, m_Settings.Intensity.value);
		m_Material.SetFloat(ShaderProperties.multiBounceInfluence, m_Settings.MultiBounceInfluence.value);
		m_Material.SetFloat(ShaderProperties.maxDistance, m_Settings.MaxDistance.value);
		m_Material.SetFloat(ShaderProperties.distanceFalloff, m_Settings.DistanceFalloff.value);
		m_Material.SetColor(ShaderProperties.baseColor, m_Settings.BaseColor.value);
		m_Material.SetFloat(ShaderProperties.colorBleedSaturation, m_Settings.Saturation.value);
		m_Material.SetFloat(ShaderProperties.albedoMultiplier, m_Settings.AlbedoMultiplier.value);
		m_Material.SetFloat(ShaderProperties.colorBleedBrightnessMask, m_Settings.BrightnessMask.value);
		m_Material.SetVector(ShaderProperties.colorBleedBrightnessMaskRange, m_Settings.BrightnessMaskRange.value);
		m_Material.SetFloat(ShaderProperties.blurSharpness, m_Settings.Sharpness.value);
	}

	protected void UpdateShaderKeywords()
	{
		_hbaoShaderKeywords[0] = (m_Settings.ColorBleedingEnabled.value ? "COLOR_BLEEDING_ON" : "__");
		if (_renderTarget.orthographic)
		{
			_hbaoShaderKeywords[1] = "ORTHOGRAPHIC_PROJECTION_ON";
		}
		else
		{
			_hbaoShaderKeywords[1] = (IsDeferredShading() ? "DEFERRED_SHADING_ON" : "__");
		}
		_hbaoShaderKeywords[2] = "__";
		_hbaoShaderKeywords[3] = "__";
		CoreUtils.SetKeyword(m_Material, "COLOR_BLEEDING_ON", m_Settings.ColorBleedingEnabled.value);
		CoreUtils.SetKeyword(m_Material, "ORTHOGRAPHIC_PROJECTION_ON", _renderTarget.orthographic);
		CoreUtils.SetKeyword(m_Material, "DEFERRED_SHADING_ON", IsDeferredShading());
	}

	private bool IsDeferredShading()
	{
		return false;
	}

	private void CreateRandomTexture(int size)
	{
		noiseTex = new Texture2D(size, size, TextureFormat.RGB24, mipChain: false, linear: true);
		noiseTex.filterMode = FilterMode.Point;
		noiseTex.wrapMode = TextureWrapMode.Repeat;
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num2 = ((m_Settings.NoiseType == NoiseType.Dither) ? MersenneTwister.Numbers[num++] : UnityEngine.Random.Range(0f, 1f));
				float b = ((m_Settings.NoiseType == NoiseType.Dither) ? MersenneTwister.Numbers[num++] : UnityEngine.Random.Range(0f, 1f));
				float f = MathF.PI * 2f * num2 / (float)_numSampleDirections[GetAoPass()];
				Color color = new Color(Mathf.Cos(f), Mathf.Sin(f), b);
				noiseTex.SetPixel(i, j, color);
			}
		}
		noiseTex.Apply();
		int k = 0;
		int num3 = 0;
		for (; k < _jitter.Length; k++)
		{
			float num4 = MersenneTwister.Numbers[num3++];
			float z = MersenneTwister.Numbers[num3++];
			float f2 = MathF.PI * 2f * num4 / (float)_numSampleDirections[GetAoPass()];
			_jitter[k] = new Vector4(Mathf.Cos(f2), Mathf.Sin(f2), z, 0f);
		}
	}

	private int GetAoPass()
	{
		return m_Settings.Quality.value switch
		{
			Quality.Lowest => 0, 
			Quality.Low => 1, 
			Quality.Medium => 2, 
			Quality.High => 3, 
			Quality.Highest => 4, 
			_ => 2, 
		};
	}

	protected int GetBlurXPass()
	{
		return m_Settings.BlurAmount.value switch
		{
			Blur.Narrow => 17, 
			Blur.Medium => 18, 
			Blur.Wide => 19, 
			Blur.ExtraWide => 20, 
			_ => 18, 
		};
	}

	protected int GetBlurYPass()
	{
		return m_Settings.BlurAmount.value switch
		{
			Blur.Narrow => 21, 
			Blur.Medium => 22, 
			Blur.Wide => 23, 
			Blur.ExtraWide => 24, 
			_ => 22, 
		};
	}

	protected int GetDeinterleavingFactor()
	{
		return m_Settings.Deinterleaving.value switch
		{
			Deinterleaving._2x => 2, 
			Deinterleaving._4x => 4, 
			_ => 1, 
		};
	}

	protected int GetAoDeinterleavedPass()
	{
		return m_Settings.Quality.value switch
		{
			Quality.Lowest => 5, 
			Quality.Low => 6, 
			Quality.Medium => 7, 
			Quality.High => 8, 
			Quality.Highest => 9, 
			_ => 7, 
		};
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(ShaderProperties.hbaoTex);
	}
}
