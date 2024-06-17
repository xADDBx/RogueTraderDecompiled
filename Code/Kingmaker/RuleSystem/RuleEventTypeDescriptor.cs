using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem;

public class RuleEventTypeDescriptor
{
	public static List<Type> GetAllEventTypesTillBase(IRulebookEvent evt)
	{
		List<Type> list = new List<Type>();
		using (ContextData<StackOverflowProtection>.Request())
		{
			Type type = evt.GetType();
			while (type != null && type != typeof(RulebookEvent))
			{
				list.Add(type);
				type = type.BaseType;
			}
		}
		return list;
	}
}
