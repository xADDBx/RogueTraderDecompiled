using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.GameModes;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Serializable]
[TypeId("fea7ec3b8ccf495bb150c0374fe9c424")]
public class CheckLoadedAreaType : Condition
{
	[Flags]
	private enum Type
	{
		Surface = 1,
		StarSystem = 2,
		SpaceCombat = 4,
		GlobalMap = 8
	}

	[SerializeField]
	[EnumFlagsAsButtons]
	private Type m_Type = Type.Surface;

	protected override string GetConditionCaption()
	{
		return $"Loaded area type is ({m_Type})";
	}

	protected override bool CheckCondition()
	{
		GameModeType gameModeType = Game.Instance.CurrentlyLoadedArea?.AreaStatGameMode;
		if (gameModeType == GameModeType.Default)
		{
			return (m_Type & Type.Surface) != 0;
		}
		if (gameModeType == GameModeType.StarSystem)
		{
			return (m_Type & Type.StarSystem) != 0;
		}
		if (gameModeType == GameModeType.SpaceCombat)
		{
			return (m_Type & Type.SpaceCombat) != 0;
		}
		if (gameModeType == GameModeType.GlobalMap)
		{
			return (m_Type & Type.GlobalMap) != 0;
		}
		return false;
	}
}
