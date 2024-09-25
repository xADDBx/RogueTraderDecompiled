using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public readonly struct CornerDirection
{
	public const int kIndexNE = 0;

	public const int kIndexNW = 1;

	public const int kIndexSW = 2;

	public const int kIndexSE = 3;

	public const int kIndicesCount = 4;

	private const int kTurnCounterClockwise90IndexOffset = 1;

	private const int kTurnClockwise90IndexOffset = 3;

	private const int kTurn180IndexOffset = 2;

	public readonly int index;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CornerDirection(int index)
	{
		this.index = index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CornerDirection TurnCounterClockwise90()
	{
		return new CornerDirection((index + 1) % 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CornerDirection TurnClockwise90()
	{
		return new CornerDirection((index + 3) % 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CornerDirection Turn180()
	{
		return new CornerDirection((index + 2) % 4);
	}
}
