using System.Collections.Generic;
using System.Linq;
using Kingmaker.GPUCrowd;

namespace Kingmaker.Controllers;

public class GpuCrowdController
{
	public HashSet<GpuCrowd> GpuCrowds = new HashSet<GpuCrowd>();

	public void RegisterCrowd(GpuCrowd gpuCrowd)
	{
		GpuCrowds.Add(gpuCrowd);
	}

	public void UnregisterCrowd(GpuCrowd gpuCrowd)
	{
		if (GpuCrowds.Contains(gpuCrowd))
		{
			GpuCrowds.Remove(gpuCrowd);
		}
	}

	public void ClearAllCrowds()
	{
		GpuCrowds.Clear();
	}

	public void ClearNullCrowds()
	{
		GpuCrowds = GpuCrowds.Where((GpuCrowd crowd) => crowd != null).ToHashSet();
	}

	public int CountAllCrowdsUnits(bool withShadows)
	{
		int num = 0;
		foreach (GpuCrowd gpuCrowd in GpuCrowds)
		{
			if (gpuCrowd == null || gpuCrowd.CrowdVfx == null)
			{
				continue;
			}
			if (gpuCrowd.CrowdVfx.HasBool("Shadows Enable"))
			{
				if (gpuCrowd.CrowdVfx.GetBool("Shadows Enable") != withShadows)
				{
					continue;
				}
			}
			else if (withShadows)
			{
				continue;
			}
			num += CountCrowdUnitsVisible(gpuCrowd);
		}
		return num;
	}

	public static int CountCrowdUnitsVisible(GpuCrowd gpuCrowd)
	{
		if (gpuCrowd == null || gpuCrowd.CrowdVfx == null)
		{
			return 0;
		}
		if (gpuCrowd.CrowdVfx.culled)
		{
			return 0;
		}
		return gpuCrowd.CrowdVfx.aliveParticleCount;
	}
}
