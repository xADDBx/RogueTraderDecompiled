namespace Kingmaker.Pathfinding;

public static class CellsExtension
{
	public static Cells Cells(this int self)
	{
		return new Cells(self);
	}

	public static Cells Cells(this float self)
	{
		return new Cells(self);
	}

	public static Cells CellsFromMeters(this int self)
	{
		return new Cells((float)self / Kingmaker.Pathfinding.Cells.ToMetersRatio);
	}

	public static Cells CellsFromMeters(this float self)
	{
		return new Cells(self / Kingmaker.Pathfinding.Cells.ToMetersRatio);
	}
}
