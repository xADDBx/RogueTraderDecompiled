using Unity.Collections;

namespace Owlcat.Runtime.Core.Utility;

public static class NativeArrayUtils
{
	public static void IncreaseSize<T>(this ref NativeArray<T> array, int size, NativeArrayOptions options) where T : struct
	{
		if (!array.IsCreated || array.Length < size)
		{
			if (array.IsCreated)
			{
				array.Dispose();
			}
			array = new NativeArray<T>(size, Allocator.Persistent, options);
		}
	}
}
