using Code.GameCore.Mics;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class FactData : ContextData<FactData>
{
	public IEntityFact Fact { get; private set; }

	public FactData Setup(IEntityFact fact)
	{
		Fact = fact;
		return this;
	}

	protected override void Reset()
	{
		Fact = null;
	}
}
