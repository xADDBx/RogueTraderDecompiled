using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.AI.AreaScanning;

public class CommonTravelDelayProvider : ITravelDelayProvider
{
	private bool AbilityIgnoresAoO;

	private readonly Dictionary<CustomGridNodeBase, AiBrainHelper.ThreatsInfo> threatsCache = new Dictionary<CustomGridNodeBase, AiBrainHelper.ThreatsInfo>();

	public CommonTravelDelayProvider(bool abilityIgnoresAoO)
	{
		AbilityIgnoresAoO = abilityIgnoresAoO;
	}

	public PassInfo GetPassInfo(BaseUnitEntity unit, CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, bool isEvenDiagonal, bool isDiagonalDirection)
	{
		AiBrainHelper.ThreatsInfo prevTd = FindThreats(unit, nodeFrom);
		AiBrainHelper.ThreatsInfo threatsInfo = FindThreats(unit, nodeTo);
		float num = 0f;
		int num2 = threatsInfo.aes.Count - prevTd.aes.Count;
		num += (float)(Mathf.Clamp(num2, -3, 3) + 3) * 100f;
		int num3 = threatsInfo.aooUnits.Count((BaseUnitEntity un) => prevTd.aooUnits.Contains(un));
		int num4 = prevTd.aooUnits.Count - num3;
		num += (float)(1000 * num4);
		int num5 = threatsInfo.aooUnits.Count - num3;
		num += (float)((AbilityIgnoresAoO ? 10 : 1000) * num5);
		float num6 = ((num3 > 0) ? unit.GetWarhammerMovementApPerCellThreateningArea() : unit.Blueprint.WarhammerMovementApPerCell) * (float)((!isEvenDiagonal) ? 1 : 2);
		PassInfo result = default(PassInfo);
		result.pathCost = num6;
		result.delay = num6 + (isDiagonalDirection ? 0.1f : 0f) + num;
		result.enteredAoE = Mathf.Max(0, num2);
		result.provokedAoO = num4;
		return result;
	}

	protected AiBrainHelper.ThreatsInfo FindThreats(BaseUnitEntity unit, CustomGridNodeBase node)
	{
		if (threatsCache.TryGetValue(node, out var value))
		{
			return value;
		}
		value = AiBrainHelper.TryFindThreats(unit, node);
		threatsCache[node] = value;
		return value;
	}
}
