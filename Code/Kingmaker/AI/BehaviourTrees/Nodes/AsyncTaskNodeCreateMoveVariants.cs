using System;
using System.Threading.Tasks;
using Kingmaker.AI.AreaScanning;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class AsyncTaskNodeCreateMoveVariants : AsyncTaskNode
{
	private class LogMessage : AILogObject
	{
		private readonly int maxMove;

		private readonly int availableMP;

		public LogMessage(int maxMove, int availableMP)
		{
			this.maxMove = maxMove;
			this.availableMP = availableMP;
		}

		public override string GetLogString()
		{
			return $"Move variants created for move range {maxMove} (available MP: {availableMP})";
		}
	}

	private readonly int? maxMove;

	private const int SwarmMaxMove = 3;

	private const int CommonMaxMove = 4;

	public AsyncTaskNodeCreateMoveVariants()
	{
	}

	public AsyncTaskNodeCreateMoveVariants(int maxMove)
	{
		this.maxMove = maxMove;
	}

	protected override async Task<Status> Process(Blackboard blackboard)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		BaseUnitEntity unit = decisionContext.Unit;
		int maxMove = GetMaxMove(decisionContext);
		DecisionContext decisionContext2 = decisionContext;
		decisionContext2.UnitMoveVariants = await AiAreaScanner.FindAllReachableNodesAsync(unit, unit.Position, maxMove);
		AILogger.Instance.Log(new LogMessage(maxMove, (int)unit.CombatState.ActionPointsBlue));
		return Status.Success;
	}

	private int GetMaxMove(DecisionContext context)
	{
		if (maxMove.HasValue)
		{
			return maxMove.Value;
		}
		int val = (int)context.Unit.CombatState.ActionPointsBlue;
		val = Math.Min(GetMaxMoveLimitByDifficultyType(context.Unit.Blueprint.DifficultyType), val);
		BlueprintAbility ability = (context.ConsideringAbility ?? context.Ability)?.Blueprint;
		AbilitySettings abilitySettings = context.Unit.GetBrainOptional()?.Blueprint?.GetCustomAbilitySettings(ability);
		if (abilitySettings != null && abilitySettings.HasMovePointsLimit)
		{
			val = Math.Min(abilitySettings.MovePointsLimit, val);
		}
		return val;
	}

	private static int GetMaxMoveLimitByDifficultyType(UnitDifficultyType difficultyType)
	{
		return difficultyType switch
		{
			UnitDifficultyType.Swarm => 3, 
			UnitDifficultyType.Common => 4, 
			_ => 1000, 
		};
	}
}
