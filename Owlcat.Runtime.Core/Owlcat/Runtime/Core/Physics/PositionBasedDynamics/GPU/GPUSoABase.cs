using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public abstract class GPUSoABase
{
	private int m_NameSizeId;

	private int m_Size;

	protected ComputeBufferWrapper[] m_Buffers;

	public int Length => m_Size;

	public abstract string Name { get; }

	public GPUSoABase(int size = 64)
	{
		InitNameId();
		m_Size = size;
		m_Buffers = InitBuffers(size);
	}

	protected virtual void InitNameId()
	{
		m_NameSizeId = Shader.PropertyToID("_" + Name + "Size");
	}

	protected abstract ComputeBufferWrapper[] InitBuffers(int size);

	public void SetGlobalData(CommandBuffer cmd)
	{
		ComputeBufferWrapper[] buffers = m_Buffers;
		foreach (ComputeBufferWrapper computeBufferWrapper in buffers)
		{
			cmd.SetGlobalBuffer(computeBufferWrapper.NameId, computeBufferWrapper);
			cmd.SetGlobalInt(computeBufferWrapper.SizeNameId, computeBufferWrapper.Count);
		}
		cmd.SetGlobalInt(m_NameSizeId, m_Size);
	}

	public void SetComputeData(CommandBuffer cmd, ComputeShader shader, int kernel)
	{
		ComputeBufferWrapper[] buffers = m_Buffers;
		foreach (ComputeBufferWrapper computeBufferWrapper in buffers)
		{
			cmd.SetComputeBufferParam(shader, kernel, computeBufferWrapper.NameId, computeBufferWrapper);
			cmd.SetComputeIntParam(shader, computeBufferWrapper.SizeNameId, computeBufferWrapper.Count);
		}
		cmd.SetComputeIntParam(shader, m_NameSizeId, m_Size);
	}

	public bool IsValid()
	{
		bool flag = true;
		ComputeBufferWrapper[] buffers = m_Buffers;
		foreach (ComputeBufferWrapper computeBufferWrapper in buffers)
		{
			flag &= computeBufferWrapper.IsValid();
		}
		return flag;
	}

	public void Dispose()
	{
		ComputeBufferWrapper[] buffers = m_Buffers;
		for (int i = 0; i < buffers.Length; i++)
		{
			buffers[i].Dispose();
		}
	}

	public void Resize(int newSize)
	{
		m_Size = newSize;
		ComputeBufferWrapper[] buffers = m_Buffers;
		for (int i = 0; i < buffers.Length; i++)
		{
			buffers[i].Resize(newSize);
		}
	}
}
