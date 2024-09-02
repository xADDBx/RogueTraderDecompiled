using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.Utility;

public static class UIConstsExtensions
{
	public static string GetValueWithSign(int value)
	{
		return UIUtility.AddSign(value);
	}

	public static string GetAnswerFormattedString(BlueprintAnswer answer, string bind, int index)
	{
		string format = ((Game.Instance.DialogController.Dialog.Type == DialogType.Book) ? UIDialog.Instance.AnswerDialogueBeFormat : UIDialog.Instance.AnswerDialogueFormat);
		string stringByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(bind));
		return string.Format(format, stringByBinding.Empty() ? index.ToString() : stringByBinding, GetAnswerText(answer));
	}

	public static string GetAnswerText(BlueprintAnswer answer)
	{
		bool flag = Game.Instance.DialogController.Dialog.Type == DialogType.Book;
		string checkFormat = (flag ? UIDialog.Instance.AnswerStringWithCheckBeFormat : UIDialog.Instance.AnswerStringWithCheckFormat);
		GameDialogsSettings dialogs = SettingsRoot.Game.Dialogs;
		string text = string.Empty;
		if ((bool)dialogs.ShowSkillcheckDC)
		{
			text = answer.SkillChecks.Aggregate("", (string current, CheckData skillCheck) => current + string.Format(checkFormat, UIUtility.PackKeys(EntityLink.Type.SkillcheckDC, skillCheck.Type), LocalizedTexts.Instance.Stats.GetText(skillCheck.Type), skillCheck.DC + (int)SettingsRoot.Difficulty.SkillCheckModifier));
		}
		string text2 = string.Empty;
		if (answer.HasExchangeData)
		{
			text2 = string.Format(UIConfig.Instance.UIDialogExchangeLinkFormat, answer.AssetGuid);
		}
		if ((bool)dialogs.ShowAlignmentRequirements && !answer.SoulMarkRequirement.Empty)
		{
			string arg = UIUtility.GetSoulMarkDirectionText(answer.SoulMarkRequirement.Direction).Text ?? "";
			text = string.Format(arg1: (!answer.SoulMarkRequirement.CheckByRank) ? (UIUtility.GetSoulMarkRankText(SoulMarkShiftExtension.GetSoulMarkRankIndex(answer.SoulMarkRequirement.Direction, answer.SoulMarkRequirement.Value) + 1).Text ?? "") : UIUtility.GetSoulMarkRankText(answer.SoulMarkRequirement.Rank).Text, format: UIDialog.Instance.AlignmentRequirementLabel, arg0: arg) + text;
		}
		if ((bool)dialogs.ShowSkillcheckResult && answer.HasShowCheck)
		{
			text = string.Format(UIDialog.Instance.AnswerShowCheckFormat, UIUtility.PackKeys(EntityLink.Type.SkillcheckDC, answer.ShowCheck.Type), LocalizedTexts.Instance.Stats.GetText(answer.ShowCheck.Type), text);
		}
		if ((bool)dialogs.ShowAlignmentShiftsInAnswer && answer.SoulMarkRequirement.Empty && answer.SoulMarkShift.Value != 0 && (bool)dialogs.ShowAlignmentShiftsInAnswer)
		{
			text = string.Format(UIDialog.Instance.AligmentShiftedFormat, UIUtility.GetSoulMarkDirectionText(answer.SoulMarkShift.Direction).Text) + text;
		}
		return text + (text.Empty() ? string.Empty : " ") + text2 + ((!answer.IsSoulMarkRequirementSatisfied()) ? "" : answer.DisplayText);
	}
}
