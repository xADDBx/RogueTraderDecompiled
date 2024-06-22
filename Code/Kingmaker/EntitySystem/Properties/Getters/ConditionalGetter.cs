using System;
using System.Text;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("2b7102ebfff042409c7f1493b16125a9")]
public class ConditionalGetter : PropertyGetter
{
	public PropertyCalculator Condition;

	public PropertyCalculator True;

	public PropertyCalculator False;

	public override bool IsSimple => false;

	public override bool AddBracketsAroundInnerCaption => false;

	protected override int GetBaseValue()
	{
		if (!Condition.GetBoolValue(base.PropertyContext))
		{
			return False.GetValue(base.PropertyContext);
		}
		return True.GetValue(base.PropertyContext);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = (true ? "yellow" : "blue");
		bool flag = false;
		if (useLineBreaks)
		{
			flag = True.Getters.Length > 1 || (True.Getters.Length == 1 && !True.Getters[0].IsSimple);
			if (!flag)
			{
				flag = False.Getters.Length > 1 || (False.Getters.Length == 1 && !False.Getters[0].IsSimple);
			}
		}
		if (useLineBreaks && flag)
		{
			using (PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request())
			{
				StringBuilder builder = pooledStringBuilder.Builder;
				builder.Append('\n');
				if (useLineBreaks)
				{
					builder.AppendIndentedFormula("<color='" + text + "'>if</color> ");
				}
				else
				{
					builder.AppendIndentedFormula("if ");
				}
				builder.Append(Condition.GenerateDescription(useLineBreaks));
				builder.Append('\n');
				builder.Append(FormulaScope.Indent);
				if (useLineBreaks)
				{
					builder.Append("<color='" + text + "'>then</color>");
				}
				else
				{
					builder.Append("then");
				}
				if (True.IsSimple)
				{
					builder.Append('\n');
					using (FormulaScope.Enter(useLineBreaks))
					{
						builder.AppendIndentedFormula(True.GenerateDescription(useLineBreaks));
					}
					builder.Append('\n');
				}
				else
				{
					builder.Append(True.GenerateDescription(useLineBreaks));
				}
				builder.Append(FormulaScope.Indent);
				if (useLineBreaks)
				{
					builder.Append("<color='" + text + "'>else</color>");
				}
				else
				{
					builder.Append("else");
				}
				if (False.IsSimple)
				{
					builder.Append('\n');
					using (FormulaScope.Enter(useLineBreaks))
					{
						builder.AppendIndentedFormula(False.GenerateDescription(useLineBreaks));
					}
					builder.Append('\n');
				}
				else
				{
					builder.Append(False.GenerateDescription(useLineBreaks));
				}
				return builder.ToString();
			}
		}
		return "if " + Condition.GenerateDescription(useLineBreaks: false) + " then " + True.GenerateDescription(useLineBreaks: false) + " else " + False.GenerateDescription(useLineBreaks: false);
	}
}
