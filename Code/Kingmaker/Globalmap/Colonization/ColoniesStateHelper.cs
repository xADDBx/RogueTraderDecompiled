using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public static class ColoniesStateHelper
{
	public static int GetResourceFromMinerCountWithProductivity(int count, int productivity)
	{
		return Mathf.CeilToInt((float)(count * productivity) / 100f);
	}

	public static int GetResourceFromMinerCountWithProductivity(ColoniesState.MinerData minerData, int productivity)
	{
		return GetResourceFromMinerCountWithProductivity(minerData.InitialCount, productivity);
	}

	public static int GetResourceFromMinerCountWithProductivity(int count)
	{
		int value = Game.Instance.Player.ColoniesState.MinerProductivity.Value;
		return GetResourceFromMinerCountWithProductivity(count, value);
	}

	public static int GetResourceFromMinerCountWithProductivity(ColoniesState.MinerData minerData)
	{
		int value = Game.Instance.Player.ColoniesState.MinerProductivity.Value;
		return GetResourceFromMinerCountWithProductivity(minerData, value);
	}

	private static float GetEfficiencyModifier(int efficiency)
	{
		return (float)efficiency * 0.1f + 1f;
	}

	public static int GetProducedResourceCountWithEfficiencyModifier(int count, int efficiency)
	{
		float efficiencyModifier = GetEfficiencyModifier(efficiency);
		return Mathf.CeilToInt((float)count * efficiencyModifier);
	}
}
