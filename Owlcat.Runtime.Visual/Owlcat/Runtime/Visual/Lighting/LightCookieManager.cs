using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Owlcat.Runtime.Visual.Collections;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Lighting;

public class LightCookieManager : IDisposable
{
	public struct Settings
	{
		public int2 atlasTextureResolution;

		public GraphicsFormat atlasTextureFormat;

		public static Settings Default
		{
			get
			{
				Settings result = default(Settings);
				result.atlasTextureResolution = new int2(1024, 1024);
				result.atlasTextureFormat = GraphicsFormat.R8G8B8A8_SRGB;
				return result;
			}
		}
	}

	[BurstCompile]
	private struct Statistics
	{
		public int LightsCount;

		public int CookieLightsCount;

		public int UsedCookieLightsCount;

		public bool LightLimitOverflowDetected;

		public bool AtlasOverflowDetected;

		public void Append(StringBuilder builder)
		{
			builder.Append("( lights_count: ").Append(LightsCount).Append(", cookie_light_count: ")
				.Append(CookieLightsCount)
				.Append("/")
				.Append(LightsCount)
				.Append(", used_cookie_light_count: ")
				.Append(UsedCookieLightsCount)
				.Append("/")
				.Append(CookieLightsCount)
				.Append(", atlas_overflow_detected: ")
				.Append(AtlasOverflowDetected)
				.Append(", light_limit_overflow_detected: ")
				.Append(LightLimitOverflowDetected)
				.Append(")");
		}
	}

	[BurstCompile]
	private struct LightCookieData
	{
		public int lightDescriptorIndex;

		public LightCookieDescriptor descriptor;

		public LightCookieData(int lightDescriptorIndex, in LightCookieDescriptor descriptor)
		{
			this.lightDescriptorIndex = lightDescriptorIndex;
			this.descriptor = descriptor;
		}

		public override string ToString()
		{
			return $"(lightDescriptorIndex:{lightDescriptorIndex}, descriptor:{descriptor}";
		}
	}

	[BurstCompile]
	private readonly struct BlitCommandData
	{
		public readonly int lightDescriptorIndex;

		public readonly float4 scaleOffset;

		public BlitCommandData(int lightDescriptorIndex, float4 scaleOffset)
		{
			this.lightDescriptorIndex = lightDescriptorIndex;
			this.scaleOffset = scaleOffset;
		}

		public override string ToString()
		{
			return $"(lightDescriptorIndex:{lightDescriptorIndex}, scaleOffset:{scaleOffset})";
		}
	}

	[BurstCompile]
	private struct LightCookieAtlas : IDisposable
	{
		private struct Allocation
		{
			public uint textureVersion;

			public float4 uvScaleOffset;

			public Allocation(uint textureVersion, float4 uvScaleOffset)
			{
				this.textureVersion = textureVersion;
				this.uvScaleOffset = uvScaleOffset;
			}
		}

		private AtlasLinearAllocator m_Allocator;

		private NativeHashMap<int, Allocation> m_AllocationCache;

		public int AllocationCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_AllocationCache.Count;
			}
		}

		public int2 Size
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Allocator.Size;
			}
		}

		public LightCookieAtlas(int2 size, int capacity, Allocator allocator)
		{
			m_Allocator = new AtlasLinearAllocator(size, capacity, allocator);
			m_AllocationCache = new NativeHashMap<int, Allocation>(capacity, allocator);
		}

		public void Dispose()
		{
			m_Allocator.Dispose();
			m_AllocationCache.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			m_Allocator.Reset();
			m_AllocationCache.Clear();
		}

		public bool Allocate(int textureId, uint textureVersion, int2 textureSize, bool checkCache, ref float4 uvScaleOffset, ref bool shouldUpdate)
		{
			if (checkCache && m_AllocationCache.TryGetValue(textureId, out var item))
			{
				bool flag = textureVersion != item.textureVersion;
				if (flag)
				{
					item.textureVersion = textureVersion;
					m_AllocationCache[textureId] = item;
				}
				uvScaleOffset = item.uvScaleOffset;
				shouldUpdate = flag;
				return true;
			}
			if (m_Allocator.Allocate(in textureSize, ref uvScaleOffset))
			{
				m_AllocationCache.Add(textureId, new Allocation(textureVersion, uvScaleOffset));
				shouldUpdate = true;
				return true;
			}
			return false;
		}
	}

	private sealed class LightCookiePass : ScriptableRenderPass
	{
		private sealed class PassData
		{
			public bool hasAnyLightWithCookie;

			public TextureHandle atlasTexture;

			public List<Texture> blitCookieTextures;

			public NativeReference<LightCookieConstantBuffer> cookieConstantBufferReference;

			public NativeList<BlitCommandData> blitCommands;

			public LightCookieShaderFormat lightCookieShaderFormat;

			public TexturePackedChannelsInfo atlasChannelsInfo;
		}

		private readonly struct TexturePackedChannelsInfo
		{
			public readonly uint componentCount;

			public readonly int swizzleMask;

			public TexturePackedChannelsInfo(GraphicsFormat graphicsFormat)
			{
				componentCount = GraphicsFormatUtility.GetComponentCount(graphicsFormat);
				swizzleMask = (1 << (int)(GraphicsFormatUtility.GetSwizzleA(graphicsFormat) & (FormatSwizzle)7) << 24) | (1 << (int)(GraphicsFormatUtility.GetSwizzleB(graphicsFormat) & (FormatSwizzle)7) << 16) | (1 << (int)(GraphicsFormatUtility.GetSwizzleG(graphicsFormat) & (FormatSwizzle)7) << 8) | (1 << (int)(GraphicsFormatUtility.GetSwizzleR(graphicsFormat) & (FormatSwizzle)7));
			}
		}

		private static readonly int kLightCookieAtlasId = Shader.PropertyToID("_LightCookieAtlas");

		private static readonly int kLightCookieAtlasFormatId = Shader.PropertyToID("_LightCookieAtlasFormat");

		private static readonly int kLightCookieConstantBufferId = Shader.PropertyToID("LightCookieConstantBuffer");

		private static readonly Vector4 kIdentityScaleOffset = new Vector4(1f, 1f, 0f, 0f);

		private static readonly BaseRenderFunc<PassData, RenderGraphContext> s_RenderFunc = Render;

		private readonly TextureHandle m_AtlasTexture;

		private readonly List<Texture> m_BlitCookieTextures;

		private readonly NativeList<LightCookieData> m_CookieDataList;

		private readonly NativeList<BlitCommandData> m_BlitCommandDataList;

		private readonly NativeReference<LightCookieConstantBuffer> m_CookieConstantBufferReference;

		private readonly LightCookieShaderFormat m_LightCookieShaderFormat;

		private readonly TexturePackedChannelsInfo m_AtlasChannelsInfo;

		public override string Name => "Update Light Cookie";

		public LightCookiePass(TextureHandle atlasTexture, GraphicsFormat atlasGraphicsFormat, LightCookieShaderFormat lightCookieShaderFormat, List<Texture> blitCookieTextures, NativeList<LightCookieData> cookieDataList, NativeList<BlitCommandData> blitCommandDataList, NativeReference<LightCookieConstantBuffer> cookieConstantBufferReference)
			: base(RenderPassEvent.BeforeRendering)
		{
			m_AtlasTexture = atlasTexture;
			m_AtlasChannelsInfo = new TexturePackedChannelsInfo(atlasGraphicsFormat);
			m_BlitCookieTextures = blitCookieTextures;
			m_CookieDataList = cookieDataList;
			m_BlitCommandDataList = blitCommandDataList;
			m_CookieConstantBufferReference = cookieConstantBufferReference;
			m_LightCookieShaderFormat = lightCookieShaderFormat;
		}

		protected override void RecordRenderGraph(ref RenderingData renderingData)
		{
			PassData passData;
			using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<PassData>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Lighting\\LightCookieManager.cs", 406);
			passData.hasAnyLightWithCookie = m_CookieDataList.Length > 0;
			passData.atlasTexture = m_AtlasTexture;
			passData.blitCookieTextures = m_BlitCookieTextures;
			passData.blitCommands = m_BlitCommandDataList;
			passData.cookieConstantBufferReference = m_CookieConstantBufferReference;
			passData.lightCookieShaderFormat = m_LightCookieShaderFormat;
			passData.atlasChannelsInfo = m_AtlasChannelsInfo;
			renderGraphBuilder.AllowPassCulling(value: false);
			renderGraphBuilder.ReadWriteTexture(in m_AtlasTexture);
			renderGraphBuilder.SetRenderFunc(s_RenderFunc);
		}

		private static void Render(PassData passData, RenderGraphContext context)
		{
			CommandBuffer cmd = context.cmd;
			SetupGlobalState(cmd, in passData);
			cmd.SetRenderTarget(passData.atlasTexture);
			UpdateAtlas(cmd, in passData);
			cmd.SetGlobalTexture(kLightCookieAtlasId, passData.atlasTexture);
			context.renderContext.ExecuteCommandBuffer(cmd);
			cmd.Clear();
		}

		private static void SetupGlobalState(CommandBuffer cmd, in PassData passData)
		{
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings._LIGHT_COOKIES, passData.hasAnyLightWithCookie);
			if (passData.hasAnyLightWithCookie)
			{
				cmd.SetGlobalFloat(kLightCookieAtlasFormatId, (float)passData.lightCookieShaderFormat);
				ConstantBuffer.PushGlobal(cmd, in UnsafeCollectionExtensions.AsRef(in passData.cookieConstantBufferReference), kLightCookieConstantBufferId);
			}
		}

		private static void UpdateAtlas(CommandBuffer cmd, in PassData passData)
		{
			int length = passData.blitCommands.Length;
			for (int i = 0; i < length; i++)
			{
				BlitCommandData blitCommandData = passData.blitCommands[i];
				Texture texture = passData.blitCookieTextures[i];
				TexturePackedChannelsInfo source = new TexturePackedChannelsInfo(texture.graphicsFormat);
				bool flag = IsSingleChannelBlit(in source, in passData.atlasChannelsInfo);
				if (texture.dimension == TextureDimension.Tex2D)
				{
					if (flag)
					{
						Blitter.BlitQuadSingleChannel(cmd, texture, kIdentityScaleOffset, blitCommandData.scaleOffset, 0);
					}
					else
					{
						Blitter.BlitQuad(cmd, texture, kIdentityScaleOffset, blitCommandData.scaleOffset, 0, bilinear: true);
					}
				}
				else if (texture.dimension == TextureDimension.Cube)
				{
					if (flag)
					{
						Blitter.BlitCubeToOctahedral2DQuadSingleChannel(cmd, texture, blitCommandData.scaleOffset, 0);
					}
					else
					{
						Blitter.BlitCubeToOctahedral2DQuad(cmd, texture, blitCommandData.scaleOffset, 0);
					}
				}
			}
		}

		private static bool IsSingleChannelBlit(in TexturePackedChannelsInfo source, in TexturePackedChannelsInfo dest)
		{
			if (source.componentCount == 1 || dest.componentCount == 1)
			{
				if (source.componentCount != dest.componentCount)
				{
					return true;
				}
				if (source.swizzleMask != dest.swizzleMask)
				{
					return true;
				}
			}
			return false;
		}
	}

	[BurstCompile]
	private struct Job : IJob
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct CookieDataComparer : IComparer<LightCookieData>
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int Compare(LightCookieData a, LightCookieData b)
			{
				bool num = a.lightDescriptorIndex >= 0;
				bool flag = b.lightDescriptorIndex >= 0;
				if (!num)
				{
					if (!flag)
					{
						return 0;
					}
					return 1;
				}
				if (!flag)
				{
					return -1;
				}
				int num2 = b.descriptor.textureSize.x * b.descriptor.textureSize.y - a.descriptor.textureSize.x * a.descriptor.textureSize.y;
				if (num2 != 0)
				{
					return num2;
				}
				return a.descriptor.textureId - b.descriptor.textureId;
			}
		}

		private static readonly float4x4 s_DirectionalLightProjectionMatrix = float4x4.Ortho(1f, 1f, -0.5f, 0.5f);

		private const int kTextureSizeMin = 4;

		private const int kSizeDivisorMax = 16;

		public NativeArray<LightDescriptor> lightDescriptorArray;

		public NativeList<LightCookieData> lightCookieDataList;

		public LightCookieAtlas atlas;

		public NativeReference<int> lastSizeDivisorReference;

		public NativeList<float4> lightCookieScaleOffsetList;

		[WriteOnly]
		public NativeList<BlitCommandData> blitCommandDataList;

		[WriteOnly]
		public NativeReference<LightCookieConstantBuffer> cookieConstantBufferReference;

		public NativeReference<Statistics> statisticsReference;

		public void Execute()
		{
			statisticsReference.Value = default(Statistics);
			PopulateLightCookieData();
			LayoutAtlas();
			PopulateConstantBuffer();
			TransferLightCookieIndices();
		}

		private unsafe void PopulateLightCookieData()
		{
			Statistics* unsafePtr = statisticsReference.GetUnsafePtr();
			unsafePtr->LightsCount = lightDescriptorArray.Length;
			lightCookieDataList.ResizeUninitialized(lightDescriptorArray.Length);
			int num = 0;
			int i = 0;
			for (int length = lightDescriptorArray.Length; i < length; i++)
			{
				LightDescriptor lightDescriptor = lightDescriptorArray[i];
				if (lightDescriptor.lightCookieDescriptor.textureId != 0 && (lightDescriptor.lightCookieDescriptor.textureDimension == TextureDimension.Tex2D || lightDescriptor.lightCookieDescriptor.textureDimension == TextureDimension.Cube))
				{
					lightCookieDataList[num++] = new LightCookieData(i, in lightDescriptor.lightCookieDescriptor);
				}
			}
			lightCookieDataList.AsArray().Slice(0, num).Sort(default(CookieDataComparer));
			lightCookieDataList.ResizeUninitialized(math.min(num, 128));
			unsafePtr->CookieLightsCount = num;
			unsafePtr->UsedCookieLightsCount = lightCookieDataList.Length;
			unsafePtr->LightLimitOverflowDetected = num > 128;
		}

		private unsafe void LayoutAtlas()
		{
			Statistics* unsafePtr = statisticsReference.GetUnsafePtr();
			int* unsafePtr2 = lastSizeDivisorReference.GetUnsafePtr();
			if (atlas.AllocationCount > 0 && AllocateCookieTextures(*unsafePtr2, checkCache: true))
			{
				return;
			}
			*unsafePtr2 = PredictSizeDivisor();
			while (true)
			{
				atlas.Clear();
				if (!AllocateCookieTextures(*unsafePtr2, checkCache: false))
				{
					if (*unsafePtr2 >= 16)
					{
						unsafePtr->AtlasOverflowDetected = true;
						break;
					}
					(*unsafePtr2)++;
					continue;
				}
				break;
			}
		}

		private void PopulateConstantBuffer()
		{
			PopulateConstantBufferLightCookieMatrices();
			PopulateConstantBufferLightCookieUvRects();
		}

		public unsafe void TransferLightCookieIndices()
		{
			LightCookieData* ptr = lightCookieDataList.GetUnsafeReadOnlyPtr();
			LightDescriptor* unsafePtr = (LightDescriptor*)lightDescriptorArray.GetUnsafePtr();
			int num = 0;
			int length = lightCookieDataList.Length;
			while (num < length)
			{
				unsafePtr[ptr->lightDescriptorIndex].LightCookieIndex = num;
				num++;
				ptr++;
			}
		}

		private unsafe void PopulateConstantBufferLightCookieMatrices()
		{
			int length = lightCookieDataList.Length;
			float4x4* ptr = (float4x4*)cookieConstantBufferReference.GetUnsafePtr()->_LightCookieMatrices;
			int num = 0;
			while (num < length)
			{
				LightDescriptor lightDescriptor = lightDescriptorArray[lightCookieDataList[num].lightDescriptorIndex];
				float4x4 float4x = math.inverse(lightDescriptor.VisibleLight.localToWorldMatrix);
				if (lightDescriptor.VisibleLight.lightType == LightType.Spot)
				{
					float4x4 a = float4x4.PerspectiveFov(math.radians(lightDescriptor.VisibleLight.spotAngle), 1f, 0.001f, lightDescriptor.VisibleLight.range);
					float4x4 lightUvTransformationMatrix = GetLightUvTransformationMatrix(in lightDescriptor);
					float4x = math.mul(a, math.mul(lightUvTransformationMatrix, float4x));
				}
				else if (lightDescriptor.VisibleLight.lightType == LightType.Directional)
				{
					float4x4 lightUvTransformationMatrix2 = GetLightUvTransformationMatrix(in lightDescriptor);
					float4x = math.mul(s_DirectionalLightProjectionMatrix, math.mul(lightUvTransformationMatrix2, float4x));
				}
				*ptr = float4x;
				num++;
				ptr++;
			}
		}

		private unsafe void PopulateConstantBufferLightCookieUvRects()
		{
			UnsafeUtility.MemCpy(cookieConstantBufferReference.GetUnsafePtr()->_LightCookieUVRects, lightCookieScaleOffsetList.GetUnsafeReadOnlyPtr(), lightCookieScaleOffsetList.Length * sizeof(float4));
		}

		private float4x4 GetLightUvTransformationMatrix(in LightDescriptor lightDescriptor)
		{
			float2 @float = new float2(1f, 1f) / lightDescriptor.lightCookieDescriptor.uvSize;
			float2 uvOffset = lightDescriptor.lightCookieDescriptor.uvOffset;
			float4x4 result = float4x4.Scale(@float.x, @float.y, 1f);
			result.c3 = new float4((0f - uvOffset.x) * @float.x, (0f - uvOffset.y) * @float.y, 0f, 1f);
			return result;
		}

		private bool AllocateCookieTextures(int sizeDivisor, bool checkCache)
		{
			lightCookieScaleOffsetList.Clear();
			blitCommandDataList.Clear();
			int num = 0;
			float4 uvScaleOffset = default(float4);
			float4 value = default(float4);
			bool shouldUpdate = false;
			int i = 0;
			for (int length = lightCookieDataList.Length; i < length; i++)
			{
				LightCookieData lightCookieData = lightCookieDataList[i];
				if (num != lightCookieData.descriptor.textureId)
				{
					int2 allocationTextureSize = GetAllocationTextureSize(in lightCookieData.descriptor, sizeDivisor);
					if (!atlas.Allocate(lightCookieData.descriptor.textureId, lightCookieData.descriptor.textureVersion, allocationTextureSize, checkCache, ref uvScaleOffset, ref shouldUpdate))
					{
						return false;
					}
					value = GetSampleUvScaleOffset(uvScaleOffset, allocationTextureSize);
					if (shouldUpdate)
					{
						ref NativeList<BlitCommandData> reference = ref blitCommandDataList;
						BlitCommandData value2 = new BlitCommandData(lightCookieData.lightDescriptorIndex, uvScaleOffset);
						reference.Add(in value2);
					}
					num = lightCookieData.descriptor.textureId;
				}
				lightCookieScaleOffsetList.Add(in value);
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int PredictSizeDivisor()
		{
			return (int)math.max(math.ceil(math.sqrt((float)GetTotalCookiePixelsCount() / (float)(atlas.Size.x * atlas.Size.y))), 1f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetTotalCookiePixelsCount()
		{
			int num = 0;
			foreach (LightCookieData lightCookieData in lightCookieDataList)
			{
				num += lightCookieData.descriptor.textureSize.x * lightCookieData.descriptor.textureSize.y;
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int2 GetAllocationTextureSize(in LightCookieDescriptor descriptor, int sizeDivisor)
		{
			TextureDimension textureDimension = descriptor.textureDimension;
			if (textureDimension != TextureDimension.Tex2D && textureDimension == TextureDimension.Cube)
			{
				return math.max(math.cmax(descriptor.textureSize) * 2 / sizeDivisor, 4);
			}
			return math.max(descriptor.textureSize / sizeDivisor, new int2(4, 4));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float4 GetSampleUvScaleOffset(float4 atlasUvScaleOffset, float2 cookieSizeInAtlas)
		{
			float2 @float = atlasUvScaleOffset.xy * new float2(0.5f / cookieSizeInAtlas);
			float2 float2 = (cookieSizeInAtlas - new float2(1)) / cookieSizeInAtlas;
			atlasUvScaleOffset.z += @float.x;
			atlasUvScaleOffset.w += @float.y;
			atlasUvScaleOffset.x *= float2.x;
			atlasUvScaleOffset.y *= float2.y;
			return atlasUvScaleOffset;
		}
	}

	private readonly RenderGraph m_RenderGraph;

	private const int kMaxAtlasAllocationCount = 512;

	private const int kMaxCookieCount = 128;

	private readonly TextureHandle m_AtlasTextureHandle;

	private LightCookieAtlas m_Atlas;

	private NativeList<LightCookieData> m_LightCookieDataList;

	private NativeList<float4> m_CookieScaleOffsetList;

	private NativeList<BlitCommandData> m_BlitCommandDataList;

	private NativeReference<LightCookieConstantBuffer> m_CookieConstantBufferReference;

	private NativeReference<int> m_LastSizeDivisorReference;

	private NativeReference<Statistics> m_StatisticsReference;

	private readonly LightCookiePass m_LightCookiePass;

	private readonly List<Texture> m_CookieTextures;

	private readonly StringBuilder m_MessageBuilder = new StringBuilder();

	public LightCookieManager(RenderGraph renderGraph, in Settings settings)
	{
		m_RenderGraph = renderGraph;
		m_LightCookieDataList = new NativeList<LightCookieData>(Allocator.Persistent);
		m_CookieScaleOffsetList = new NativeList<float4>(Allocator.Persistent);
		m_BlitCommandDataList = new NativeList<BlitCommandData>(Allocator.Persistent);
		m_CookieConstantBufferReference = new NativeReference<LightCookieConstantBuffer>(Allocator.Persistent);
		m_LastSizeDivisorReference = new NativeReference<int>(Allocator.Persistent);
		m_StatisticsReference = new NativeReference<Statistics>(Allocator.Persistent);
		m_Atlas = new LightCookieAtlas(settings.atlasTextureResolution, 512, Allocator.Persistent);
		TextureDesc desc = new TextureDesc(settings.atlasTextureResolution.x, settings.atlasTextureResolution.y)
		{
			colorFormat = settings.atlasTextureFormat,
			filterMode = FilterMode.Bilinear,
			wrapMode = TextureWrapMode.Clamp,
			useMipMap = false,
			autoGenerateMips = false,
			name = "Light Cookie Atlas"
		};
		m_AtlasTextureHandle = m_RenderGraph.CreateSharedTexture(in desc, explicitRelease: true);
		m_CookieTextures = new List<Texture>();
		m_LightCookiePass = new LightCookiePass(m_AtlasTextureHandle, settings.atlasTextureFormat, blitCookieTextures: m_CookieTextures, cookieDataList: m_LightCookieDataList, blitCommandDataList: m_BlitCommandDataList, lightCookieShaderFormat: GetLightCookieShaderFormat(settings.atlasTextureFormat), cookieConstantBufferReference: m_CookieConstantBufferReference);
	}

	public void Dispose()
	{
		m_RenderGraph.ReleaseSharedTexture(m_AtlasTextureHandle);
		m_Atlas.Dispose();
		m_LightCookieDataList.Dispose();
		m_CookieScaleOffsetList.Dispose();
		m_BlitCommandDataList.Dispose();
		m_CookieConstantBufferReference.Dispose();
		m_LastSizeDivisorReference.Dispose();
		m_StatisticsReference.Dispose();
	}

	internal JobHandle ScheduleSetupJobs(ref NativeArray<LightDescriptor> lightDescriptors, JobHandle dependency)
	{
		m_LightCookieDataList.Clear();
		m_CookieScaleOffsetList.Clear();
		m_BlitCommandDataList.Clear();
		m_CookieTextures.Clear();
		Job jobData = default(Job);
		jobData.lightDescriptorArray = lightDescriptors;
		jobData.lightCookieDataList = m_LightCookieDataList;
		jobData.atlas = m_Atlas;
		jobData.lastSizeDivisorReference = m_LastSizeDivisorReference;
		jobData.lightCookieScaleOffsetList = m_CookieScaleOffsetList;
		jobData.blitCommandDataList = m_BlitCommandDataList;
		jobData.cookieConstantBufferReference = m_CookieConstantBufferReference;
		jobData.statisticsReference = m_StatisticsReference;
		return jobData.Schedule(dependency);
	}

	internal void FinishSetup(ScriptableRenderer scriptableRenderer, ref NativeArray<LightDescriptor> lightDescriptors, ref RenderingData renderingData)
	{
		PopulateCookieTextureList(ref lightDescriptors);
		scriptableRenderer.EnqueuePass(m_LightCookiePass);
		ReportStatistics(in renderingData);
	}

	private void ReportStatistics(in RenderingData renderingData)
	{
	}

	private unsafe void PopulateCookieTextureList(ref NativeArray<LightDescriptor> lightDescriptors)
	{
		m_CookieTextures.Clear();
		BlitCommandData* ptr = m_BlitCommandDataList.GetUnsafePtr();
		int num = 0;
		int length = m_BlitCommandDataList.Length;
		while (num < length)
		{
			Texture cookie = lightDescriptors[ptr->lightDescriptorIndex].VisibleLight.light.cookie;
			m_CookieTextures.Add(cookie);
			num++;
			ptr++;
		}
	}

	private static LightCookieShaderFormat GetLightCookieShaderFormat(GraphicsFormat cookieFormat)
	{
		switch (cookieFormat)
		{
		case (GraphicsFormat)54:
		case (GraphicsFormat)55:
			return LightCookieShaderFormat.Alpha;
		case GraphicsFormat.R8_SRGB:
		case GraphicsFormat.R8_UNorm:
		case GraphicsFormat.R8_SNorm:
		case GraphicsFormat.R8_UInt:
		case GraphicsFormat.R8_SInt:
		case GraphicsFormat.R16_UNorm:
		case GraphicsFormat.R16_SNorm:
		case GraphicsFormat.R16_UInt:
		case GraphicsFormat.R16_SInt:
		case GraphicsFormat.R32_UInt:
		case GraphicsFormat.R32_SInt:
		case GraphicsFormat.R16_SFloat:
		case GraphicsFormat.R32_SFloat:
			return LightCookieShaderFormat.Red;
		default:
			return LightCookieShaderFormat.RGB;
		}
	}
}
