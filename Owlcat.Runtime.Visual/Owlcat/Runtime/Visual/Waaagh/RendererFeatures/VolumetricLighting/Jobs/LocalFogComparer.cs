using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[BurstCompile]
public struct LocalFogComparer : IComparer<LocalFogDescriptor>
{
	public int Compare(LocalFogDescriptor x, LocalFogDescriptor y)
	{
		if (x.MeanZ < y.MeanZ)
		{
			return -1;
		}
		if (x.MeanZ > y.MeanZ)
		{
			return 1;
		}
		return 0;
	}
}
