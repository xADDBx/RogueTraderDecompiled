namespace Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;

public class CalculatedPrerequisiteSimple : CalculatedPrerequisite
{
	public static readonly CalculatedPrerequisiteSimple True = new CalculatedPrerequisiteSimple(value: true);

	public static readonly CalculatedPrerequisiteSimple False = new CalculatedPrerequisiteSimple(value: false);

	public CalculatedPrerequisiteSimple(bool value)
		: base(value)
	{
	}

	protected override string GetDescriptionInternal()
	{
		return base.Value.ToString();
	}
}
