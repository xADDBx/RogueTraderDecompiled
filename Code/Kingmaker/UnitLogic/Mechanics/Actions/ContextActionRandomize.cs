using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Utility.Attributes;
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

		[HideIf("UseContextValue")]
		public int Weight;

		public bool UseContextValue;

		[ShowIf("UseContextValue")]
		public ContextPropertyName ValueName;
	}

	[UsedImplicitly]
	[SerializeField]
	private ActionWrapper[] m_Actions = new ActionWrapper[0];

	public override string GetCaption()
	{
		return $"Randomize ({m_Actions.Length} variants)";
	}

	protected override void RunAction()
	{
		if (m_Actions.Length < 1)
		{
			return;
		}
		int num = ((IEnumerable<ActionWrapper>)m_Actions).Sum((Func<ActionWrapper, int>)GetWeight);
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
			num3 += GetWeight(actionWrapper);
			if (num3 >= num2)
			{
				actionWrapper.Action.Run();
				break;
			}
		}
	}

	private int GetWeight(ActionWrapper aw)
	{
		if (aw.UseContextValue)
		{
			return base.Context[aw.ValueName];
		}
		return aw.Weight;
	}
}
