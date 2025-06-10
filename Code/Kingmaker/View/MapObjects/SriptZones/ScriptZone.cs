using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects.SriptZones;

[KnowledgeDatabaseID("166fbc22bc0f466428491ffb6056bb27")]
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

	[FormerlySerializedAs("m_DisableMultipleTriggersOnSameState")]
	[SerializeField]
	[Tooltip("When set, trigger for enter only once, until everyone leave that zone, and vice-versa")]
	private bool m_DisableMultipleTriggers;

	[SerializeField]
	[Tooltip("When set, zone ony triggers events for player-controllable charactes")]
	private bool m_PlayersOnly;

	[SerializeField]
	[Tooltip("When set, zone also detects dead units")]
	private bool m_UseDeads;

	[SerializeField]
	[Tooltip("When set, zone starts inactive. Set IsActive to true to start detecting units.")]
	private bool m_StartInactive;

	public UnitEvent OnUnitEntered;

	public UnitEvent OnUnitExited;

	public readonly List<IScriptZoneShape> Shapes = new List<IScriptZoneShape>();

	public bool UseDeads => m_UseDeads;

	public bool PlayersOnly => m_PlayersOnly;

	public override bool CreatesDataOnLoad => true;

	public BlueprintScriptZone Blueprint => m_Blueprint?.Get();

	public new ScriptZoneEntity Data => (ScriptZoneEntity)base.Data;

	public bool OnceOnly => m_OnceOnly;

	public bool DisableSameMultipleTriggers => m_DisableMultipleTriggers;

	public int Count => Data.InsideUnits.Count;

	public bool IsActive
	{
		get
		{
			return Data.IsActive;
		}
		set
		{
			bool isActive = Data.IsActive;
			Data.IsActive = value;
			if (!value && isActive)
			{
				Data.RemoveAll();
			}
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
