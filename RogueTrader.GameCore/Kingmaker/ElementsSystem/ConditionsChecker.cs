using System;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.QA;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
[HashRoot]
public class ConditionsChecker : IHashable
{
	public Operation Operation;

	[SerializeReference]
	public Condition[] Conditions;

	public bool HasConditions
	{
		get
		{
			if (Conditions != null)
			{
				return Conditions.Length != 0;
			}
			return false;
		}
	}

	public bool Check()
	{
		return Check(null);
	}

	public bool Check([CanBeNull] IConditionDebugContext debugContext)
	{
		if (!HasConditions)
		{
			return true;
		}
		Condition[] conditions = Conditions;
		foreach (Condition condition in conditions)
		{
			if (condition == null)
			{
				continue;
			}
			try
			{
				using ElementsDebugScope elementsDebugScope = ElementsDebugScope.Open(condition);
				bool flag = condition.Check(debugContext);
				elementsDebugScope?.SetState(flag);
				if (Operation == Operation.And && !flag)
				{
					return false;
				}
				if (Operation == Operation.Or && flag)
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				if (CutscenePlayerDataScope.Current != null)
				{
					throw;
				}
				if (!(ex is ElementLogicException))
				{
					ex = new ElementLogicException(condition, ex);
				}
				PFLog.Actions.ExceptionWithReport(ex, null);
				return false;
			}
			finally
			{
			}
		}
		return Operation == Operation.And;
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
