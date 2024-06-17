using System;

namespace Kingmaker.AI.AreaScanning.Scoring;

[Serializable]
public struct ScorePair
{
	public ScoreType type;

	public ScoreFactor factor;
}
