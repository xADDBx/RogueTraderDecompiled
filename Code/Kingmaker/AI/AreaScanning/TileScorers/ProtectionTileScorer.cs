using System.Collections.Generic;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AI.AreaScanning.TileScorers;

public class ProtectionTileScorer : TileScorer
{
	private static readonly float[] hideCoverValues = new float[4] { 0f, 0.0004f, 0.02f, 1f };

	protected override Score CalculateThreatsScore(DecisionContext context, CustomGridNodeBase node)
	{
		AiBrainHelper.ThreatsInfo threatsInfo = context.FindThreats(context.Unit, node);
		context.UnitMoveVariants.cells.TryGetValue(node, out var value);
		int num = value.ProvokedAttacks;
		int num2 = threatsInfo.aes.Count + value.EnteredAoE + value.StepsInsideDamagingAoE;
		int count = threatsInfo.aooUnits.Count;
		AbilityData abilityData = context.ConsideringAbility ?? context.Ability;
		if (abilityData != null && abilityData.UsingInThreateningArea == BlueprintAbility.UsingInThreateningAreaType.WillCauseAOO)
		{
			num += count;
		}
		return new Score(-num, -num2, -count);
	}

	protected override Score CalculateHideScore(DecisionContext context, CustomGridNodeBase node)
	{
		Dictionary<LosCalculations.CoverType, float> ensuredCovers = GetEnsuredCovers(node, context.Unit.SizeRect, context.Enemies);
		float hideValue = GetHideValue(node, context.Unit.SizeRect, context.Enemies);
		return new Score((ensuredCovers[LosCalculations.CoverType.Invisible] + ensuredCovers[LosCalculations.CoverType.Full] == (float)context.Enemies.Count) ? 1 : 0, (ensuredCovers[LosCalculations.CoverType.Invisible] + ensuredCovers[LosCalculations.CoverType.Full] + ensuredCovers[LosCalculations.CoverType.Half] == (float)context.Enemies.Count) ? 1 : 0, (ensuredCovers[LosCalculations.CoverType.Invisible] + ensuredCovers[LosCalculations.CoverType.Full] + ensuredCovers[LosCalculations.CoverType.Half]) / (float)context.Enemies.Count, (ensuredCovers[LosCalculations.CoverType.Invisible] + ensuredCovers[LosCalculations.CoverType.Full]) / (float)context.Enemies.Count, hideValue);
	}

	protected override Score CalculateStayingAwayScore(DecisionContext context, CustomGridNodeBase node)
	{
		float num = 0f;
		foreach (TargetInfo enemy in context.Enemies)
		{
			int num2 = WarhammerGeometryUtils.DistanceToInCells(node.Vector3Position, context.Unit.SizeRect, context.Unit.Forward, enemy.Entity.Position, enemy.Entity.SizeRect, enemy.Entity.Forward);
			BlueprintUnit blueprint = ((BaseUnitEntity)enemy.Entity).Blueprint;
			float num3 = (float)(2 * blueprint.WarhammerInitialAPBlue) / blueprint.WarhammerMovementApPerCell;
			num += Mathf.Min(num2, num3) / num3;
		}
		return new Score(num / (float)context.Enemies.Count);
	}

	private float GetHideValue(CustomGridNodeBase node, IntRect unitSizeRect, List<TargetInfo> enemies)
	{
		float num = 0f;
		foreach (TargetInfo enemy in enemies)
		{
			num += hideCoverValues[(int)LosCalculations.GetWarhammerLos(enemy.Node, enemy.Entity.SizeRect, node, unitSizeRect).CoverType];
		}
		return num;
	}

	private Dictionary<LosCalculations.CoverType, float> GetEnsuredCovers(CustomGridNodeBase node, IntRect unitSizeRect, List<TargetInfo> enemies)
	{
		Dictionary<LosCalculations.CoverType, float> dictionary = new Dictionary<LosCalculations.CoverType, float>
		{
			{
				LosCalculations.CoverType.None,
				0f
			},
			{
				LosCalculations.CoverType.Half,
				0f
			},
			{
				LosCalculations.CoverType.Full,
				0f
			},
			{
				LosCalculations.CoverType.Invisible,
				0f
			}
		};
		foreach (TargetInfo enemy in enemies)
		{
			if (enemy.AiConsideredMoveVariants == null)
			{
				continue;
			}
			LosCalculations.CoverType coverType = LosCalculations.CoverType.Invisible;
			foreach (CustomGridNodeBase aiConsideredMoveVariant in enemy.AiConsideredMoveVariants)
			{
				LosDescription warhammerLos = LosCalculations.GetWarhammerLos(aiConsideredMoveVariant, enemy.Entity.SizeRect, node, unitSizeRect);
				if ((LosCalculations.CoverType)warhammerLos < coverType)
				{
					coverType = warhammerLos;
				}
			}
			dictionary[coverType] += 1f;
		}
		return dictionary;
	}
}
