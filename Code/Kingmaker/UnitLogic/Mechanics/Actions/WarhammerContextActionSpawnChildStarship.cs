using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("83b515e412962c240830984dd31893e5")]
public class WarhammerContextActionSpawnChildStarship : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintStarshipReference m_Blueprint;

	public ActionList AfterSpawn;

	[SerializeField]
	private bool ActBeforeSummoner = true;

	public BlueprintStarship Blueprint => m_Blueprint?.Get();

	public override string GetCaption()
	{
		return "Spawn " + Blueprint.name;
	}

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		BaseUnitEntity entity = SpawnStarship(Blueprint, base.Target.Point, null, maybeCaster, ActBeforeSummoner);
		using (base.Context.GetDataScope(entity.ToITargetWrapper()))
		{
			AfterSpawn.Run();
		}
		maybeCaster.GetStarshipNavigationOptional()?.UpdateReachableTiles_Blocking();
	}

	public static BaseUnitEntity SpawnStarship(BlueprintStarship Blueprint, Vector3 position, [CanBeNull] Quaternion? rotation, MechanicEntity caster, bool actBeforeSummoner = true)
	{
		GraphNode node = ObstacleAnalyzer.GetNearestNode(position).node;
		if (WarhammerBlockManager.Instance.NodeContainsAny(node))
		{
			Element.LogError(Blueprint, "Can't spawn on blocked cell");
			return null;
		}
		position = (Vector3)node.position;
		UnitEntityView unitEntityView = Blueprint.Prefab.Load();
		float radius = ((unitEntityView != null) ? unitEntityView.Corpulence : 0.5f);
		FreePlaceSelector.PlaceSpawnPlaces(1, radius, position);
		Quaternion rotation2 = rotation ?? GetRotation();
		BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(Blueprint, position, rotation2, caster.HoldingState);
		if (actBeforeSummoner)
		{
			baseUnitEntity.Initiative.LastTurn = Game.Instance.TurnController.GameRound;
		}
		PartCombatGroup combatGroupOptional = caster.GetCombatGroupOptional();
		if (combatGroupOptional != null)
		{
			baseUnitEntity.CombatGroup.Id = combatGroupOptional.Id;
		}
		PartFaction factionOptional = caster.GetFactionOptional();
		if ((object)factionOptional != null)
		{
			baseUnitEntity.Faction.Set(factionOptional.Blueprint);
		}
		baseUnitEntity.GetOrCreate<UnitPartSummonedMonster>().Init(caster);
		SetInitiative(baseUnitEntity, caster, actBeforeSummoner);
		foreach (StarshipAddFeaturesOnSummon component in caster.Facts.GetComponents<StarshipAddFeaturesOnSummon>())
		{
			component.ProcessUnit(baseUnitEntity);
		}
		return baseUnitEntity;
		Quaternion GetRotation()
		{
			return Quaternion.LookRotation(CustomGraphHelper.GetVector3Direction(CustomGraphHelper.GuessDirection(position - caster.View.ViewTransform.position)));
		}
	}

	private static void SetInitiative(MechanicEntity entity, MechanicEntity summoner, bool actBeforeSummoner)
	{
		IEnumerable<MechanicEntity> allUnits = Game.Instance.TurnController.AllUnits;
		entity.Initiative.Value = summoner.Initiative.Roll;
		IEnumerable<MechanicEntity> source = allUnits.Where((MechanicEntity i) => i != entity && Math.Abs(i.Initiative.Value - entity.Initiative.Value) < 1E-06f);
		if (actBeforeSummoner)
		{
			entity.Initiative.Order = 0;
			source.ForEach(delegate(MechanicEntity x)
			{
				x.Initiative.Order++;
			});
		}
		else
		{
			entity.Initiative.Order = source.Count();
		}
	}
}
