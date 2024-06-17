using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Pathfinding;
using Pathfinding.Util;

namespace Kingmaker.AI;

public class TargetInfo : IAstarPooledObject
{
	public static TargetInfo Empty = new TargetInfo();

	[CanBeNull]
	public MechanicEntity Entity;

	public CustomGridNodeBase Node;

	public List<GraphNode> AiConsideredMoveVariants;

	public virtual void Init(MechanicEntity unit)
	{
		Entity = unit;
		Node = unit.GetNearestNodeXZ();
	}

	public virtual void Release()
	{
		TargetInfo obj = this;
		ObjectPool<TargetInfo>.Release(ref obj);
	}

	public static T Claim<T>(MechanicEntity unit) where T : TargetInfo, new()
	{
		T val = ObjectPool<T>.Claim();
		val.Init(unit);
		return val;
	}

	public virtual void OnEnterPool()
	{
		Entity = null;
		Node = null;
		if (AiConsideredMoveVariants != null)
		{
			AiConsideredMoveVariants.Clear();
			AiConsideredMoveVariants = null;
		}
	}

	public override string ToString()
	{
		return (Entity?.Blueprint?.ToString() ?? "<null>") ?? "";
	}
}
