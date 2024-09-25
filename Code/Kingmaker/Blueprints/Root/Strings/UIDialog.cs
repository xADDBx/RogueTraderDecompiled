using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIDialog
{
	public LocalizedString SucccedeedCheckFormat;

	public LocalizedString FailedCheckFormat;

	public LocalizedString SoulMarkShiftFormat;

	public LocalizedString Succeeded;

	public LocalizedString Failed;

	public LocalizedString AligmentShiftedFormat;

	public LocalizedString AlignmentRequirementLabel;

	public LocalizedString AnswerDialogueFormat;

	public LocalizedString AnswerDialogueBeFormat;

	public LocalizedString AnswerStringWithCheckFormat;

	public LocalizedString AnswerStringWithCheckBeFormat;

	public LocalizedString AnswerShowCheckFormat;

	public LocalizedString AnswerYouNeedFullCargo;

	public LocalizedString AnswerYouNeedItem;

	public LocalizedString OpenGlossary;

	public LocalizedString CloseGlossary;

	public LocalizedString InterchapterSkip;

	public LocalizedString InterchapterSkipConsole;

	public LocalizedString ScrollToNew;

	public LocalizedString CargoRequiredText;

	public LocalizedString OperationOrConditionDesc;

	public LocalizedString OperationAndConditionDesc;

	public LocalizedString ShowVotes;

	public LocalizedString HideVotes;

	public static UIDialog Instance => UIStrings.Instance.Dialog;
}
