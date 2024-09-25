using System.Linq;

namespace Kingmaker.AI.AreaScanning.Scoring;

public struct ScoreSet
{
	private Score[] scores;

	public bool IsZero
	{
		get
		{
			if (scores != null)
			{
				return scores.All((Score s) => s.IsZero);
			}
			return true;
		}
	}

	public void Set(ScoreType type, Score score)
	{
		if (scores == null)
		{
			Init();
		}
		scores[(int)type] = score;
	}

	public Score Get(ScoreType type)
	{
		if (scores == null)
		{
			return Score.zero;
		}
		return scores[(int)type];
	}

	private void Init()
	{
		scores = new Score[ScoreHelper.ScoreTypesNum];
	}

	public static ScoreSet operator +(ScoreSet lhs, ScoreSet rhs)
	{
		ScoreSet result = default(ScoreSet);
		if (lhs.IsZero && rhs.IsZero)
		{
			return result;
		}
		result.Init();
		if (lhs.IsZero)
		{
			rhs.scores.CopyTo(result.scores, 0);
			return result;
		}
		if (rhs.IsZero)
		{
			lhs.scores.CopyTo(result.scores, 0);
			return result;
		}
		for (int i = 0; i < ScoreHelper.ScoreTypesNum; i++)
		{
			result.scores[i] = lhs.scores[i] + rhs.scores[i];
		}
		return result;
	}
}
