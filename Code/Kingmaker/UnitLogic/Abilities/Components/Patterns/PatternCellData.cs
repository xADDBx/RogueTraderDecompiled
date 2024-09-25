namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public readonly struct PatternCellData
{
	public static readonly PatternCellData Empty = new PatternCellData(1f, new float[1] { 1f }, 0f, 0f, 0f, 1, mainCell: false, 1f, alwaysHit: true);

	public float ProbabilitiesSum { get; }

	public float[] InitialProbabilities { get; }

	public float DodgeProbability { get; }

	public float CoverProbability { get; }

	public float EvasionProbability { get; }

	public int Lines { get; }

	public bool MainCell { get; }

	public float InitialAverageProbability { get; }

	public bool AlwaysHit { get; }

	public PatternCellData(float probabilitiesSum, float[] initialProbabilities, float dodgeProbability, float coverProbability, float evasionProbability, int lines, bool mainCell, float initialAverageProbability, bool alwaysHit)
	{
		this = default(PatternCellData);
		ProbabilitiesSum = probabilitiesSum;
		InitialProbabilities = initialProbabilities;
		DodgeProbability = dodgeProbability;
		CoverProbability = coverProbability;
		EvasionProbability = evasionProbability;
		Lines = lines;
		MainCell = mainCell;
		InitialAverageProbability = initialAverageProbability;
		AlwaysHit = alwaysHit;
	}
}
