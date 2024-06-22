using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[Serializable]
[TypeId("35c28d55db5440459de2470af3c04f76")]
public class EnsurePartyOnCommonNavMeshArea : PlayerUpgraderOnlyAction
{
	public bool UseHint;

	[SerializeField]
	[ShowIf("UseHint")]
	private BlueprintAreaEnterPointReference m_Hint;

	[CanBeNull]
	public BlueprintAreaEnterPoint Hint => m_Hint;

	public override string GetCaption()
	{
		return "Ensure party units stand on common NavMesh area";
	}

	protected override void RunActionOverride()
	{
		AreaEnterPoint areaEnterPoint = ((UseHint && Hint != null) ? AreaEnterPoint.FindAreaEnterPointOnScene(Hint) : null);
		uint? hintArea = areaEnterPoint?.transform.position.GetNearestNodeXZUnwalkable()?.Area;
		List<(uint area, List<BaseUnitEntity> units)> list = (from i in Game.Instance.Player.Party
			select (area: i.CurrentUnwalkableNode.Area, unit: i) into i
			group i.unit by i.area into i
			select (area: i.Key, units: i.ToList())).ToList();
		(uint, List<BaseUnitEntity>) tuple = list.MaxBy(((uint area, List<BaseUnitEntity> units) i) => i.units.Count);
		(uint, List<BaseUnitEntity>) tuple2 = list.FirstItem(((uint area, List<BaseUnitEntity> units) i) => i.units.HasItem((BaseUnitEntity u) => u.IsMainCharacter));
		(uint, List<BaseUnitEntity>) tuple3 = list.FirstItem(((uint area, List<BaseUnitEntity> units) i) => i.area == hintArea);
		if (hintArea.HasValue && !tuple3.Item2.Empty())
		{
			FixPartyMembersPositions(tuple3.Item1, tuple3.Item2.First().Position);
		}
		else if (!tuple2.Item2.Empty())
		{
			FixPartyMembersPositions(tuple2.Item1, tuple2.Item2.First().Position);
		}
		else if (!tuple.Item2.Empty())
		{
			FixPartyMembersPositions(tuple.Item1, tuple.Item2.First().Position);
		}
		else if (areaEnterPoint != null && hintArea.HasValue)
		{
			FixPartyMembersPositions(hintArea.Value, areaEnterPoint.transform.position);
		}
		else
		{
			PFLog.Default.Error("Failed to place units on common NavMesh area");
		}
	}

	private static void FixPartyMembersPositions(uint area, Vector3 position)
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.CurrentUnwalkableNode.Area != area)
			{
				item.Position = position;
				item.SnapToGrid();
				PFLog.Default.Log("Moved unit: {0}", item);
			}
		}
	}
}
