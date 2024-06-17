using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Logging;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class AreaEffectView : MechanicEntityView
{
	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_Blueprint;

	[SerializeField]
	private BlueprintUnitReference m_Caster;

	[CanBeNull]
	private MechanicsContext m_Context;

	private TargetWrapper m_Target;

	private TimeSpan m_CreationTime;

	private TimeSpan? m_Duration;

	private GameObject m_SpawnedFx;

	private NavmeshCut m_NavmeshCut;

	[CanBeNull]
	private CustomGridNodeBase m_TargetNode;

	public bool HasSpawnedFx => m_SpawnedFx != null;

	public IScriptZoneShape Shape { get; private set; }

	public bool OnUnit { get; set; }

	public MechanicsContext Context => m_Context;

	public new AreaEffectEntity Data => (AreaEffectEntity)base.Data;

	public void InitAtRuntime([NotNull] MechanicsContext context, [NotNull] BlueprintAbilityAreaEffect blueprint, [NotNull] TargetWrapper target, TimeSpan creationTime, TimeSpan? duration)
	{
		m_Blueprint = blueprint.ToReference<BlueprintAbilityAreaEffectReference>();
		m_Context = context;
		m_Target = target;
		m_CreationTime = creationTime;
		m_Duration = duration;
		base.name = $"Area effect ({blueprint})";
		base.ViewTransform.position = (OnUnit ? target.Point : target.NearestNode.Vector3Position);
		IScriptZoneShape shape;
		if (!blueprint.IsAllArea)
		{
			IScriptZoneShape scriptZoneShape = base.gameObject.AddComponent<ScriptZonePattern>();
			shape = scriptZoneShape;
		}
		else
		{
			IScriptZoneShape scriptZoneShape = base.gameObject.AddComponent<ScriptZoneAllArea>();
			shape = scriptZoneShape;
		}
		Shape = shape;
		new GameObject("Locator_GroundFX").transform.SetParent(base.ViewTransform, worldPositionStays: false);
		SpawnFxs();
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		UpdatePatternIfNecessary(Data.Blueprint);
	}

	public void UpdatePatternIfNecessary(BlueprintAbilityAreaEffect blueprint)
	{
		if (!Game.Instance.CurrentlyLoadedArea.IsNavmeshArea || m_Context == null || m_Target == null || !(Shape is ScriptZonePattern scriptZonePattern))
		{
			return;
		}
		CustomGridNodeBase targetNode = m_TargetNode;
		m_TargetNode = m_Target.NearestNode;
		if (scriptZonePattern.ApplicationNodeExists && targetNode == m_TargetNode)
		{
			return;
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = (m_Context.MaybeCaster?.Position ?? m_Target.Point).GetNearestNodeXZUnwalkable();
		MechanicEntity caster;
		if (OnUnit)
		{
			MechanicEntity entity = m_Target.Entity;
			if (entity != null)
			{
				caster = entity;
				goto IL_00ae;
			}
		}
		caster = base.EntityData;
		goto IL_00ae;
		IL_00ae:
		CustomGridNodeBase actualCastNode;
		OrientedPatternData pattern = AoEPatternHelper.GetOrientedPattern(null, caster, blueprint.Pattern, blueprint, nearestNodeXZUnwalkable, m_TargetNode, castOnSameLevel: false, directional: false, coveredTargetsOnly: false, out actualCastNode);
		scriptZonePattern.SetPattern(actualCastNode, actualCastNode.Vector3Position.y, in pattern);
	}

	public bool Contains(BaseUnitEntity unit)
	{
		return Shape.Contains(unit.Position, unit.SizeRect);
	}

	public bool Contains(Vector3 point, IntRect size = default(IntRect))
	{
		return Shape.Contains(point, size);
	}

	public bool Contains(CustomGridNodeBase node, IntRect size = default(IntRect))
	{
		return Shape.Contains(node, size);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_NavmeshCut != null)
		{
			UnityEngine.Object.Destroy(m_NavmeshCut);
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		if ((object)m_Target == null)
		{
			m_Target = new TargetWrapper(base.ViewTransform.position);
		}
		return Entity.Initialize(new AreaEffectEntity(this, m_Context, m_Blueprint.Get(), m_Target, m_CreationTime, m_Duration, OnUnit));
	}

	public void SpawnFxs()
	{
		GameObject gameObject = m_Blueprint.Get().Fx.Load();
		if (!(gameObject != null) || (bool)m_SpawnedFx)
		{
			return;
		}
		if (OnUnit)
		{
			MechanicEntityView mechanicEntityView = m_Target.Entity?.View;
			if (mechanicEntityView == null)
			{
				LogChannel.Default.Error("Missing target unit view reference during FX spawn. m_Target " + m_Target.EntityRef.Id + ". AreaEffectView " + UniqueId);
			}
			else
			{
				m_SpawnedFx = FxHelper.SpawnFxOnEntity(gameObject, mechanicEntityView);
			}
		}
		else
		{
			m_SpawnedFx = FxHelper.SpawnFxOnGameObject(gameObject, base.gameObject);
		}
	}

	public void RemoveFxs()
	{
		if (m_SpawnedFx != null)
		{
			FxHelper.Destroy(m_SpawnedFx);
			m_SpawnedFx = null;
		}
		if (!(m_Blueprint.Get().FxOnEndAreaEffect == null))
		{
			GameObject gameObject = m_Blueprint.Get().FxOnEndAreaEffect.Load();
			if (gameObject != null)
			{
				FxHelper.SpawnFxOnGameObject(gameObject, base.gameObject);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Shape = GetComponent<IScriptZoneShape>();
	}

	private void LateUpdate()
	{
		if (OnUnit)
		{
			base.ViewTransform.position = m_Target.Point;
		}
		else if (m_SpawnedFx != null)
		{
			m_SpawnedFx.transform.position = base.ViewTransform.position;
		}
	}

	protected override void OnDestroy()
	{
		RemoveFxs();
		base.OnDestroy();
	}
}
