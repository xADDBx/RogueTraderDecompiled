using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.AI.AreaScanning;

public abstract class TileScorer
{
	public virtual GraphNode GetHighestScoreNode(DecisionContext context, List<CustomGridNodeBase> nodes, ScoreOrder scoreOrder = null)
	{
		if (scoreOrder == null)
		{
			scoreOrder = context.ScoreOrder;
		}
		List<CustomGridNodeBase> list = TempList.Get<CustomGridNodeBase>();
		list.AddRange(nodes);
		List<CustomGridNodeBase> list2 = TempList.Get<CustomGridNodeBase>();
		foreach (ScorePair item in scoreOrder)
		{
			if (item.factor == ScoreFactor.Ignored)
			{
				continue;
			}
			Score score = default(Score);
			if (item.factor == ScoreFactor.Inverted)
			{
				score += new Score(float.MaxValue);
			}
			list2.Clear();
			foreach (CustomGridNodeBase item2 in list)
			{
				Score score2 = CalculateTileScore(context, item2, item.type);
				if ((item.factor == ScoreFactor.Default && score2 > score) || (item.factor == ScoreFactor.Inverted && score2 < score))
				{
					score = score2;
					list2.Clear();
					list2.Add(item2);
				}
				else if (score2 == score)
				{
					list2.Add(item2);
				}
			}
			if (list2.Count == 1)
			{
				return list2[0];
			}
			list.Clear();
			list.AddRange(list2);
		}
		return list2.FirstOrDefault();
	}

	private Score CalculateTileScore(DecisionContext context, CustomGridNodeBase node, ScoreType scoreType)
	{
		return scoreType switch
		{
			ScoreType.EnemyCoverScore => CalculateEnemyCoverScore(context, node), 
			ScoreType.EffectiveDistanceScore => CalculateEffectiveDistanceScore(context, node), 
			ScoreType.EnemyThreatScore => CalculateEnemyThreatScore(context, node), 
			ScoreType.PriorityScore => CalculatePriorityScore(context, node), 
			ScoreType.ThreatsScore => CalculateThreatsScore(context, node), 
			ScoreType.HideScore => CalculateHideScore(context, node), 
			ScoreType.StayingAwayScore => CalculateStayingAwayScore(context, node), 
			ScoreType.EnemyHPLeftScore => CalculateEnemyHPLeftScore(context, node), 
			ScoreType.ClosinessScore => CalculateClosinessScore(context, node), 
			ScoreType.BodyGuardScore => CalculateBodyGuardScore(context, node), 
			_ => default(Score), 
		};
	}

	protected virtual Score CalculateEnemyCoverScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateEffectiveDistanceScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateEnemyThreatScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculatePriorityScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateThreatsScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateHideScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateStayingAwayScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateEnemyHPLeftScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateClosinessScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	protected virtual Score CalculateBodyGuardScore(DecisionContext context, CustomGridNodeBase node)
	{
		return default(Score);
	}

	public string GetReadableString(ScoreOrder order, ScoreSet score)
	{
		StringBuilder stringBuilder = new StringBuilder(GetType().Name);
		stringBuilder.Append("{");
		foreach (ScoreType item in order.Order)
		{
			Score score2 = score.Get(item);
			if (!score2.IsZero)
			{
				stringBuilder.Append($"{item}: [");
				for (int i = 0; i < score2.values.Length - 1; i++)
				{
					stringBuilder.Append($"{score2.values[i]}, ");
				}
				stringBuilder.Append($"{score2.values[score2.values.Length - 1]}], ");
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
