using System;
using System.Collections.Generic;
using Code.GameCore.ElementsSystem;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Utility.CodeTimer;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
[HashRoot]
public class ConditionsChecker : ElementsList, IHashable
{
	public Operation Operation;

	[SerializeReference]
	public Condition[] Conditions;

	public override IEnumerable<Element> Elements => Conditions;

	public bool HasConditions
	{
		get
		{
			Condition[] conditions = Conditions;
			if (conditions != null)
			{
				return conditions.Length > 0;
			}
			return false;
		}
	}

	public bool Check([CanBeNull] IConditionDebugContext debugContext = null, bool @unsafe = false)
	{
		using (ProfileScope.New("ConditionChecker"))
		{
			using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(this);
			if (!HasConditions)
			{
				elementsDebugger?.SetResult(1);
				return true;
			}
			Exception ex = null;
			bool flag = Operation == Operation.And;
			Condition[] conditions = Conditions;
			foreach (Condition condition in conditions)
			{
				if (condition == null)
				{
					continue;
				}
				bool flag2 = false;
				try
				{
					flag2 = condition.Check(this, debugContext);
				}
				catch (Exception ex2)
				{
					if (ex == null)
					{
						ex = ex2;
						elementsDebugger?.SetException(ex);
					}
					if (@unsafe || CutscenePlayerDataScope.Current != null)
					{
						throw;
					}
				}
				if (Operation == Operation.And && !flag2)
				{
					flag = false;
					break;
				}
				if (Operation == Operation.Or && flag2)
				{
					flag = true;
					break;
				}
			}
			if (ex == null)
			{
				elementsDebugger?.SetResult(flag ? 1 : 0);
			}
			return flag;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
