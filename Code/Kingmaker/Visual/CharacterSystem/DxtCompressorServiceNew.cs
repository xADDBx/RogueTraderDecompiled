using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Owlcat.Runtime.Core.Utility.Locator;
using StbDxtSharp;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.CharacterSystem;

[BurstCompile]
public class DxtCompressorServiceNew : IService
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate void CompressDelegate(int width, int height, byte* inData, byte* outData, bool hasAlpha);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void Compress_00001738_0024PostfixBurstDelegate(int width, int height, byte* inData, byte* outData, bool hasAlpha);

	internal static class Compress_00001738_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Compress_00001738_0024PostfixBurstDelegate>(Compress).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(int width, int height, byte* inData, byte* outData, bool hasAlpha)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<int, int, byte*, byte*, bool, void>)functionPointer)(width, height, inData, outData, hasAlpha);
					return;
				}
			}
			Compress_0024BurstManaged(width, height, inData, outData, hasAlpha);
		}
	}

	private int m_RequestCount;

	private HashSet<Texture2D> m_OutputTextures = new HashSet<Texture2D>();

	private ComputeShader m_StbDxtMiniMaxShader;

	private int m_KernelStbDxtMiniMax;

	private readonly uint[] m_SyncReadback = new uint[128];

	private static CompressDelegate s_CompressPtr;

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public int RequestsCount => m_RequestCount;

	public DxtCompressorServiceNew()
	{
		m_StbDxtMiniMaxShader = Resources.Load<ComputeShader>("CharacterSystem/StbDxt_GPU_MiniMax");
		if (m_StbDxtMiniMaxShader != null)
		{
			m_KernelStbDxtMiniMax = m_StbDxtMiniMaxShader.FindKernel("CompressStbDxt");
			if (m_KernelStbDxtMiniMax >= 0)
			{
				PFLog.Default.Log("✅ StbDxt_GPU_MiniMax shader loaded");
			}
			else
			{
				PFLog.Default.Warning("⚠\ufe0f StbDxt_GPU_MiniMax kernel not found");
			}
		}
		else
		{
			PFLog.Default.Warning("⚠\ufe0f StbDxt_GPU_MiniMax shader not found");
		}
	}

	[BurstCompile]
	[AOT.MonoPInvokeCallback(typeof(CompressDelegate))]
	public unsafe static void Compress(int width, int height, byte* inData, byte* outData, bool hasAlpha)
	{
		Compress_00001738_0024BurstDirectCall.Invoke(width, height, inData, outData, hasAlpha);
	}

	private unsafe static void CheckCompressPtr()
	{
		if (s_CompressPtr == null)
		{
			s_CompressPtr = BurstCompiler.CompileFunctionPointer<CompressDelegate>(Compress).Invoke;
		}
	}

	public async Task<Texture2D> CompressTexture(Texture textureIn)
	{
		CheckCompressPtr();
		Texture2D textureOut = null;
		m_RequestCount++;
		try
		{
			bool linear = !GraphicsFormatUtility.IsSRGBFormat(textureIn.graphicsFormat);
			bool hasAlpha = GraphicsFormatUtility.HasAlphaChannel(textureIn.graphicsFormat);
			bool num = textureIn.mipmapCount > 1;
			int width = textureIn.width;
			int height = textureIn.height;
			NativeArray<byte>[] mips = await (num ? ExtractMipsSanePlatforms(textureIn) : ExtractMipsSwitch2(textureIn));
			textureOut = new Texture2D(width, height, hasAlpha ? TextureFormat.DXT5 : TextureFormat.DXT1, mipChain: true, linear, createUninitialized: true)
			{
				filterMode = textureIn.filterMode,
				wrapMode = textureIn.wrapMode,
				anisoLevel = textureIn.anisoLevel,
				name = textureIn.name + "_Compressed_CPU"
			};
			m_OutputTextures.Add(textureOut);
			NativeArray<byte> outData = textureOut.GetRawTextureData<byte>();
			await Awaitable.BackgroundThreadAsync();
			CompressImpl(mips, outData, width, height, hasAlpha);
			await Awaitable.MainThreadAsync();
			NativeArray<byte>[] array = mips;
			foreach (NativeArray<byte> nativeArray in array)
			{
				nativeArray.Dispose();
			}
			textureOut.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			return textureOut;
		}
		finally
		{
			m_OutputTextures.Remove(textureOut);
			m_RequestCount--;
		}
	}

	public Task<Texture2D> CompressTextureGPU(Texture textureIn, string atlasType = "diffuse")
	{
		return Task.FromResult(CompressTextureGPUSync(textureIn, 0, 0, atlasType));
	}

	public Texture2D CompressTextureGPUSync(Texture textureIn, int targetWidth = 0, int targetHeight = 0, string atlasType = "diffuse")
	{
		Texture2D texture2D = null;
		m_RequestCount++;
		try
		{
			if (m_StbDxtMiniMaxShader == null || m_KernelStbDxtMiniMax < 0)
			{
				PFLog.Default.Error("❌ StbDxt_GPU_MiniMax shader not available");
				return null;
			}
			bool flag = GraphicsFormatUtility.IsSRGBFormat(textureIn.graphicsFormat);
			int num = ((targetWidth > 0) ? targetWidth : textureIn.width);
			int num2 = ((targetHeight > 0) ? targetHeight : textureIn.height);
			int num3 = num / 4;
			int num4 = num2 / 4;
			RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			Graphics.Blit(textureIn, temporary);
			RenderTexture temporary2 = RenderTexture.GetTemporary(num3, num4, 0, GraphicsFormat.R32G32B32A32_UInt);
			temporary2.enableRandomWrite = true;
			temporary2.useMipMap = false;
			temporary2.autoGenerateMips = false;
			temporary2.Create();
			ComputeBuffer computeBuffer = new ComputeBuffer(32, 16);
			m_StbDxtMiniMaxShader.SetInt("InputWidth", num);
			m_StbDxtMiniMaxShader.SetInt("InputHeight", num2);
			m_StbDxtMiniMaxShader.SetInt("SRGBMode", flag ? 1 : 0);
			m_StbDxtMiniMaxShader.SetInt("DebugEnabled", 0);
			m_StbDxtMiniMaxShader.SetBuffer(m_KernelStbDxtMiniMax, "DebugBuffer", computeBuffer);
			m_StbDxtMiniMaxShader.SetTexture(m_KernelStbDxtMiniMax, "InputRGBA32", temporary);
			m_StbDxtMiniMaxShader.SetTexture(m_KernelStbDxtMiniMax, "OutputDXT5", temporary2);
			int threadGroupsX = Mathf.Max(1, Mathf.CeilToInt((float)num3 / 8f));
			int threadGroupsY = Mathf.Max(1, Mathf.CeilToInt((float)num4 / 8f));
			m_StbDxtMiniMaxShader.Dispatch(m_KernelStbDxtMiniMax, threadGroupsX, threadGroupsY, 1);
			computeBuffer.Dispose();
			bool linear = !flag;
			texture2D = new Texture2D(num, num2, TextureFormat.DXT5, mipChain: false, linear);
			texture2D.name = atlasType + "_Compressed_GPU";
			texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			Graphics.CopyTexture(temporary2, 0, 0, 0, 0, num3, num4, texture2D, 0, 0, 0, 0);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			return texture2D;
		}
		finally
		{
			m_OutputTextures.Remove(texture2D);
			m_RequestCount--;
		}
	}

	private static async Task<NativeArray<byte>[]> ExtractMipsSwitch2(Texture textureIn)
	{
		bool linear = !GraphicsFormatUtility.IsSRGBFormat(textureIn.graphicsFormat);
		TextureFormat texFormat = GraphicsFormatUtility.GetTextureFormat(textureIn.graphicsFormat);
		AsyncGPUReadbackRequest asyncGPUReadbackRequest = await AsyncGPUReadback.RequestAsync(textureIn);
		if (asyncGPUReadbackRequest.hasError)
		{
			throw new Exception("AsyncGPUReadback failed on texture " + textureIn.name + ".");
		}
		NativeArray<byte> data = asyncGPUReadbackRequest.GetData<byte>();
		int width = textureIn.width;
		int height = textureIn.height;
		Texture2D texture2D = new Texture2D(width, height, texFormat, mipChain: true, linear, createUninitialized: true)
		{
			filterMode = textureIn.filterMode,
			wrapMode = textureIn.wrapMode,
			anisoLevel = textureIn.anisoLevel,
			name = textureIn.name + "_Temp"
		};
		texture2D.SetPixelData(data, 0);
		texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		NativeArray<byte>[] array = new NativeArray<byte>[texture2D.mipmapCount];
		for (int i = 0; i < texture2D.mipmapCount; i++)
		{
			array[i] = new NativeArray<byte>(texture2D.GetPixelData<byte>(i), Allocator.Persistent);
		}
		UnityEngine.Object.Destroy(texture2D);
		return array;
	}

	private static async Task<NativeArray<byte>[]> ExtractMipsSanePlatforms(Texture textureIn)
	{
		Task<NativeArray<byte>>[] array = new Task<NativeArray<byte>>[textureIn.mipmapCount];
		for (int i = 0; i < textureIn.mipmapCount; i++)
		{
			array[i] = ReadbackImpl(textureIn, i);
		}
		return await Task.WhenAll(array);
	}

	private static async Task<NativeArray<byte>> ReadbackImpl(Texture textureIn, int mip)
	{
		AsyncGPUReadbackRequest asyncGPUReadbackRequest = await AsyncGPUReadback.RequestAsync(textureIn, mip);
		if (!asyncGPUReadbackRequest.hasError)
		{
			return new NativeArray<byte>(asyncGPUReadbackRequest.GetData<byte>(), Allocator.Persistent);
		}
		throw new Exception($"AsyncGPUReadback failed on texture {textureIn.name}, mip {mip}.");
	}

	private static void CompressImpl(NativeArray<byte>[] mips, NativeArray<byte> outData, int width, int height, bool hasAlpha)
	{
		for (int i = 0; i < mips.Length; i++)
		{
			NativeArray<byte> inData = mips[i];
			int num = width;
			int num2 = height;
			int num3 = 0;
			int num4 = i;
			for (int j = 0; j < num4; j++)
			{
				num3 += num * num2 / (hasAlpha ? 1 : 2);
				num /= 2;
				num2 /= 2;
			}
			if (num >= 4 && num2 >= 4)
			{
				int length = num * num2 / (hasAlpha ? 1 : 2);
				NativeArray<byte> subArray = outData.GetSubArray(num3, length);
				CompressImplUnsafe(inData, subArray, num, num2, hasAlpha);
			}
		}
	}

	private unsafe static void CompressImplUnsafe(NativeArray<byte> inData, NativeArray<byte> outData, int width, int height, bool hasAlpha)
	{
		byte* unsafePtr = (byte*)inData.GetUnsafePtr();
		byte* unsafePtr2 = (byte*)outData.GetUnsafePtr();
		s_CompressPtr(width, height, unsafePtr, unsafePtr2, hasAlpha);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	[AOT.MonoPInvokeCallback(typeof(CompressDelegate))]
	internal unsafe static void Compress_0024BurstManaged(int width, int height, byte* inData, byte* outData, bool hasAlpha)
	{
		DxtContext ctx = default(DxtContext);
		ctx.Init();
		if (hasAlpha)
		{
			StbDxt.CompressDxt5(ctx, width, height, inData, outData);
		}
		else
		{
			StbDxt.CompressDxt1(ctx, width, height, inData, outData);
		}
	}
}
