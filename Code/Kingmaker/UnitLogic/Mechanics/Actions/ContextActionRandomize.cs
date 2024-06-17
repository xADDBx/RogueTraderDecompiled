using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("0cdc69d3ddfd42f28045114acc14e069")]
public class ContextActionRandomize : ContextAction
{
	[Serializable]
	private class ActionWrapper
	{
		public ActionList Action;

		public int Weight;
	}

	[UsedImplicitly]
	[SerializeField]
	private ActionWrapper[] m_Actions = new ActionWrapper[0];

	public override string GetCaption()
	{
		return $"Randomize ({m_Actions.Length} variants)";
	}

	public override void RunAction()
	{
		if (m_Actions.Length < 1)
		{
			return;
		}
		int num = m_Actions.Sum((ActionWrapper aw) => aw.Weight);
		if (num < 1)
		{
			m_Actions.Random(PFStatefulRandom.Mechanics)?.Action.Run();
			return;
		}
		int num2 = PFStatefulRandom.Mechanics.Range(1, num + 1);
		int num3 = 0;
		ActionWrapper[] actions = m_Actions;
		foreach (ActionWrapper actionWrapper in actions)
		{
			num3 += actionWrapper.Weight;
			if (num3 >= num2)
			{
				actionWrapper.Action.Run();
				break;
			}
		}
	}
}
