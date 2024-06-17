namespace Kingmaker.Pathfinding;

public readonly struct WarhammerPathChargeMetric
{
	public readonly float Length;

	public readonly int DiagonalsCount;

	public WarhammerPathChargeMetric(float length, int diagonalsCount)
	{
		Length = length;
		DiagonalsCount = diagonalsCount;
	}

	public override string ToString()
	{
		return $"L:{Length}";
	}
}
