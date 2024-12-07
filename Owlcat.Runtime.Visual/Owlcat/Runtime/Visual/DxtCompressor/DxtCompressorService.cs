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
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.DxtCompressor;

public class DxtCompressorService : IService
{
	public enum Compression
	{
		Dxt1,
		Dxt5
	}

	[BurstCompile]
	public class Request
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void CompressDelegate(int width, int height, byte* inData, byte* outData, bool hasAlpha);

		public unsafe delegate void Compress_00000E0A_0024PostfixBurstDelegate(int width, int height, byte* inData, byte* outData, bool hasAlpha);

		internal static class Compress_00000E0A_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			private static IntPtr DeferredCompilation;

			[BurstDiscard]
			private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Compress_00000E0A_0024PostfixBurstDelegate).TypeHandle);
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public static void Constructor()
			{
				DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
			}

			public static void Initialize()
			{
			}

			static Compress_00000E0A_0024BurstDirectCall()
			{
				Constructor();
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

		private static CompressDelegate CompressPtr;

		public float StartTime;

		private Compression Compression;

		private bool m_IsComplete;

		private Dictionary<int, NativeArray<byte>> SourceBytes = new Dictionary<int, NativeArray<byte>>();

		private int m_SkipMips;

		public bool HasError { get; private set; }

		public string ErrorText { get; private set; }

		public Texture TextureIn { get; private set; }

		public Texture2D TextureOut { get; private set; }

		public bool IsComplete
		{
			get
			{
				if (!HasError)
				{
					return m_IsComplete;
				}
				return true;
			}
		}

		public event Action<Request> OnDone;

		[BurstCompile]
		[MonoPInvokeCallback(typeof(CompressDelegate))]
		public unsafe static void Compress(int width, int height, byte* inData, byte* outData, bool hasAlpha)
		{
			Compress_00000E0A_0024BurstDirectCall.Invoke(width, height, inData, outData, hasAlpha);
		}

		public unsafe Request(Texture textureIn, Texture2D textureOut, Compression compression, int skipMips)
		{
			TextureIn = textureIn;
			TextureOut = textureOut;
			Compression = compression;
			m_SkipMips = skipMips;
			StartTime = Time.time;
			if (CompressPtr == null)
			{
				CompressPtr = BurstCompiler.CompileFunctionPointer<CompressDelegate>(Compress).Invoke;
			}
		}

		private void Error(string errorText)
		{
			HasError = true;
			ErrorText = errorText;
		}

		public void Run()
		{
			if (TextureIn == null)
			{
				Error("Texture already removed");
				return;
			}
			for (int i = 0; i < TextureIn.mipmapCount; i++)
			{
				int mipIndex = i;
				AsyncGPUReadback.Request(TextureIn, mipIndex, TextureFormat.RGBA32, delegate(AsyncGPUReadbackRequest r)
				{
					OnReadbackDone(mipIndex, r);
				});
			}
		}

		public void RaiseDoneEvent(Texture requestTextureIn, Texture2D requestTextureOut, string requestErrorText)
		{
			this.OnDone?.Invoke(this);
		}

		public void Clear()
		{
			TextureIn = null;
			TextureOut = null;
			this.OnDone = null;
		}

		public unsafe void OnReadbackDone(int mipIndex, AsyncGPUReadbackRequest request)
		{
			if (request.hasError)
			{
				Error($"AsyncGPUReadback failed on texture {TextureIn}.");
				return;
			}
			if (!TextureIn || !TextureOut)
			{
				Error("Request is obsolete. Texture is destroyed already.");
				return;
			}
			NativeArray<byte> value = new NativeArray<byte>(request.GetData<byte>(), Allocator.Persistent);
			SourceBytes.Add(mipIndex, value);
			if (SourceBytes.Count < TextureIn.mipmapCount)
			{
				return;
			}
			NativeArray<byte> outData = TextureOut.GetRawTextureData<byte>();
			int texWidth = TextureOut.width;
			int texHeight = TextureOut.height;
			int mipOffset = Math.Min(m_SkipMips, TextureIn.mipmapCount - 1);
			new Task(delegate
			{
				foreach (int key in SourceBytes.Keys)
				{
					NativeArray<byte> nativeArray = SourceBytes[key];
					int num = texWidth;
					int num2 = texHeight;
					int num3 = 0;
					int num4 = key + mipOffset;
					for (int i = 0; i < num4; i++)
					{
						num3 += num * num2 / ((Compression != 0) ? 1 : 2);
						num /= 2;
						num2 /= 2;
					}
					if (num >= 4 && num2 >= 4)
					{
						int length = num * num2 / ((Compression != 0) ? 1 : 2);
						byte* unsafePtr = (byte*)nativeArray.GetUnsafePtr();
						byte* unsafePtr2 = (byte*)outData.GetSubArray(num3, length).GetUnsafePtr();
						CompressPtr(num, num2, unsafePtr, unsafePtr2, Compression == Compression.Dxt5);
					}
					nativeArray.Dispose();
				}
				m_IsComplete = true;
			}).Start();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		[MonoPInvokeCallback(typeof(CompressDelegate))]
		public unsafe static void Compress_0024BurstManaged(int width, int height, byte* inData, byte* outData, bool hasAlpha)
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

	private readonly List<Request> m_Requests = new List<Request>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public int RequestsCount => m_Requests.Count;

	public void Update()
	{
		for (int i = 0; i < m_Requests.Count; i++)
		{
			Request request = m_Requests[i];
			if (request.IsComplete)
			{
				request.RaiseDoneEvent(request.TextureIn, request.TextureOut, request.HasError ? request.ErrorText : null);
				m_Requests.Remove(request);
				request.Clear();
				i--;
			}
		}
	}

	public Request CompressTexture(Texture textureIn, Texture2D textureOut, Compression compression, int skipMips)
	{
		Request request = new Request(textureIn, textureOut, compression, skipMips);
		m_Requests.Add(request);
		request.Run();
		return request;
	}
}
