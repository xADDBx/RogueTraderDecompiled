using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Localization;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UnityEngine;

namespace Kingmaker.Blueprints.Console;

[TypeId("aa2c115aa4f140d58da3536a36101276")]
public class GamePadTexts : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<GamePadTexts>
	{
	}

	[Serializable]
	private class GamePadTextsEntity
	{
		public RewiredActionType ActionType;

		public LocalizedString Text;
	}

	[Serializable]
	private class GamePadTextsLayer
	{
		public GamePadControlLayer LayerType;

		public List<GamePadTextsEntity> Texts;
	}

	[SerializeField]
	private List<GamePadTextsLayer> m_Layers;

	public static GamePadTexts Instance => BlueprintRoot.Instance.ConsoleRoot.Texts.Get();

	public string GetText(GamePadControlLayer layerType, RewiredActionType actionType)
	{
		LocalizedString localizedString = m_Layers.FirstOrDefault((GamePadTextsLayer l) => l.LayerType == layerType)?.Texts.FirstOrDefault((GamePadTextsEntity t) => t.ActionType == actionType)?.Text;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}
}
