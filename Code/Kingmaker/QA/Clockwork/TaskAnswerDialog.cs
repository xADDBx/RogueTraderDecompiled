using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.QA.Clockwork;

internal class TaskAnswerDialog : ClockworkRunnerTask
{
	private BlueprintAnswer m_Answer;

	public TaskAnswerDialog(ClockworkRunner runner, BlueprintAnswer Answer)
		: base(runner)
	{
		m_Answer = Answer;
	}

	protected override IEnumerator Routine()
	{
		yield return new TaskWaitForDialog(Runner);
		DialogController controller = Game.Instance.DialogController;
		yield return 1f;
		while (!controller.Answers.Any())
		{
			if (Game.Instance.CurrentMode != GameModeType.Dialog)
			{
				yield break;
			}
			yield return null;
			controller = Game.Instance.DialogController;
		}
		yield return new TaskAutoClickSystemDialog(Runner);
		if (m_Answer.IsSystem())
		{
			yield break;
		}
		if (!controller.Answers.Contains(m_Answer))
		{
			Clockwork.Instance.Reporter.HandleWarning("No answer in current dialog state, trying to find path though dialog.");
			BlueprintAnswer firstAnswerInPath = GetFirstAnswerInPath(m_Answer);
			while (firstAnswerInPath != null && firstAnswerInPath != m_Answer && controller.Answers.Contains(firstAnswerInPath))
			{
				controller.SelectAnswer(firstAnswerInPath);
				yield return null;
				yield return 1f;
				firstAnswerInPath = GetFirstAnswerInPath(m_Answer);
			}
		}
		if (controller.Answers.Contains(m_Answer))
		{
			controller.SelectAnswer(m_Answer);
			yield return null;
			yield return 1f;
		}
		else
		{
			float num = Clockwork.Instance.Scenario.TaskTimeout + 1f;
			Clockwork.Instance.Reporter.HandleError($"Cannot select answer <{m_Answer}> - not found in current dialog!");
			yield return num;
		}
		yield return new TaskAutoClickSystemDialog(Runner);
	}

	private BlueprintAnswer GetFirstAnswerInPath(BlueprintAnswer answerToFind)
	{
		DialogController dialogController = Game.Instance.DialogController;
		BlueprintAnswer result = null;
		int num = -1;
		foreach (BlueprintAnswer answer in dialogController.Answers)
		{
			int distanceToNode = GetDistanceToNode(answer, answerToFind);
			if (num == -1 || (distanceToNode != -1 && distanceToNode < num))
			{
				num = distanceToNode;
				result = answer;
			}
		}
		return result;
	}

	private int GetDistanceToNode(BlueprintAnswer rootNode, BlueprintAnswer answerToFindNode)
	{
		int num = 0;
		Dictionary<BlueprintScriptableObject, int> dictionary = new Dictionary<BlueprintScriptableObject, int> { { rootNode, num } };
		HashSet<BlueprintScriptableObject> hashSet = new HashSet<BlueprintScriptableObject>();
		Stack<BlueprintScriptableObject> stack = new Stack<BlueprintScriptableObject>();
		stack.Push(rootNode);
		while (stack.Count > 0)
		{
			BlueprintScriptableObject blueprintScriptableObject = stack.Pop();
			hashSet.Add(blueprintScriptableObject);
			if (blueprintScriptableObject as BlueprintAnswer == answerToFindNode)
			{
				return dictionary[blueprintScriptableObject];
			}
			List<BlueprintAnswerBaseReference> nodeNeighbours = GetNodeNeighbours(blueprintScriptableObject);
			if (nodeNeighbours.Empty())
			{
				continue;
			}
			num++;
			foreach (BlueprintAnswerBaseReference item in nodeNeighbours)
			{
				if (!hashSet.Contains((BlueprintAnswerBase)item))
				{
					stack.Push((BlueprintAnswerBase)item);
					if (!dictionary.ContainsKey((BlueprintAnswerBase)item))
					{
						dictionary[(BlueprintAnswerBase)item] = num;
					}
				}
			}
		}
		return -1;
	}

	public static List<BlueprintAnswerBaseReference> GetNodeNeighbours(BlueprintScriptableObject currentNode)
	{
		if (currentNode is BlueprintCue blueprintCue)
		{
			if (!blueprintCue.Answers.Empty())
			{
				return blueprintCue.Answers;
			}
			if (!blueprintCue.Continue.Cues.Empty())
			{
				return GetNodeNeighbours(blueprintCue.Continue.Select());
			}
		}
		if (currentNode is BlueprintCheck blueprintCheck)
		{
			return GetNodeNeighbours(blueprintCheck.Fail).Concat(GetNodeNeighbours(blueprintCheck.Success)).ToList();
		}
		if (currentNode is BlueprintBookPage blueprintBookPage)
		{
			return blueprintBookPage.Answers;
		}
		if (currentNode is BlueprintCueSequence blueprintCueSequence)
		{
			IEnumerable<List<BlueprintAnswerBaseReference>> source = from x in blueprintCueSequence.Cues
				where x.Get().CanShow()
				select GetNodeNeighbours((BlueprintCueBase)x);
			if (!source.Empty())
			{
				return source.Aggregate((List<BlueprintAnswerBaseReference> a, List<BlueprintAnswerBaseReference> b) => a.Concat(b).ToList());
			}
		}
		if (currentNode is BlueprintAnswer blueprintAnswer)
		{
			return GetNodeNeighbours(blueprintAnswer.NextCue.Select());
		}
		if (currentNode is BlueprintAnswersList blueprintAnswersList)
		{
			return blueprintAnswersList.Answers;
		}
		return new List<BlueprintAnswerBaseReference>();
	}

	public override string ToString()
	{
		string arg = m_Answer.DisplayText.Substring(0, Math.Min(m_Answer.DisplayText.Length, 30));
		return $"Select answer <{arg}> {m_Answer}";
	}
}
