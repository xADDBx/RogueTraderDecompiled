namespace Kingmaker.Pathfinding;

public readonly struct WarhammerPathPlayerMetric
{
	public readonly int DiagonalsCount;

	public readonly float Length;

	public WarhammerPathPlayerMetric(int diagonalsCount, float length)
	{
		DiagonalsCount = diagonalsCount;
		Length = length;
	}

	public override string ToString()
	{
		return $"L:{Length}";
	}
}
