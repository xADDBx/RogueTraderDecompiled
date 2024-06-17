using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

[Serializable]
public class GamePadIcons
{
	public static GamePadIcons Instance;

	[SerializeField]
	private List<IconsByAction> m_Icons;

	public static void SetInstance(GamePadIcons instance)
	{
		Instance = instance;
	}

	public Sprite GetIcon(RewiredActionType type)
	{
		return GetIcon((int)type);
	}

	public Sprite GetIcon(int actionId)
	{
		IconsByAction iconsByAction = m_Icons.FirstOrDefault((IconsByAction i) => i.Type == (RewiredActionType)actionId);
		return (iconsByAction?.Icons.FirstOrDefault((SpriteByConsole i) => i.Console == GamePad.Instance.Type) ?? iconsByAction?.Icons.FirstOrDefault((SpriteByConsole i) => i.Console == ConsoleType.Common))?.Icon;
	}

	public Sprite GetCustomDPadIcon(List<int> actionIds)
	{
		bool flag = actionIds.Contains(4);
		bool flag2 = actionIds.Contains(5);
		bool flag3 = actionIds.Contains(6);
		bool flag4 = actionIds.Contains(7);
		if (flag && flag2 && flag3 && flag4)
		{
			return GetIcon(RewiredActionType.DPadFull);
		}
		if (flag && flag2)
		{
			return GetIcon(RewiredActionType.DPadHorizontal);
		}
		if (flag3 && flag4)
		{
			return GetIcon(RewiredActionType.DPadVertical);
		}
		return null;
	}
}
