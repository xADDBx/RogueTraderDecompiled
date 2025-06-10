using Kingmaker.AI.BehaviourTrees;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker;

public interface ICustomBehaviourTreeBuilder
{
	BehaviourTree Create(MechanicEntity entity);
}
