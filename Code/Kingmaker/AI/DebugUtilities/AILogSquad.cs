using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.AI.DebugUtilities;

public class AILogSquad : AILogObject
{
	private readonly MechanicEntity unit;

	public AILogSquad(MechanicEntity unit)
	{
		this.unit = unit;
	}

	public override string GetLogString()
	{
		return $"Current squad unit: {unit}";
	}
}
