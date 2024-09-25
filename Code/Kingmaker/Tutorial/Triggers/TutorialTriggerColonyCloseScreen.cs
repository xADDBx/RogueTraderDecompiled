using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("7e12811a6f4a4e7d961067a0a2eb5182")]
public class TutorialTriggerColonyCloseScreen : TutorialTrigger, IExplorationUIHandler, ISubscriber, IHashable
{
	private PlanetEntity m_Planet;

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		m_Planet = explorationObjectView.EntityData as PlanetEntity;
	}

	public void CloseExplorationScreen()
	{
		if (m_Planet != null && m_Planet.IsColonized)
		{
			TryToTrigger(null);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
