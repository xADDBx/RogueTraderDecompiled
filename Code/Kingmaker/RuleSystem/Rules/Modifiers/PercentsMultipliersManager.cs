using System;
using JetBrains.Annotations;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class PercentsMultipliersManager : AbstractModifiersManager
{
	public float Value => GetPercentsMultiplication();

	public float GetValue(Predicate<Modifier> filter)
	{
		return GetPercentsMultiplication(filter);
	}

	protected float GetPercentsMultiplication([CanBeNull] Predicate<Modifier> filter = null)
	{
		float num = 1f;
		for (int i = 0; i < base.List.Count; i++)
		{
			if (filter == null || filter(base.List[i]))
			{
				num *= (float)base.List[i].Value / 100f;
			}
		}
		return num;
	}

	public void CopyFrom(PercentsMultipliersManager other)
	{
		CopyFrom(other, null);
	}
}
