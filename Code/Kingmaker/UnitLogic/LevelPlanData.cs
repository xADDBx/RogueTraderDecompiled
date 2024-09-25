using JetBrains.Annotations;
using Kingmaker.UnitLogic.Levelup.Obsolete.Actions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class LevelPlanData
{
	private static class ILevelUpActionArrayHasher
	{
		public static Hash128 GetHash128(ILevelUpAction[] obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			for (int i = 0; i < obj.Length; i++)
			{
				Hash128 val = (obj[i] as IHashable)?.GetHash128() ?? default(Hash128);
				result.Append(ref val);
			}
			return result;
		}
	}

	[JsonProperty]
	public readonly int Level;

	[NotNull]
	[JsonProperty]
	[HasherCustom(Type = typeof(ILevelUpActionArrayHasher))]
	public readonly ILevelUpAction[] Actions;

	[JsonConstructor]
	public LevelPlanData()
	{
	}

	public LevelPlanData(int level, [NotNull] ILevelUpAction[] actions)
	{
		Level = level;
		Actions = actions;
	}

	public void PostLoad()
	{
		ILevelUpAction[] actions = Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i]?.PostLoad();
		}
	}
}
