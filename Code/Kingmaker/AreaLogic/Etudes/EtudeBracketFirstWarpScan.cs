using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("2cc0176d77ee41d7a259d53d5844c86f")]
public class EtudeBracketFirstWarpScan : EtudeBracketTrigger, ISectorMapScanHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private ActionList m_OnTriggerActions;

	public void HandleScanStarted(float range, float duration)
	{
	}

	public void HandleSectorMapObjectScanned(SectorMapPassageView passageToStarSystem)
	{
		m_OnTriggerActions?.Run();
	}

	public void HandleScanStopped()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
