using System;
using Owlcat.Runtime.Visual.Overrides.HBAO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class HbaoPass : ScriptableRenderPass<HbaoPassData>
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

		public static int _CameraDepthTexture;

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
			_CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
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

	private Material m_Material;

	protected RenderTarget _renderTarget;

	private Texture2D noiseTex;

	private int[] _numSampleDirections = new int[5] { 3, 4, 6, 8, 8 };

	protected const int NUM_MRTS = 4;

	protected Vector4[] _jitter = new Vector4[16];

	private string[] _hbaoShaderKeywords = new string[4];

	private Quality _quality;

	private NoiseType _noiseType;

	private Mesh m_QuadMesh;

	public override string Name => "HbaoPass";

	public HbaoPass(RenderPassEvent evt, Material material)
		: base(evt)
	{
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

	protected override void Setup(RenderGraphBuilder builder, HbaoPassData data, ref RenderingData renderingData)
	{
		data.Material = m_Material;
		VolumeStack stack = VolumeManager.instance.stack;
		data.Settings = stack.GetComponent<Hbao>();
		data.RendererList = builder.DependsOn(in data.Resources.RendererLists.OpaqueGBuffer.List);
		builder.AllowRendererListCulling(!renderingData.IrsHasOpaques);
		builder.AllowPassCulling(value: true);
		if (data.Settings.IsActive())
		{
			UpdateShaderProperties(data, ref renderingData);
			UpdateShaderKeywords(data);
			data.CameraDepthRT = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
			data.CameraNormalsRT = builder.ReadTexture(in data.Resources.CameraNormalsRT);
			data.CameraColorRT = builder.ReadWriteTexture(in data.Resources.CameraColorBuffer);
			TextureDesc desc = RenderingUtils.CreateTextureDesc("HbaoRT", renderingData.CameraData.CameraTargetDescriptor);
			desc.width = _renderTarget.fullWidth / _renderTarget.downsamplingFactor;
			desc.height = _renderTarget.fullHeight / _renderTarget.downsamplingFactor;
			desc.filterMode = FilterMode.Bilinear;
			desc.wrapMode = TextureWrapMode.Clamp;
			desc.depthBufferBits = DepthBits.None;
			desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
			data.HbaoRT = builder.CreateTransientTexture(in desc);
			if (data.Settings.BlurAmount.value != 0)
			{
				TextureDesc desc2 = desc;
				desc2.name = "HbarBlurRT";
				desc2.width = _renderTarget.fullWidth / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor;
				desc2.height = _renderTarget.fullHeight / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor;
				data.HbaoBlurRT = builder.CreateTransientTexture(in desc2);
			}
			if (data.Settings.UseMultiBounce.value || !_renderTarget.hdr)
			{
				TextureDesc desc3 = desc;
				desc3.name = "HbaoRT3";
				desc3.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
				desc3.width = _renderTarget.fullWidth;
				desc3.height = _renderTarget.height;
				desc3.filterMode = (_renderTarget.hdr ? FilterMode.Bilinear : FilterMode.Point);
				data.HbaoRT3 = builder.CreateTransientTexture(in desc3);
			}
		}
	}

	protected override void Render(HbaoPassData data, RenderGraphContext context)
	{
		if (data.Settings.IsActive())
		{
			context.cmd.SetGlobalTexture(ShaderProperties._CameraDepthTexture, data.CameraDepthRT);
			if (data.Settings.Deinterleaving.value == Deinterleaving.Disabled)
			{
				PrepareCommandBufferHBAO(data, ref context.cmd);
			}
			else
			{
				PrepareCommandBufferHBAO(data, ref context.cmd);
			}
		}
	}

	private void PrepareCommandBufferHBAO(HbaoPassData data, ref CommandBuffer cmd)
	{
		cmd.SetRenderTarget(data.HbaoRT);
		cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
		cmd.SetGlobalTexture(ShaderProperties._CameraGBufferTexture2, data.CameraNormalsRT);
		cmd.Blit(data.CameraColorRT, data.HbaoRT, m_Material, GetAoPass(data));
		if (data.Settings.BlurAmount.value != 0)
		{
			cmd.Blit(data.HbaoRT, data.HbaoBlurRT, m_Material, GetBlurXPass(data));
			cmd.Blit(data.HbaoBlurRT, data.HbaoRT, m_Material, GetBlurYPass(data));
		}
		cmd.SetGlobalTexture(ShaderProperties.hbaoTex, data.HbaoRT);
		cmd.SetGlobalFloat(ShaderProperties.intensityGlobal, data.Settings.Intensity.value);
		RenderHBAO(data, ref cmd);
	}

	protected int GetBlurXPass(HbaoPassData data)
	{
		return data.Settings.BlurAmount.value switch
		{
			Blur.Narrow => 17, 
			Blur.Medium => 18, 
			Blur.Wide => 19, 
			Blur.ExtraWide => 20, 
			_ => 18, 
		};
	}

	protected int GetBlurYPass(HbaoPassData data)
	{
		return data.Settings.BlurAmount.value switch
		{
			Blur.Narrow => 21, 
			Blur.Medium => 22, 
			Blur.Wide => 23, 
			Blur.ExtraWide => 24, 
			_ => 22, 
		};
	}

	private void RenderHBAO(HbaoPassData data, ref CommandBuffer cmd)
	{
		if (_renderTarget.hdr)
		{
			if (data.Settings.UseMultiBounce.value)
			{
				cmd.Blit(data.CameraColorRT, data.HbaoRT3);
			}
			cmd.Blit(BuiltinRenderTextureType.None, data.CameraColorRT, data.Material, data.Settings.UseMultiBounce.value ? 41 : 40);
			if (data.Settings.ColorBleedingEnabled.value)
			{
				cmd.Blit(BuiltinRenderTextureType.None, data.CameraColorRT, data.Material, 42);
			}
		}
		else
		{
			cmd.Blit(data.CameraColorRT, data.HbaoRT3);
			cmd.SetGlobalTexture(ShaderProperties.rt3Tex, data.HbaoRT3);
			cmd.Blit(data.HbaoRT3, data.CameraColorRT, data.Material, data.Settings.UseMultiBounce.value ? 39 : 38);
		}
	}

	protected void UpdateShaderProperties(HbaoPassData data, ref RenderingData renderingData)
	{
		Camera camera = renderingData.CameraData.Camera;
		_renderTarget.orthographic = camera.orthographic;
		_renderTarget.renderingPath = camera.actualRenderingPath;
		_renderTarget.hdr = renderingData.CameraData.IsHdrEnabled;
		_renderTarget.width = renderingData.CameraData.CameraTargetDescriptor.width;
		_renderTarget.height = renderingData.CameraData.CameraTargetDescriptor.height;
		switch (data.Settings.Resolution.value)
		{
		case Owlcat.Runtime.Visual.Overrides.HBAO.Resolution.Full:
			_renderTarget.downsamplingFactor = 1;
			break;
		case Owlcat.Runtime.Visual.Overrides.HBAO.Resolution.Half:
			_renderTarget.downsamplingFactor = 2;
			break;
		case Owlcat.Runtime.Visual.Overrides.HBAO.Resolution.Quarter:
			_renderTarget.downsamplingFactor = 4;
			break;
		default:
			_renderTarget.downsamplingFactor = 1;
			break;
		}
		_renderTarget.deinterleavingFactor = GetDeinterleavingFactor(data);
		_renderTarget.blurDownsamplingFactor = ((!data.Settings.Downsample.value) ? 1 : 2);
		float num = Mathf.Tan(0.5f * camera.fieldOfView * (MathF.PI / 180f));
		float num2 = 1f / (1f / num * ((float)_renderTarget.height / (float)_renderTarget.width));
		float num3 = 1f / (1f / num);
		m_Material.SetVector(ShaderProperties.uvToView, new Vector4(2f * num2, -2f * num3, -1f * num2, 1f * num3));
		m_Material.SetMatrix(ShaderProperties.worldToCameraMatrix, camera.worldToCameraMatrix);
		if (data.Settings.Deinterleaving.value != 0)
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
		if (noiseTex == null || _quality != data.Settings.Quality.value || _noiseType != data.Settings.NoiseType.value)
		{
			if (noiseTex != null)
			{
				UnityEngine.Object.DestroyImmediate(noiseTex);
			}
			float num4 = ((data.Settings.NoiseType.value == NoiseType.Dither) ? 4 : 64);
			CreateRandomTexture(data, (int)num4);
		}
		_quality = data.Settings.Quality.value;
		_noiseType = data.Settings.NoiseType.value;
		m_Material.SetTexture(ShaderProperties.noiseTex, noiseTex);
		m_Material.SetFloat(ShaderProperties.noiseTexSize, (_noiseType == NoiseType.Dither) ? 4 : 64);
		m_Material.SetFloat(ShaderProperties.radius, data.Settings.Radius.value * 0.5f * ((float)_renderTarget.height / (num * 2f)) / (float)_renderTarget.deinterleavingFactor);
		m_Material.SetFloat(ShaderProperties.maxRadiusPixels, data.Settings.MaxRadiusPixels.value / (float)_renderTarget.deinterleavingFactor);
		m_Material.SetFloat(ShaderProperties.negInvRadius2, -1f / (data.Settings.Radius.value * data.Settings.Radius.value));
		m_Material.SetFloat(ShaderProperties.angleBias, data.Settings.Bias.value);
		m_Material.SetFloat(ShaderProperties.aoMultiplier, 2f * (1f / (1f - data.Settings.Bias.value)));
		m_Material.SetFloat(ShaderProperties.intensity, data.Settings.Intensity.value);
		m_Material.SetFloat(ShaderProperties.multiBounceInfluence, data.Settings.MultiBounceInfluence.value);
		m_Material.SetFloat(ShaderProperties.maxDistance, data.Settings.MaxDistance.value);
		m_Material.SetFloat(ShaderProperties.distanceFalloff, data.Settings.DistanceFalloff.value);
		m_Material.SetColor(ShaderProperties.baseColor, data.Settings.BaseColor.value);
		m_Material.SetFloat(ShaderProperties.colorBleedSaturation, data.Settings.Saturation.value);
		m_Material.SetFloat(ShaderProperties.albedoMultiplier, data.Settings.AlbedoMultiplier.value);
		m_Material.SetFloat(ShaderProperties.colorBleedBrightnessMask, data.Settings.BrightnessMask.value);
		m_Material.SetVector(ShaderProperties.colorBleedBrightnessMaskRange, data.Settings.BrightnessMaskRange.value);
		m_Material.SetFloat(ShaderProperties.blurSharpness, data.Settings.Sharpness.value);
	}

	protected void UpdateShaderKeywords(HbaoPassData data)
	{
		_hbaoShaderKeywords[0] = (data.Settings.ColorBleedingEnabled.value ? "COLOR_BLEEDING_ON" : "__");
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
		CoreUtils.SetKeyword(m_Material, "COLOR_BLEEDING_ON", data.Settings.ColorBleedingEnabled.value);
		CoreUtils.SetKeyword(m_Material, "ORTHOGRAPHIC_PROJECTION_ON", _renderTarget.orthographic);
		CoreUtils.SetKeyword(m_Material, "DEFERRED_SHADING_ON", IsDeferredShading());
	}

	private bool IsDeferredShading()
	{
		return false;
	}

	protected int GetDeinterleavingFactor(HbaoPassData data)
	{
		return data.Settings.Deinterleaving.value switch
		{
			Deinterleaving._2x => 2, 
			Deinterleaving._4x => 4, 
			_ => 1, 
		};
	}

	private void CreateRandomTexture(HbaoPassData data, int size)
	{
		noiseTex = new Texture2D(size, size, TextureFormat.RGB24, mipChain: false, linear: true);
		noiseTex.filterMode = FilterMode.Point;
		noiseTex.wrapMode = TextureWrapMode.Repeat;
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num2 = ((data.Settings.NoiseType == NoiseType.Dither) ? MersenneTwister.Numbers[num++] : UnityEngine.Random.Range(0f, 1f));
				float b = ((data.Settings.NoiseType == NoiseType.Dither) ? MersenneTwister.Numbers[num++] : UnityEngine.Random.Range(0f, 1f));
				float f = MathF.PI * 2f * num2 / (float)_numSampleDirections[GetAoPass(data)];
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
			float f2 = MathF.PI * 2f * num4 / (float)_numSampleDirections[GetAoPass(data)];
			_jitter[k] = new Vector4(Mathf.Cos(f2), Mathf.Sin(f2), z, 0f);
		}
	}

	private int GetAoPass(HbaoPassData data)
	{
		return data.Settings.Quality.value switch
		{
			Quality.Lowest => 0, 
			Quality.Low => 1, 
			Quality.Medium => 2, 
			Quality.High => 3, 
			Quality.Highest => 4, 
			_ => 2, 
		};
	}
}
