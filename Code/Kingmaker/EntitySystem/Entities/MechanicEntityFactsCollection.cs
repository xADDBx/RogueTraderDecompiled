using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.EntitySystem.Entities;

public abstract class MechanicEntityFactsCollection<TFact> : EntityFactsProcessor<TFact> where TFact : MechanicEntityFact
{
	public List<TFact> Enumerable => base.RawFacts;

	public IEnumerable<TFact> Visible => base.RawFacts.Where((TFact fact) => !fact.Hidden);

	protected override void OnFactDidAttach(TFact fact)
	{
		EventBus.RaiseEvent((IMechanicEntity)(MechanicEntity)base.Manager.Owner, (Action<IEntityGainFactHandler>)delegate(IEntityGainFactHandler h)
		{
			h.HandleEntityGainFact(fact);
		}, isCheckRuntime: true);
	}

	protected override void OnFactWillDetach(TFact fact)
	{
	}

	protected override void OnFactDidDetached(TFact fact)
	{
		EventBus.RaiseEvent((IMechanicEntity)(MechanicEntity)base.Manager.Owner, (Action<IEntityLostFactHandler>)delegate(IEntityLostFactHandler h)
		{
			h.HandleEntityLostFact(fact);
		}, isCheckRuntime: true);
	}
}
