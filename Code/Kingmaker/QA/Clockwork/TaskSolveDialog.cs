using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class TaskSolveDialog : ClockworkRunnerTask
{
	private static Dictionary<BlueprintAnswer, float> s_AnswerTime = new Dictionary<BlueprintAnswer, float>();

	private static BlueprintDialog s_LastDialog;

	public TaskSolveDialog(ClockworkRunner runner)
		: base(runner)
	{
	}

	protected override IEnumerator Routine()
	{
		DialogController controller = Game.Instance.DialogController;
		BlueprintDialog dialog = Game.Instance.DialogController.Dialog;
		if (s_LastDialog != dialog)
		{
			s_LastDialog = dialog;
			s_AnswerTime.Clear();
		}
		PlayData.DialogEntry dialogData = Runner.Data.GetDialogData(dialog);
		dialogData.Seen = true;
		dialogData.LastTalkTime = Game.Instance.TimeController.GameTime;
		while (Game.Instance.CurrentMode == GameModeType.Dialog)
		{
			yield return 1f;
			while (!controller.Answers.Any())
			{
				if (Game.Instance.CurrentMode != GameModeType.Dialog)
				{
					yield break;
				}
				yield return null;
			}
			BlueprintAnswer sb = controller.Answers.Where((BlueprintAnswer a) => !Game.Instance.Player.Dialog.SelectedAnswersContains(a) && Clockwork.Instance.Scenario.CanUseAnswer(a)).Random(PFStatefulRandom.Qa);
			sb = SimpleBlueprintExtendAsObject.Or(sb, null) ?? (from a in controller.Answers
				where Clockwork.Instance.Scenario.CanUseAnswer(a)
				orderby s_AnswerTime.Get(a, 0f)
				select a).FirstOrDefault();
			if (sb == null)
			{
				Clockwork.Instance.Reporter.HandleError("No available answers!");
			}
			s_AnswerTime[sb] = Time.time;
			controller.SelectAnswer(sb);
		}
	}

	public static bool HasUntriedAnswer(BlueprintDialog dialog)
	{
		BlueprintCueBase blueprintCueBase = dialog.FirstCue.Select();
		if (blueprintCueBase == null)
		{
			PFLog.Clockwork.Warning($"Dialog {dialog} may be broken - FirstCue is null");
			return false;
		}
		Dictionary<BlueprintScriptableObject, BlueprintScriptableObject> dictionary = new Dictionary<BlueprintScriptableObject, BlueprintScriptableObject>();
		HashSet<BlueprintScriptableObject> hashSet = new HashSet<BlueprintScriptableObject>();
		Stack<BlueprintScriptableObject> stack = new Stack<BlueprintScriptableObject>();
		stack.Push(blueprintCueBase);
		while (stack.Count > 0)
		{
			BlueprintScriptableObject blueprintScriptableObject = stack.Pop();
			hashSet.Add(blueprintScriptableObject);
			if (IsUntriedAnswer(blueprintScriptableObject))
			{
				while (blueprintScriptableObject != blueprintCueBase)
				{
					blueprintScriptableObject = dictionary[blueprintScriptableObject];
				}
				return true;
			}
			List<BlueprintAnswer> nodeAnswers = GetNodeAnswers(blueprintScriptableObject);
			if (nodeAnswers.Empty() || (blueprintScriptableObject is BlueprintAnswer answer && !IsAvailableAnswer(answer)))
			{
				continue;
			}
			foreach (BlueprintAnswer item in nodeAnswers)
			{
				if (!hashSet.Contains(item))
				{
					dictionary[item] = blueprintScriptableObject;
					stack.Push(item);
				}
			}
		}
		return false;
	}

	private static List<BlueprintAnswer> GetNodeAnswers(BlueprintScriptableObject node)
	{
		List<BlueprintAnswerBaseReference> nodeNeighbours = TaskAnswerDialog.GetNodeNeighbours(node);
		List<BlueprintAnswer> list = new List<BlueprintAnswer>();
		if (nodeNeighbours.Empty())
		{
			return list;
		}
		foreach (BlueprintAnswerBaseReference item2 in nodeNeighbours)
		{
			if (item2.Get() is BlueprintAnswer item)
			{
				list.Add(item);
			}
			if (item2.Get() is BlueprintAnswersList blueprintAnswersList && blueprintAnswersList.CanSelect())
			{
				list.AddRange(blueprintAnswersList.Answers.Select((BlueprintAnswerBaseReference x) => x.Get() as BlueprintAnswer));
			}
		}
		return list;
	}

	private static bool IsAvailableAnswer(BlueprintAnswer answer)
	{
		if (answer.CanShow() && answer.CanSelect())
		{
			return Clockwork.Instance.Scenario.CanUseAnswer(answer);
		}
		return false;
	}

	private static bool IsUntriedAnswer(BlueprintScriptableObject answer)
	{
		if (answer is BlueprintAnswer blueprintAnswer && IsAvailableAnswer(blueprintAnswer))
		{
			return !Game.Instance.Player.Dialog.SelectedAnswersContains(blueprintAnswer);
		}
		return false;
	}

	public override string ToString()
	{
		return $"Talk [{Game.Instance.DialogController.Dialog}]";
	}
}
