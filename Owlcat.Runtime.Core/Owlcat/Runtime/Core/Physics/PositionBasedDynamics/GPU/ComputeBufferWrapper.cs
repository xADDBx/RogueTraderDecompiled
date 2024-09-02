using Owlcat.Runtime.Core.Utility;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public abstract class ComputeBufferWrapper
{
	protected ComputeBuffer m_Buffer;

	private int m_NameId;

	private int m_SizeNameId;

	private int m_Size;

	public readonly string Name;

	public int NameId => m_NameId;

	public int Size => m_Size;

	public int SizeNameId => m_SizeNameId;

	public int Count => m_Buffer?.count ?? 0;

	public ComputeBufferWrapper(string name, int size = 64)
	{
		Name = name;
		m_NameId = Shader.PropertyToID(name);
		m_SizeNameId = Shader.PropertyToID(Name + "_Size");
		Resize(size);
	}

	public void Resize(int newSize)
	{
		m_Size = newSize;
		ResizeBuffer(newSize);
	}

	protected abstract void ResizeBuffer(int newSize);

	public static implicit operator ComputeBuffer(ComputeBufferWrapper wrapper)
	{
		return wrapper.m_Buffer;
	}

	internal bool IsValid()
	{
		if (m_Buffer != null)
		{
			return m_Buffer.IsValid();
		}
		return false;
	}

	internal void Dispose()
	{
		if (m_Buffer != null)
		{
			m_Buffer.Dispose();
			m_Buffer = null;
		}
	}

	public void SetData<T>(NativeArray<T> data) where T : struct
	{
		m_Buffer.SetData(data);
	}

	public void SetData<T>(NativeArray<T> data, int nativeBufferStartIndex, int computeBufferStartIndex, int count) where T : struct
	{
		m_Buffer.SetData(data, nativeBufferStartIndex, computeBufferStartIndex, count);
	}
}
public class ComputeBufferWrapper<T> : ComputeBufferWrapper
{
	public ComputeBufferWrapper(string name, int size = 64)
		: base(name, size)
	{
	}

	protected override void ResizeBuffer(int newSize)
	{
		m_Buffer = ComputeBufferUtils.SetSize(m_Buffer, typeof(T), newSize, Name);
	}

	public static implicit operator ComputeBuffer(ComputeBufferWrapper<T> wrapper)
	{
		return wrapper.m_Buffer;
	}
}
