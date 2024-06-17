using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class EntityLink
{
	public enum Type
	{
		Unit,
		Item,
		ItemBlueprint,
		CargoBlueprint,
		UnitFact,
		GroupAbility,
		UI,
		Encyclopedia,
		SkillcheckResult,
		SkillcheckDC,
		DialogExchange,
		UIProperty,
		SoulMarkShiftDirection,
		UnitStat,
		Unknown,
		Empty,
		DialogConditions
	}

	private static Dictionary<Type, string> m_Tags = new Dictionary<Type, string>
	{
		{
			Type.Unit,
			"u"
		},
		{
			Type.Item,
			"i"
		},
		{
			Type.ItemBlueprint,
			"ib"
		},
		{
			Type.CargoBlueprint,
			"cb"
		},
		{
			Type.UnitFact,
			"f"
		},
		{
			Type.GroupAbility,
			"a"
		},
		{
			Type.UI,
			"ui"
		},
		{
			Type.Encyclopedia,
			"Encyclopedia"
		},
		{
			Type.SkillcheckResult,
			"SkillcheckResult"
		},
		{
			Type.SkillcheckDC,
			"SkillcheckDC"
		},
		{
			Type.DialogExchange,
			"DialogExchange"
		},
		{
			Type.UIProperty,
			"uip"
		},
		{
			Type.SoulMarkShiftDirection,
			"SoulMarkShiftDirection"
		},
		{
			Type.UnitStat,
			"us"
		},
		{
			Type.Unknown,
			""
		},
		{
			Type.Empty,
			"Empty"
		},
		{
			Type.DialogConditions,
			"DialogConditions"
		}
	};

	public static string GetTag(Type type)
	{
		if (!m_Tags.ContainsKey(type))
		{
			Debug.LogError("EntityLinkType " + type.ToString() + " does not have tag");
			return "";
		}
		return m_Tags[type];
	}

	public static Type GetEntityType(string key)
	{
		Type result = Type.Empty;
		if (m_Tags.Values.Contains(key))
		{
			result = m_Tags.FirstOrDefault((KeyValuePair<Type, string> d) => d.Value == key).Key;
		}
		return result;
	}
}
