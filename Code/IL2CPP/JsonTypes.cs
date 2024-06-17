using System.Collections.Generic;
using Kingmaker.EntitySystem.Stats.Base;
using Newtonsoft.Json.Utilities;
using UnityEngine.Scripting;

namespace IL2CPP;

[Preserve]
public class JsonTypes
{
	[Preserve]
	public static void NeverCalled()
	{
		AotHelper.EnsureType<HashSet<StatType>>();
		AotHelper.EnsureDictionary<string, object>();
	}
}
