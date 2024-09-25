namespace Kingmaker.Pathfinding;

public readonly struct WarhammerPathAiMetric
{
	public readonly int DiagonalsCount;

	public readonly float Length;

	public readonly float Delay;

	public readonly int EnteredAoE;

	public readonly int StepsInsideDamagingAoE;

	public readonly int ProvokedAttacks;

	public WarhammerPathAiMetric(int diagonalsCount, float length, float delay, int enteredAoE, int stepsInsideDamagingAoE, int provokedAttacks)
	{
		DiagonalsCount = diagonalsCount;
		Length = length;
		Delay = delay;
		EnteredAoE = enteredAoE;
		StepsInsideDamagingAoE = stepsInsideDamagingAoE;
		ProvokedAttacks = provokedAttacks;
	}
}
