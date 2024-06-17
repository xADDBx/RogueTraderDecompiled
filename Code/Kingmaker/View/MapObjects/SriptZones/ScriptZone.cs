using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects.SriptZones;

public class ScriptZone : MapObjectView, IBlueprintedMapObjectView
{
	[Serializable]
	public class UnitEvent : UnityEvent<BaseUnitEntity, ScriptZone>
	{
	}

	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintScriptZoneReference m_Blueprint;

	[SerializeField]
	[HideInInspector]
	internal Bounds m_Bounds = new Bounds(Vector3.zero, Vector3.one * 3f);

	[SerializeField]
	[HideInInspector]
	internal Bounds[] m_MoreBounds;

	[SerializeField]
	[Tooltip("When set, zone is auto-disbled when first unit enters it.")]
	private bool m_OnceOnly;

	[SerializeField]
	[Tooltip("When set, zone ony triggers events for player-controllable charactes")]
	private bool m_PlayersOnly;

	[SerializeField]
	[Tooltip("When set, zone starts inactive. Set IsActive to true to start detecting units.")]
	private bool m_StartInactive;

	public UnitEvent OnUnitEntered;

	public UnitEvent OnUnitExited;

	public readonly List<IScriptZoneShape> Shapes = new List<IScriptZoneShape>();

	public bool PlayersOnly => m_PlayersOnly;

	public override bool CreatesDataOnLoad => true;

	public BlueprintScriptZone Blueprint => m_Blueprint?.Get();

	public new ScriptZoneEntity Data => (ScriptZoneEntity)base.Data;

	public bool OnceOnly => m_OnceOnly;

	public int Count => Data.InsideUnits.Count;

	public bool IsActive
	{
		get
		{
			return Data.IsActive;
		}
		set
		{
			Data.IsActive = value;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Blueprint != null)
		{
			ApplyBlueprint(Blueprint);
		}
		Shapes.Clear();
		Shapes.AddRange(GetComponentsInChildren<IScriptZoneShape>());
	}

	protected override void OnDrawGizmos()
	{
	}

	protected override MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new ScriptZoneEntity(this)
		{
			IsActive = !m_StartInactive
		});
	}

	public override bool SupportBlueprint(BlueprintMapObject blueprint)
	{
		if (base.SupportBlueprint(blueprint))
		{
			return blueprint is BlueprintScriptZone;
		}
		return false;
	}

	public override void ApplyBlueprint(BlueprintMapObject blueprint)
	{
		base.ApplyBlueprint(blueprint);
		m_Blueprint = blueprint.ToReference<BlueprintScriptZoneReference>();
	}

	public void OnBeforeSerialize()
	{
		if (!Application.isPlaying && Blueprint != null)
		{
			ApplyBlueprint(Blueprint);
		}
	}

	public void OnAfterDeserialize()
	{
	}
}
