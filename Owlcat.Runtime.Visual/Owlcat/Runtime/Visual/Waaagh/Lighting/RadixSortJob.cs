using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

[BurstCompile]
public struct RadixSortJob : IJob
{
	public NativeArray<int> Keys;

	public NativeArray<int> Indices;

	public void Execute()
	{
		NativeArray<int> nativeArray = new NativeArray<int>(Keys.Length, Allocator.Temp);
		NativeArray<int> nativeArray2 = new NativeArray<int>(Keys.Length, Allocator.Temp);
		for (int i = 0; i < nativeArray.Length; i++)
		{
			Indices[i] = i;
		}
		int num = 4;
		NativeArray<int> nativeArray3 = new NativeArray<int>(1 << num, Allocator.Temp);
		NativeArray<int> nativeArray4 = new NativeArray<int>(1 << num, Allocator.Temp);
		int num2 = 32 / num;
		int num3 = (1 << num) - 1;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		while (num6 < num2)
		{
			for (int j = 0; j < nativeArray3.Length; j++)
			{
				nativeArray3[j] = 0;
			}
			for (int k = 0; k < Keys.Length; k++)
			{
				nativeArray3[(Keys[k] >> num7) & num3]++;
				if (num6 == 0 && Keys[k] < 0)
				{
					num4++;
				}
			}
			if (num6 == 0)
			{
				num5 = Keys.Length - num4;
			}
			nativeArray4[0] = 0;
			for (int l = 1; l < nativeArray3.Length; l++)
			{
				nativeArray4[l] = nativeArray4[l - 1] + nativeArray3[l - 1];
			}
			for (int m = 0; m < Keys.Length; m++)
			{
				int num8 = nativeArray4[(Keys[m] >> num7) & num3]++;
				if (num6 == num2 - 1)
				{
					num8 = ((Keys[m] >= 0) ? (num8 + num4) : (num5 - (num8 - num4) - 1));
				}
				nativeArray[num8] = Keys[m];
				nativeArray2[num8] = Indices[m];
			}
			nativeArray.CopyTo(Keys);
			nativeArray2.CopyTo(Indices);
			num6++;
			num7 += num;
		}
		nativeArray.Dispose();
		nativeArray2.Dispose();
		nativeArray3.Dispose();
		nativeArray4.Dispose();
	}
}
