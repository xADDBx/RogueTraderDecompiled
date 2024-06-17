using System.Linq;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public struct PatternCellDataAccumulator
{
	private float ProbabilitiesSum { get; set; }

	private float[] InitialProbabilities { get; }

	private float DodgeProbability { get; set; }

	private float CoverProbability { get; set; }

	private float EvasionProbability { get; set; }

	private int Lines { get; set; }

	private bool MainCell { get; set; }

	private float InitialAverageProbability { get; set; }

	private bool AlwaysHit { get; }

	public PatternCellData Result => new PatternCellData(ProbabilitiesSum, InitialProbabilities, DodgeProbability, CoverProbability, EvasionProbability, Lines, MainCell, InitialAverageProbability, AlwaysHit);

	public PatternCellDataAccumulator(float[] initialHitProbabilities, float dodgeProbability, float coverProbability, float evasionProbability, bool mainCell)
	{
		AlwaysHit = false;
		InitialProbabilities = initialHitProbabilities;
		float num2 = (InitialAverageProbability = initialHitProbabilities.Average());
		ProbabilitiesSum = num2 * (1f - dodgeProbability) * (1f - coverProbability);
		DodgeProbability = dodgeProbability;
		CoverProbability = coverProbability;
		EvasionProbability = evasionProbability;
		MainCell = mainCell;
		Lines = 1;
	}

	public void AddShotProbability(float[] initialHitProbability, float dodgeProbability, float coverProbability, float evasionProbability, bool mainCell)
	{
		Lines++;
		for (int i = 0; i < InitialProbabilities.Length; i++)
		{
			InitialProbabilities[i] += initialHitProbability[i];
		}
		float num = initialHitProbability.Average();
		InitialAverageProbability += num;
		ProbabilitiesSum += num * (1f - dodgeProbability) * (1f - coverProbability);
		DodgeProbability = dodgeProbability;
		CoverProbability = coverProbability;
		EvasionProbability = evasionProbability;
		MainCell |= mainCell;
	}
}
