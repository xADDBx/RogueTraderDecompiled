using System.Text;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;

namespace Kingmaker.ElementsSystem;

public static class ElementsDescription
{
	public static string Conditions(bool extended, ConditionsChecker checker, string caption = "Conditions", int indent = 0)
	{
		if (!extended)
		{
			int num = checker.Conditions.Length;
			return $"{caption} ({num})";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendIndent(indent);
		stringBuilder.Append(caption + CheckerOperation(checker) + ":\n");
		Condition[] conditions = checker.Conditions;
		foreach (Condition element in conditions)
		{
			AppendElement(stringBuilder, element, indent + 1);
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public static string Actions(bool extended, params ActionList[] actionLists)
	{
		return Actions(extended, "Actions", 0, actionLists);
	}

	public static string Actions(bool extended, string caption, int indent, params ActionList[] actionLists)
	{
		ActionList[] array;
		if (!extended)
		{
			int num = 0;
			array = actionLists;
			foreach (ActionList actionList in array)
			{
				num += actionList.Actions.Length;
			}
			return $"{caption} ({num})";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendIndent(indent);
		stringBuilder.Append(caption + ":\n");
		array = actionLists;
		for (int i = 0; i < array.Length; i++)
		{
			GameAction[] actions = array[i].Actions;
			foreach (GameAction element in actions)
			{
				AppendElement(stringBuilder, element, indent + 1);
			}
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public static void AppendIndent(this StringBuilder sb, int indent)
	{
		for (int i = 0; i < indent; i++)
		{
			sb.Append("    ");
		}
	}

	private static void AppendElement(StringBuilder sb, Element element, int indent)
	{
		sb.AppendIndent(indent);
		sb.Append(element?.GetCaption(useLineBreaks: false) ?? "").Append('\n');
		if (element is OrAndLogic orAndLogic)
		{
			Condition[] conditions = orAndLogic.ConditionsChecker.Conditions;
			foreach (Condition element2 in conditions)
			{
				AppendElement(sb, element2, indent + 1);
			}
		}
		if (element is Conditional conditional)
		{
			sb.AppendIndent(indent);
			sb.Append("If" + CheckerOperation(conditional.ConditionsChecker) + "\n");
			Condition[] conditions = conditional.ConditionsChecker.Conditions;
			foreach (Condition element3 in conditions)
			{
				AppendElement(sb, element3, indent + 1);
			}
			sb.AppendIndent(indent);
			sb.Append("Then\n");
			GameAction[] actions = conditional.IfTrue.Actions;
			foreach (GameAction element4 in actions)
			{
				AppendElement(sb, element4, indent + 1);
			}
			if (conditional.IfFalse.Actions.Length != 0)
			{
				sb.AppendIndent(indent);
				sb.Append("Else\n");
				actions = conditional.IfFalse.Actions;
				foreach (GameAction element5 in actions)
				{
					AppendElement(sb, element5, indent + 1);
				}
			}
		}
		if (!(element is RandomAction { Actions: var actions2 }))
		{
			return;
		}
		for (int i = 0; i < actions2.Length; i++)
		{
			ActionAndWeight actionAndWeight = actions2[i];
			if (actionAndWeight.Action.HasActions)
			{
				sb.AppendIndent(indent + 1);
				sb.AppendLine($"Weight: {actionAndWeight.Weight}");
				if (actionAndWeight.Conditions.HasConditions)
				{
					sb.AppendLine(Conditions(extended: true, actionAndWeight.Conditions, "Conditions", indent + 1));
				}
				sb.AppendLine(Actions(true, "Actions", indent + 1, actionAndWeight.Action));
				sb.AppendLine();
			}
		}
	}

	private static string CheckerOperation(ConditionsChecker checker)
	{
		if (checker.Operation == Operation.And || checker.Conditions.Length <= 1)
		{
			return "";
		}
		return $" [{checker.Operation}]";
	}
}
