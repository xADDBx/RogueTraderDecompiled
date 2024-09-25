using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.DialogSystem.Blueprints;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public class AnswerShowData : IDialogShowData
{
	public readonly BlueprintAnswer Answer;

	public readonly BlueprintUnit CharacterBlueprint;

	public readonly string CharacterName;

	public readonly DialogType DialogType;

	public AnswerShowData(BlueprintAnswer answer, DialogType dialogType, string characterName = "", BlueprintUnit characterBlueprint = null)
	{
		Answer = answer;
		DialogType = dialogType;
		CharacterName = characterName;
		CharacterBlueprint = characterBlueprint;
	}

	public string GetText(DialogColors colors)
	{
		DialogType dialogType = DialogType;
		if (dialogType == DialogType.Common || dialogType == DialogType.StarSystemEvent)
		{
			string arg = ColorUtility.ToHtmlStringRGB(CharacterBlueprint.Color * colors.NameColorMultiplyer);
			return string.Format(DialogFormats.AnswerFormatWithColorName, arg, CharacterName, Answer.Text.Text);
		}
		return string.Format(DialogFormats.AnswerFormatWithoutName, Answer.Text);
	}
}
