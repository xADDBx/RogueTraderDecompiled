using System;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.UI.Common;

[Serializable]
public class NetLobbyTutorialBlockInfo
{
	[Header("PC Part")]
	[SerializeField]
	private SpriteLink Sprite;

	[SerializeField]
	private LocalizedString Description;

	[Header("Console Part")]
	[SerializeField]
	private SpriteLink ConsoleSprite;

	[SerializeField]
	private LocalizedString ConsoleDescription;

	public Sprite BlockSprite
	{
		get
		{
			if (!Game.Instance.IsControllerMouse && !(ConsoleSprite?.Load() == null))
			{
				return ConsoleSprite?.Load();
			}
			return Sprite?.Load();
		}
	}

	public LocalizedString BlockDescription
	{
		get
		{
			if (!Game.Instance.IsControllerMouse && !string.IsNullOrWhiteSpace(ConsoleDescription))
			{
				return ConsoleDescription;
			}
			return Description;
		}
	}
}
