using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

[BurstCompile]
public struct ZBinningJob : IJobFor
{
	public const int BatchCount = 64;

	public float ZBinFactor;

	public float CameraNearClip;

	public int LightCount;

	public int DirectionalLightCount;

	[NativeDisableParallelForRestriction]
	public NativeArray<ZBin> ZBins;

	[ReadOnly]
	public NativeArray<LightDescriptor> Lights;

	public void Execute(int index)
	{
		int num = 64 * index;
		int num2 = num + 64 - 1;
		for (int i = num; i <= num2; i++)
		{
			ZBins[i] = new ZBin
			{
				MinIndex = ushort.MaxValue,
				MaxIndex = ushort.MaxValue
			};
		}
		for (int j = DirectionalLightCount; j < LightCount; j++)
		{
			ushort num3 = (ushort)j;
			LightDescriptor lightDescriptor = Lights[j];
			int num4 = math.max((int)((lightDescriptor.MinZ - CameraNearClip) * ZBinFactor), num);
			int num5 = math.min((int)((lightDescriptor.MaxZ - CameraNearClip) * ZBinFactor), num2);
			for (int k = num4; k <= num5; k++)
			{
				ZBin value = ZBins[k];
				value.MinIndex = Math.Min(value.MinIndex, num3);
				value.MaxIndex = num3;
				ZBins[k] = value;
			}
		}
	}
}
