using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUSingleBufferSoA<T> : GPUSoABase
{
	private string m_ShaderName;

	private int m_ShaderNameId;

	public ComputeBufferWrapper<T> Buffer;

	private string m_Name;

	public override string Name => m_Name;

	public GPUSingleBufferSoA(string shaderName)
		: this(shaderName, 64)
	{
	}

	public GPUSingleBufferSoA(string shaderName, int size)
	{
		m_ShaderName = shaderName;
		m_ShaderNameId = Shader.PropertyToID(shaderName);
		if (m_ShaderName.StartsWith("_"))
		{
			m_Name = m_ShaderName.Remove(0, 1);
		}
		else
		{
			m_Name = m_ShaderName;
		}
		InitNameId();
		m_Buffers = InitBuffers(size);
	}

	public GPUSingleBufferSoA(int size)
		: base(size)
	{
	}

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		if (m_Name == null)
		{
			return null;
		}
		return new ComputeBufferWrapper[1] { Buffer = new ComputeBufferWrapper<T>(m_ShaderName, size) };
	}
}
