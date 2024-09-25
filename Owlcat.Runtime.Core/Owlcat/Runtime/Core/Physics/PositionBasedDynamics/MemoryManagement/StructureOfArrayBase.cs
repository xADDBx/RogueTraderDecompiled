namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;

public abstract class StructureOfArrayBase
{
	protected MemoryAllocator m_Allocator;

	public int Length => m_Allocator.Size;

	public StructureOfArrayBase(int size)
	{
		m_Allocator = new MemoryAllocator(size);
		Resize(size);
	}

	public abstract void Dispose();

	public bool TryAlloc(int size, out int offset)
	{
		return m_Allocator.TryAlloc(size, out offset);
	}

	public void Free(int offset, int size)
	{
		m_Allocator.Free(offset, size);
	}

	public virtual void Resize(int newSize)
	{
		Dispose();
		m_Allocator.Resize(newSize);
	}

	public void Reset()
	{
		m_Allocator.Reset();
	}

	public int GetSizeInBytes()
	{
		if (m_Allocator.Stride > -1)
		{
			return m_Allocator.Size * m_Allocator.Stride;
		}
		return 0;
	}
}
