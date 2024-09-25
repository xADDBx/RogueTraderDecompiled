using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.ActivatableAbilities;

[JsonObject]
public class ActivatableAbilityCollection : MechanicEntityFactsCollection<ActivatableAbility>
{
	[JsonConstructor]
	public ActivatableAbilityCollection()
	{
	}

	public IEnumerator<ActivatableAbility> GetEnumerator()
	{
		return base.Enumerable.GetEnumerator();
	}

	protected override ActivatableAbility PrepareFactForAttach(ActivatableAbility fact)
	{
		return fact;
	}

	protected override ActivatableAbility PrepareFactForDetach(ActivatableAbility fact)
	{
		return fact;
	}

	protected override void OnFactDidAttach(ActivatableAbility fact)
	{
		base.OnFactDidAttach(fact);
		if (fact.Blueprint.IsOnByDefault)
		{
			fact.IsOn = true;
		}
	}
}
