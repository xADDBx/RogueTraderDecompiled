using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class LuredStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Sequence(new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет в 50 МП\n", 50), TaskNodeSetupMoveCommand.ToLureCaster("Построение пути к приманившему\n"), new TaskNodeExecuteMoveCommand("Выполнение команды движения\n"), new TaskNodeWaitCommandsDone("Ожидание завершения команд\n"))
		{
			DebugName = "Субдерево - стратегия приманенного юнита"
		};
	}
}
