using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class ResponseToAoOThreatStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Selector(new Sequence("Отступление с помощью абилки\n", new TaskNodeSelectAbilityToEscapeFromThreat("Перебор абилок с компонентом AiEscapeFromThreat,чтобы найти цель, если получилось, запоминание абилки и цели\n"), new TaskNodeCastAbility("Каст абилки\n")), new Sequence("Отступление с помощью перемещения на безопасную клетку\n", new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет имеющихся МП, но для swarm-ов лимит в 3МП, для common-ов в 4МП\n"), new TaskNodeFindPositionForRetreat("Поиск позиции для отступления, т.е. клетки без угрозы атаки по возможности, и запоминание её как лучшей позиции\n"), TaskNodeSetupMoveCommand.ToBetterPosition("Построение пути к лучшей позиции\n"), new TaskNodeExecuteMoveCommand("Выполнение команды движения\n")))
		{
			DebugName = "Субдерево - стратегия реагирования на угрозу атаки по возможности"
		};
	}
}
