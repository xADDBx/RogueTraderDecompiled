using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Globalmap.SystemMap;

public class StarSystemContextData : ContextData<StarSystemContextData>
{
	private Action m_OnDispose;

	public BaseUnitEntity TargetUnit { get; private set; }

	public StarshipEntity Starship { get; private set; }

	public StarSystemObjectEntity StarSystemObject { get; private set; }

	public StarSystemContextData Setup(StarSystemObjectEntity sso, BaseUnitEntity targetUnit = null, StarshipEntity starship = null, Action onDispose = null)
	{
		TargetUnit = targetUnit;
		Starship = starship;
		StarSystemObject = sso;
		m_OnDispose = onDispose;
		return this;
	}

	protected override void Reset()
	{
		TargetUnit = null;
		Starship = null;
		m_OnDispose?.Invoke();
		m_OnDispose = null;
		StarSystemObject = null;
	}
}
