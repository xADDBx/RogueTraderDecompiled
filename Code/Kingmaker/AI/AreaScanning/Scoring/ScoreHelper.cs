using System;

namespace Kingmaker.AI.AreaScanning.Scoring;

public static class ScoreHelper
{
	public static readonly int ScoreTypesNum;

	static ScoreHelper()
	{
		ScoreTypesNum = Enum.GetNames(typeof(ScoreType)).Length;
	}
}
