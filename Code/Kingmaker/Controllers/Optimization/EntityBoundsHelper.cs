using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.Mechanics.ScriptZones;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public static class EntityBoundsHelper
{
	private static readonly Collider2D[] OverlapResults = new Collider2D[1024];

	private static PhysicsScene2D PhysicsScene => Game.Instance.EntityBoundsController.PhysicsScene;

	public static void ClearOverlapResults()
	{
		Array.Clear(OverlapResults, 0, OverlapResults.Length);
	}

	public static int OverlapCircle(Vector2 position, float radius, Collider2D[] results, int layerMask)
	{
		using (Counters.PhysicsOverlap?.Measure())
		{
			ContactFilter2D contactFilter2D = default(ContactFilter2D);
			contactFilter2D.useTriggers = true;
			contactFilter2D.useDepth = false;
			contactFilter2D.useLayerMask = true;
			contactFilter2D.useNormalAngle = false;
			contactFilter2D.useOutsideDepth = false;
			contactFilter2D.useOutsideNormalAngle = false;
			contactFilter2D.layerMask = layerMask;
			ContactFilter2D contactFilter = contactFilter2D;
			return PhysicsScene.OverlapCircle(position, radius, contactFilter, results);
		}
	}

	public static int OverlapBox(Vector2 center, Vector2 size, float yaw, Collider2D[] results, int layerMask)
	{
		using (Counters.PhysicsOverlap?.Measure())
		{
			ContactFilter2D contactFilter2D = default(ContactFilter2D);
			contactFilter2D.useTriggers = true;
			contactFilter2D.useDepth = false;
			contactFilter2D.useLayerMask = true;
			contactFilter2D.useNormalAngle = false;
			contactFilter2D.useOutsideDepth = false;
			contactFilter2D.useOutsideNormalAngle = false;
			contactFilter2D.layerMask = layerMask;
			ContactFilter2D contactFilter = contactFilter2D;
			return PhysicsScene.OverlapBox(center, size, yaw, contactFilter, results);
		}
	}

	private static List<TData> GetDataList<TData>(Collider2D[] colliders, int count, Comparison<TData> comparison = null) where TData : Entity
	{
		using (ProfileScope.New("GetDataList"))
		{
			using (Counters.PhysicsOverlap?.Measure())
			{
				List<TData> list = TempList.Get<TData>();
				for (int i = 0; i < count; i++)
				{
					if (EntityDataLink.GetEntity(colliders[i]) is TData item)
					{
						list.Add(item);
					}
				}
				if (comparison != null)
				{
					list.Sort(comparison);
				}
				return list;
			}
		}
	}

	public static List<Entity> FindEntitiesInRange(Vector3 origin, float radius, Comparison<Entity> comparison = null)
	{
		using (ProfileScope.New("EntityBoundsHelper.FindEntitiesInRange"))
		{
			int count = OverlapCircle(origin.To2D(), radius, OverlapResults, 16777216);
			return GetDataList(OverlapResults, count, comparison);
		}
	}

	public static List<BaseUnitEntity> FindUnitsInRange(Vector3 origin, float radius)
	{
		using (ProfileScope.New("EntityBoundsHelper.FindUnitsInRange"))
		{
			int count = OverlapCircle(origin.To2D(), radius, OverlapResults, 16777216);
			return GetDataList<BaseUnitEntity>(OverlapResults, count);
		}
	}

	[CanBeNull]
	public static List<BaseUnitEntity> FindUnitsInShape(IScriptZoneShape shape)
	{
		using (ProfileScope.New("EntityBoundsHelper.FindUnitsInShape"))
		{
			int count;
			if (shape is ScriptZoneCylinder scriptZoneCylinder)
			{
				Vector3 lossyScale = scriptZoneCylinder.transform.lossyScale;
				count = OverlapCircle(((IScriptZoneShape)scriptZoneCylinder).Center().To2D(), (float)scriptZoneCylinder.Radius * lossyScale.x, OverlapResults, 16777216);
			}
			else
			{
				if (!(shape is ScriptZoneBox scriptZoneBox))
				{
					return null;
				}
				Vector2 center = ((IScriptZoneShape)scriptZoneBox).Center().To2D();
				Vector2 size = scriptZoneBox.Bounds.size.To2D();
				size.Scale(scriptZoneBox.transform.lossyScale.To2D());
				count = OverlapBox(center, size, 0f - scriptZoneBox.transform.rotation.eulerAngles.y, OverlapResults, 16777216);
			}
			return GetDataList(OverlapResults, count, (Comparison<BaseUnitEntity>)MechanicEntityHelper.ByIdComparison);
		}
	}
}
