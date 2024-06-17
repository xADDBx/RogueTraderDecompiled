using System;
using System.Linq;
using Kingmaker.Utility.GuidUtility;

namespace Kingmaker.Utility.Random;

public static class PFUuid
{
	public static Uuid BehaviourTreeNode;

	public static readonly Uuid[] All;

	public static readonly Uuid[] Serializable;

	public static readonly Uuid[] NonSerializable;

	static PFUuid()
	{
		BehaviourTreeNode = new Uuid("UUID_BehaviourTreeNode", new Guid("59c00b86-2ea8-4ff6-b7c3-850877ae7eed"));
		All = new Uuid[2]
		{
			Uuid.Instance,
			BehaviourTreeNode
		};
		Serializable = All;
		NonSerializable = All.Except(Serializable).ToArray();
	}
}
