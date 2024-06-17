using System;
using System.Text;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression;

[Serializable]
public class PrerequisitesList : IFeaturePrerequisite
{
	private class ToStringMark : ContextFlag<ToStringMark>
	{
	}

	public FeaturePrerequisiteComposition Composition;

	[SerializeReference]
	[ValidateNoNullEntries]
	public Prerequisite[] List = new Prerequisite[0];

	public bool Any => List.Any();

	public bool Empty => !Any;

	public bool Meet(IBaseUnitEntity unit)
	{
		Prerequisite[] list = List;
		for (int i = 0; i < list.Length; i++)
		{
			bool flag = list[i].Meet(unit);
			if (flag && Composition == FeaturePrerequisiteComposition.Or)
			{
				return true;
			}
			if (!flag && Composition == FeaturePrerequisiteComposition.And)
			{
				return false;
			}
		}
		return Composition == FeaturePrerequisiteComposition.And;
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
				builder.Append(prerequisite.GetCaption());
			}
			if (flag)
			{
				builder.Append(')');
			}
			return builder.ToString();
		}
	}
}
