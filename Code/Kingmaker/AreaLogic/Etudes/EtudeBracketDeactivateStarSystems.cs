using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("314915f554a44185965ce69e8045ef71")]
public class EtudeBracketDeactivateStarSystems : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	private List<BlueprintSectorMapPointReference> m_StarSystems;

	protected override void OnEnter()
	{
		SetAvailability(availability: false);
	}

	protected override void OnExit()
	{
		SetAvailability(availability: true);
	}

	protected override void OnResume()
	{
		SetAvailability(availability: false);
	}

	private void SetAvailability(bool availability)
	{
		IEnumerable<BlueprintSectorMapPoint> source = m_StarSystems?.Dereference().EmptyIfNull();
		if (source.Empty())
		{
			return;
		}
		foreach (SectorMapObjectEntity item in Game.Instance.State.SectorMapObjects.All)
		{
			if (source.Contains(item.Blueprint))
			{
				item.SetAvailability(availability);
			}
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
