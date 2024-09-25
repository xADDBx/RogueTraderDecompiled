using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;

[BurstCompile]
public struct ZBinningJob : IJobFor
{
	public const int BatchCount = 64;

	public float ZBinFactor;

	public float CameraNearClip;

	public int VisibleCount;

	[NativeDisableParallelForRestriction]
	public NativeArray<ZBin> ZBins;

	[ReadOnly]
	public NativeArray<LocalFogDescriptor> FogDescs;

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
		for (int j = 0; j < VisibleCount; j++)
		{
			ushort num3 = (ushort)j;
			LocalFogDescriptor localFogDescriptor = FogDescs[j];
			int num4 = math.max((int)((localFogDescriptor.MinZ - CameraNearClip) * ZBinFactor), num);
			int num5 = math.min((int)((localFogDescriptor.MaxZ - CameraNearClip) * ZBinFactor), num2);
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
