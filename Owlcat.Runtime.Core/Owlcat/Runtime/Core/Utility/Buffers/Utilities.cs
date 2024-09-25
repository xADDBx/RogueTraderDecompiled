using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Core.Utility.Buffers;

internal static class Utilities
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int SelectBucketIndex(int bufferSize)
	{
		int x = bufferSize - 1 >> 4;
		return 32 - LeadingZeroCount(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetMaxSizeForBucket(int binIndex)
	{
		return 16 << binIndex;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LeadingZeroCount(int x)
	{
		x |= x >> 1;
		x |= x >> 2;
		x |= x >> 4;
		x |= x >> 8;
		x |= x >> 16;
		x -= (x >> 1) & 0x55555555;
		x = ((x >> 2) & 0x33333333) + (x & 0x33333333);
		x = ((x >> 4) + x) & 0xF0F0F0F;
		x += x >> 8;
		x += x >> 16;
		return 32 - (x & 0x3F);
	}
}
