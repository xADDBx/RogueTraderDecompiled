using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Controllers.Units;

public static class UnitDifficultyMomentumHelper
{
	public static readonly float[] ResolvesGained;

	public static readonly int[] ResolvesGainedFlat;

	public static readonly float[] ResolvesLost;

	public static float ResolvesGainedForPartyMemberKill => ResolvesGained[3];

	public static int ResolvesGainedFlatForPartyMemberKill => ResolvesGainedFlat[3];

	static UnitDifficultyMomentumHelper()
	{
		ResolvesGained = new float[8];
		ResolvesGainedFlat = new int[8];
		ResolvesLost = new float[8];
		ResolvesGained[0] = 0.5f;
		ResolvesGained[1] = 0.5f;
		ResolvesGained[2] = 1f;
		ResolvesGained[3] = 1f;
		ResolvesGained[4] = 2f;
		ResolvesGained[5] = 2f;
		ResolvesGained[6] = 2f;
		ResolvesGainedFlat[0] = 3;
		ResolvesGainedFlat[1] = 6;
		ResolvesGainedFlat[2] = 7;
		ResolvesGainedFlat[3] = 12;
		ResolvesGainedFlat[4] = 20;
		ResolvesGainedFlat[5] = 25;
		ResolvesGainedFlat[6] = 30;
		ResolvesLost[0] = 5f;
		ResolvesLost[1] = 6f;
		ResolvesLost[2] = 8f;
		ResolvesLost[3] = 10f;
		ResolvesLost[4] = 12f;
		ResolvesLost[5] = 15f;
		ResolvesLost[6] = 20f;
	}

	public static float GetResolveGained(UnitDifficultyType type)
	{
		return ResolvesGained[(int)type];
	}

	public static int GetResolveGainedFlat(UnitDifficultyType type)
	{
		return ResolvesGainedFlat[(int)type];
	}

	public static float GetResolveGained([CanBeNull] MechanicEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return 0f;
		}
		return GetResolveGained(baseUnitEntity.Blueprint.DifficultyType);
	}

	public static int GetResolveGainedFlat([CanBeNull] MechanicEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		return GetResolveGainedFlat(baseUnitEntity.Blueprint.DifficultyType);
	}

	public static float GetResolveLost(UnitDifficultyType type)
	{
		return ResolvesLost[(int)type];
	}

	public static float GetResolveLost([CanBeNull] MechanicEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return 0f;
		}
		return GetResolveLost(baseUnitEntity.Blueprint.DifficultyType);
	}
}
