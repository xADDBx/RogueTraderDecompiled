using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUParallelSort;

public sealed class RadixSorter
{
	private const int kGpuSortBitCount = 32;

	private const int kRadixBits = 4;

	private const int kDigitCount = 16;

	private const int kKeysPerLoop = 8;

	private const int kThreadCount = 128;

	private const int kTileSize = 1024;

	private const int kMaxGroupCount = 64;

	private const int kMaxPassCount = 8;

	private static int _RadixShift = Shader.PropertyToID("_RadixShift");

	private static int _TilesPerGroup = Shader.PropertyToID("_TilesPerGroup");

	private static int _ExtraTileCount = Shader.PropertyToID("_ExtraTileCount");

	private static int _ExtraKeyCount = Shader.PropertyToID("_ExtraKeyCount");

	private static int _GroupCount = Shader.PropertyToID("_GroupCount");

	private static int _InOffsets = Shader.PropertyToID("InOffsets");

	private static int _OutOffsets = Shader.PropertyToID("OutOffsets");

	private static int _InKeys = Shader.PropertyToID("InKeys");

	private static int _OutKeys = Shader.PropertyToID("OutKeys");

	private static int _InValues = Shader.PropertyToID("InValues");

	private static int _OutValues = Shader.PropertyToID("OutValues");

	private ComputeShader m_Shader;

	private int m_KernelClearOffsets;

	private int m_KernelUpsweep;

	private int m_KernelSpine;

	private int m_KernelDownsweep;

	private ComputeBuffer[] m_OffsetsBuffers = new ComputeBuffer[2];

	public RadixSorter(ComputeShader shader)
	{
		m_Shader = shader;
		m_KernelClearOffsets = m_Shader.FindKernel("ClearOffsets");
		m_KernelUpsweep = m_Shader.FindKernel("Upsweep");
		m_KernelSpine = m_Shader.FindKernel("Spine");
		m_KernelDownsweep = m_Shader.FindKernel("Downsweep");
	}

	private void InitOffsetBuffers()
	{
		if (m_OffsetsBuffers[0] == null)
		{
			int count = 1024;
			for (int i = 0; i < m_OffsetsBuffers.Length; i++)
			{
				m_OffsetsBuffers[i] = new ComputeBuffer(count, 4, ComputeBufferType.Structured);
				m_OffsetsBuffers[i].name = $"RadixSort.OffsetsBuffer_{i}";
			}
		}
	}

	public void Dispose()
	{
		if (m_OffsetsBuffers[0] != null)
		{
			for (int i = 0; i < m_OffsetsBuffers.Length; i++)
			{
				m_OffsetsBuffers[i].Release();
			}
		}
	}

	public int GetGPUSortPassCount(uint keyMask)
	{
		int num = 32 / 4;
		int num2 = 0;
		uint num3 = 15u;
		for (int i = 0; i < num; i++)
		{
			if ((num3 & keyMask) != 0)
			{
				num2++;
			}
			num3 <<= 4;
		}
		return num2;
	}

	public int Sort(CommandBuffer cmd, uint keyMask, int count, RadixSortBuffers sortBuffers, int bufferIndex)
	{
		InitOffsetBuffers();
		int num;
		int num2 = (num = count / 1024);
		if (num > 64)
		{
			num = 64;
		}
		else if (num == 0)
		{
			num = 1;
		}
		int val = num2 / num;
		int val2 = num2 % num;
		int val3 = count % 1024;
		int num3 = 32 / 4;
		cmd.SetComputeIntParam(m_Shader, _TilesPerGroup, val);
		cmd.SetComputeIntParam(m_Shader, _ExtraTileCount, val2);
		cmd.SetComputeIntParam(m_Shader, _ExtraKeyCount, val3);
		cmd.SetComputeIntParam(m_Shader, _GroupCount, num);
		uint num4 = 15u;
		uint num5 = 0u;
		for (int i = 0; i < num3; i++)
		{
			if ((num4 & keyMask) != 0)
			{
				cmd.SetComputeIntParam(m_Shader, _RadixShift, (int)num5);
				cmd.SetComputeBufferParam(m_Shader, m_KernelClearOffsets, _OutOffsets, m_OffsetsBuffers[0]);
				cmd.DispatchCompute(m_Shader, m_KernelClearOffsets, 1, 1, 1);
				cmd.SetComputeBufferParam(m_Shader, m_KernelUpsweep, _InKeys, sortBuffers.KeysBuffers[bufferIndex]);
				cmd.SetComputeBufferParam(m_Shader, m_KernelUpsweep, _OutOffsets, m_OffsetsBuffers[0]);
				cmd.DispatchCompute(m_Shader, m_KernelUpsweep, num, 1, 1);
				cmd.SetComputeBufferParam(m_Shader, m_KernelSpine, _InOffsets, m_OffsetsBuffers[0]);
				cmd.SetComputeBufferParam(m_Shader, m_KernelSpine, _OutOffsets, m_OffsetsBuffers[1]);
				cmd.DispatchCompute(m_Shader, m_KernelSpine, 1, 1, 1);
				cmd.SetComputeBufferParam(m_Shader, m_KernelDownsweep, _InKeys, sortBuffers.KeysBuffers[bufferIndex]);
				cmd.SetComputeBufferParam(m_Shader, m_KernelDownsweep, _InValues, sortBuffers.ValuesBuffers[bufferIndex]);
				cmd.SetComputeBufferParam(m_Shader, m_KernelDownsweep, _InOffsets, m_OffsetsBuffers[1]);
				cmd.SetComputeBufferParam(m_Shader, m_KernelDownsweep, _OutKeys, sortBuffers.KeysBuffers[bufferIndex ^ 1]);
				cmd.SetComputeBufferParam(m_Shader, m_KernelDownsweep, _OutValues, sortBuffers.ValuesBuffers[bufferIndex ^ 1]);
				cmd.DispatchCompute(m_Shader, m_KernelDownsweep, num, 1, 1);
				bufferIndex ^= 1;
			}
			num5 += 4;
			num4 <<= 4;
		}
		return bufferIndex;
	}
}
