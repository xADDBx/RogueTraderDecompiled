using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Progression.Prerequisites;

namespace Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;

public class CalculatedPrerequisiteStat : CalculatedPrerequisite
{
	public StatType Stat { get; }

	public int MinValue { get; }

	private static CalculatedPrerequisiteStrings Strings => LocalizedTexts.Instance.CalculatedPrerequisites;

	public CalculatedPrerequisiteStat(bool value, [NotNull] PrerequisiteStat source)
		: base(value, source)
	{
		Stat = source.Stat;
		MinValue = source.MinValue;
	}

	protected override string GetDescriptionInternal()
	{
		string arg = (base.Not ? Strings.StatLessThan.Text : Strings.StatGreaterOrEqual.Text);
		return string.Format(Strings.HasStat, Stat, arg, MinValue);
	}
}
