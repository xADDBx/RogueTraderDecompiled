using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Progression.Prerequisites;

namespace Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;

public class CalculatedPrerequisiteLevel : CalculatedPrerequisite
{
	public readonly int Level;

	private static CalculatedPrerequisiteStrings Strings => LocalizedTexts.Instance.CalculatedPrerequisites;

	public CalculatedPrerequisiteLevel(bool value, [NotNull] PrerequisiteLevel source)
		: base(value, source)
	{
		Level = source.Level;
	}

	protected override string GetDescriptionInternal()
	{
		int level = Level;
		return level.ToString();
	}
}
