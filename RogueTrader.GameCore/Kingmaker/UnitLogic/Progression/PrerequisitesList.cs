using System;
using System.Collections.Generic;
using System.Text;
using Code.GameCore.ElementsSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression;

[Serializable]
public class PrerequisitesList : ElementsList, IFeaturePrerequisite, IHashable
{
	private class ToStringMark : ContextFlag<ToStringMark>
	{
	}

	public FeaturePrerequisiteComposition Composition;

	[SerializeReference]
	[ValidateNoNullEntries]
	public Prerequisite[] List = new Prerequisite[0];

	public override IEnumerable<Element> Elements => List;

	public bool Any => List.Any();

	public bool Empty => !Any;

	public bool Meet(IBaseUnitEntity unit)
	{
		using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(this);
		try
		{
			Prerequisite[] list = List;
			for (int i = 0; i < list.Length; i++)
			{
				bool flag = list[i].Meet(unit);
				if (flag && Composition == FeaturePrerequisiteComposition.Or)
				{
					elementsDebugger?.SetResult(1);
					return true;
				}
				if (!flag && Composition == FeaturePrerequisiteComposition.And)
				{
					elementsDebugger?.SetResult(0);
					return false;
				}
			}
			bool flag2 = Composition == FeaturePrerequisiteComposition.And;
			elementsDebugger?.SetResult(flag2 ? 1 : 0);
			return flag2;
		}
		catch (Exception exception)
		{
			elementsDebugger?.SetException(exception);
			return false;
		}
	}

	public override string ToString()
	{
		bool flag = ContextData<ToStringMark>.Current;
		using (ContextData<ToStringMark>.Request())
		{
			using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
			StringBuilder builder = pooledStringBuilder.Builder;
			if (flag)
			{
				builder.Append('(');
			}
			bool flag2 = true;
			Prerequisite[] list = List;
			foreach (Prerequisite prerequisite in list)
			{
				if (!flag2)
				{
					builder.Append((Composition == FeaturePrerequisiteComposition.And) ? " && " : " || ");
				}
				if (flag2)
				{
					flag2 = false;
				}
				builder.Append(prerequisite.GetCaption(useLineBreaks: false));
			}
			if (flag)
			{
				builder.Append(')');
			}
			return builder.ToString();
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
