using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUHashtable;

public class LinearProbingHashtable
{
	private static int _HashtableCapacity = Shader.PropertyToID("_HashtableCapacity");

	private static int _HashtableBuffer = Shader.PropertyToID("_HashtableBuffer");

	private static int _InKeysBuffer = Shader.PropertyToID("_InKeysBuffer");

	private static int _InValuesBuffer = Shader.PropertyToID("_InValuesBuffer");

	private int m_Capacity;

	private ComputeShader m_Shader;

	private int m_KernelClear;

	private int m_KernelInsert;

	private uint3 m_KernelClearThreadGroupSize;

	private uint3 m_KernelInsertThreadGroupSize;

	private ComputeBuffer m_HashtableBuffer;

	public int Capacity => m_Capacity;

	public ComputeBuffer HashtableBuffer => m_HashtableBuffer;

	public LinearProbingHashtable(int capacity, ComputeShader shader)
	{
		m_Shader = shader;
		m_KernelClear = m_Shader.FindKernel("Clear");
		m_KernelInsert = m_Shader.FindKernel("Insert");
		m_Shader.GetKernelThreadGroupSizes(m_KernelClear, out m_KernelClearThreadGroupSize.x, out m_KernelClearThreadGroupSize.y, out m_KernelClearThreadGroupSize.z);
		m_Shader.GetKernelThreadGroupSizes(m_KernelInsert, out m_KernelInsertThreadGroupSize.x, out m_KernelInsertThreadGroupSize.y, out m_KernelInsertThreadGroupSize.z);
		Resize(capacity);
	}

	public void Clear(CommandBuffer cmd)
	{
		cmd.SetComputeIntParam(m_Shader, _HashtableCapacity, m_Capacity);
		cmd.SetComputeBufferParam(m_Shader, m_KernelClear, _HashtableBuffer, m_HashtableBuffer);
		cmd.DispatchCompute(m_Shader, m_KernelClear, RenderingUtils.DivRoundUp(m_Capacity, (int)m_KernelClearThreadGroupSize.x), 1, 1);
	}

	public void Insert(CommandBuffer cmd, ComputeBuffer keys, ComputeBuffer values, int count)
	{
		cmd.SetComputeBufferParam(m_Shader, m_KernelInsert, _InKeysBuffer, keys);
		cmd.SetComputeBufferParam(m_Shader, m_KernelInsert, _InValuesBuffer, values);
		cmd.SetComputeBufferParam(m_Shader, m_KernelInsert, _HashtableBuffer, m_HashtableBuffer);
		cmd.DispatchCompute(m_Shader, m_KernelInsert, RenderingUtils.DivRoundUp(count, (int)m_KernelInsertThreadGroupSize.x), 1, 1);
	}

	public void Dispose()
	{
		m_HashtableBuffer.Dispose();
	}

	public void Resize(int capacity)
	{
		m_Capacity = Mathf.NextPowerOfTwo(capacity);
		if (m_HashtableBuffer != null)
		{
			m_HashtableBuffer.Dispose();
		}
		m_HashtableBuffer = new ComputeBuffer(m_Capacity, Marshal.SizeOf(typeof(uint2)), ComputeBufferType.Structured);
	}
}
