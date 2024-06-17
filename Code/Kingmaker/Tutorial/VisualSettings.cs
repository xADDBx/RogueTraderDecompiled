using System;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UnityEngine;

namespace Kingmaker.Tutorial;

[Serializable]
public class VisualSettings
{
	[SerializeField]
	private SpriteLink m_Picture;

	[SerializeField]
	private VideoLink m_Video;

	[SerializeField]
	private VisualOverride m_XBox;

	[SerializeField]
	private VisualOverride m_PS4;

	public VisualOverride XBox => m_XBox;

	public VisualOverride PS4 => m_PS4;

	public SpriteLink Picture => GetVisualOverride()?.Picture ?? m_Picture;

	public VideoLink Video => GetVisualOverride()?.Video ?? m_Video;

	private VisualOverride GetVisualOverride()
	{
		VisualOverride result = null;
		if (Game.Instance.IsControllerGamepad)
		{
			switch (GamePad.Instance.Type)
			{
			case ConsoleType.Common:
			case ConsoleType.XBox:
			case ConsoleType.Switch:
			case ConsoleType.SteamController:
			case ConsoleType.SteamDeck:
				result = m_XBox;
				break;
			case ConsoleType.PS4:
			case ConsoleType.PS5:
				result = m_PS4;
				break;
			}
		}
		return result;
	}
}
