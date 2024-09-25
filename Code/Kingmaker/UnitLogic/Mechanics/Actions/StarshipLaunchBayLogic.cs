using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("ebc58ddfe9f3df7468c3c2c3e38d18b8")]
public class StarshipLaunchBayLogic : UnitFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public enum BayLocation
	{
		port,
		starboard
	}

	[Serializable]
	public class SingleBayInfo
	{
		[SerializeField]
		private BlueprintStarshipReference m_WingBlueprint;

		public BayLocation bayLocation;

		public float launchTime;

		public int launchCD;

		public int boardOffset;

		public BlueprintStarship WingBlueprint => m_WingBlueprint?.Get();
	}

	public class SpawnInfo
	{
		public SingleBayInfo bayInfo;

		public Vector3 position;

		public Quaternion rotation;
	}

	public int wingsPerTurn = 1;

	public int wingsTotal;

	public int wingsReloadRoundsAfterExpiration;

	public SingleBayInfo[] baysInfo;

	private StarshipUnitPartLaunchBayLogic BayLogicStore(BaseUnitEntity unit)
	{
		return unit.GetOptional<StarshipUnitPartLaunchBayLogic>();
	}

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<StarshipUnitPartLaunchBayLogic>().cooldowns = new int[baysInfo.Length];
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<StarshipUnitPartLaunchBayLogic>();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (EventInvokerExtensions.MechanicEntity != base.Owner)
		{
			return;
		}
		StarshipUnitPartLaunchBayLogic starshipUnitPartLaunchBayLogic = BayLogicStore(base.Owner);
		for (int i = 0; i < starshipUnitPartLaunchBayLogic.cooldowns.Length; i++)
		{
			starshipUnitPartLaunchBayLogic.cooldowns[i] = Math.Max(starshipUnitPartLaunchBayLogic.cooldowns[i] - 1, 0);
		}
		starshipUnitPartLaunchBayLogic.launchedThisTurn = 0;
		if (starshipUnitPartLaunchBayLogic.launchedTotal >= wingsTotal)
		{
			if (starshipUnitPartLaunchBayLogic.reloadLeft <= 0)
			{
				starshipUnitPartLaunchBayLogic.launchedTotal = 0;
			}
			else
			{
				starshipUnitPartLaunchBayLogic.reloadLeft--;
			}
		}
	}

	public (int index, SpawnInfo) GetBestBayIndex(StarshipEntity starship, Vector3 Point)
	{
		StarshipUnitPartLaunchBayLogic starshipUnitPartLaunchBayLogic = BayLogicStore(starship);
		if (starshipUnitPartLaunchBayLogic.launchedThisTurn >= wingsPerTurn || starshipUnitPartLaunchBayLogic.launchedTotal >= wingsTotal)
		{
			return (index: -1, null);
		}
		Vector3 vector3Direction = CustomGraphHelper.GetVector3Direction(starship.GetDirection());
		float num = 0f;
		int num2 = -1;
		SpawnInfo item = null;
		for (int i = 0; i < baysInfo.Length; i++)
		{
			if (starshipUnitPartLaunchBayLogic.cooldowns[i] > 0)
			{
				continue;
			}
			Vector3 vector = Quaternion.Euler(0f, (baysInfo[i].bayLocation == BayLocation.port) ? 90 : (-90), 0f) * vector3Direction;
			CustomGridNodeBase spawnNode = AbilityCustomStarshipNPCTorpedoLaunch.GetSpawnNode(starship.Position, vector, starship.SizeRect.Width, 0, baysInfo[i].boardOffset, starship.GetDirection());
			if (spawnNode != null)
			{
				Vector3 vector3Position = spawnNode.Vector3Position;
				float magnitude = (Point - vector3Position).magnitude;
				if (num2 < 0 || magnitude < num)
				{
					num = magnitude;
					num2 = i;
					item = new SpawnInfo
					{
						bayInfo = baysInfo[i],
						position = vector3Position,
						rotation = Quaternion.LookRotation(vector, Vector3.up)
					};
				}
			}
		}
		return (index: num2, item);
	}

	public void LaunchBay(StarshipEntity starship, int bayIndex)
	{
		StarshipUnitPartLaunchBayLogic starshipUnitPartLaunchBayLogic = BayLogicStore(starship);
		starshipUnitPartLaunchBayLogic.launchedThisTurn++;
		starshipUnitPartLaunchBayLogic.launchedTotal++;
		if (starshipUnitPartLaunchBayLogic.launchedTotal >= wingsTotal)
		{
			starshipUnitPartLaunchBayLogic.reloadLeft = wingsReloadRoundsAfterExpiration;
		}
		starshipUnitPartLaunchBayLogic.cooldowns[bayIndex] = baysInfo[bayIndex].launchCD;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
