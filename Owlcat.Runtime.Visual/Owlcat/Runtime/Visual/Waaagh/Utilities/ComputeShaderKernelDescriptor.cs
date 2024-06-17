using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Utilities;

public struct ComputeShaderKernelDescriptor
{
	public int Index;

	public uint3 ThreadGroupSize;

	public int GetThreadGroupSizeX(int sizeX)
	{
		return RenderingUtils.DivRoundUp(sizeX, (int)ThreadGroupSize.x);
	}

	public int GetThreadGroupSizeY(int sizeY)
	{
		return RenderingUtils.DivRoundUp(sizeY, (int)ThreadGroupSize.y);
	}

	public int GetThreadGroupSizeZ(int sizeZ)
	{
		return RenderingUtils.DivRoundUp(sizeZ, (int)ThreadGroupSize.y);
	}

	public int3 GetDispatchSize(int x, int y, int z)
	{
		return new int3(GetThreadGroupSizeX(x), GetThreadGroupSizeY(y), GetThreadGroupSizeZ(z));
	}

	public int3 GetDispatchSize(float x, float y, float z)
	{
		return new int3(GetThreadGroupSizeX((int)x), GetThreadGroupSizeY((int)y), GetThreadGroupSizeZ((int)z));
	}
}
