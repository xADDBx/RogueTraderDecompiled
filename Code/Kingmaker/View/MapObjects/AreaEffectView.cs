using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Logging;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[KnowledgeDatabaseID("6c9e918a566788343b39a46a9e3f3a1c")]
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

	private OverrideAreaEffectPatternData? m_OverridePatternData;

	private GameObject m_SpawnedFx;

	private NavmeshCut m_NavmeshCut;

	[CanBeNull]
	private CustomGridNodeBase m_TargetNode;

	public bool HasSpawnedFx => m_SpawnedFx != null;

	public IScriptZoneShape Shape { get; private set; }

	public bool OnUnit { get; set; }

	public MechanicsContext Context => m_Context;

	public new AreaEffectEntity Data => (AreaEffectEntity)base.Data;

	public void InitAtRuntime([NotNull] MechanicsContext context, [NotNull] BlueprintAbilityAreaEffect blueprint, [NotNull] TargetWrapper target, TimeSpan creationTime, TimeSpan? duration, OverrideAreaEffectPatternData? overridenPatternData = null, bool getOrientationFromCaster = false)
	{
		m_Blueprint = blueprint.ToReference<BlueprintAbilityAreaEffectReference>();
		m_Context = context;
		m_Target = target;
		m_CreationTime = creationTime;
		m_Duration = duration;
		m_OverridePatternData = overridenPatternData;
		base.name = $"Area effect ({blueprint})";
		base.ViewTransform.position = (OnUnit ? target.Point : target.NearestNode.Vector3Position);
		if (context.MaybeCaster != null && getOrientationFromCaster)
		{
			base.ViewTransform.rotation = Quaternion.Euler(0f, context.MaybeCaster.Orientation, 0f);
		}
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
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		UpdatePatternIfNecessary(Data.Blueprint);
		SpawnFxs();
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
		OrientedPatternData pattern = AoEPatternHelper.GetOrientedPattern(null, caster, blueprint.Pattern, blueprint, nearestNodeXZUnwalkable, m_TargetNode, castOnSameLevel: false, blueprint.Pattern.CanBeDirectional, coveredTargetsOnly: false, out actualCastNode);
		actualCastNode = pattern.ApplicationNode ?? actualCastNode;
		if (m_OverridePatternData.HasValue)
		{
			actualCastNode = m_OverridePatternData.Value.Pattern.ApplicationNode ?? actualCastNode;
			if (m_OverridePatternData.Value.OverridePatternWithAttackPattern)
			{
				CustomGridNodeBase appliedNode = actualCastNode;
				float y = actualCastNode.Vector3Position.y;
				OrientedPatternData pattern2 = m_OverridePatternData.Value.Pattern;
				scriptZonePattern.SetPattern(appliedNode, y, in pattern2);
				return;
			}
			if (m_TargetNode == null)
			{
				throw new Exception("AreaEffectView[" + base.name + "]: m_TargetNode is null");
			}
			pattern = SetupAreaEffectPatternNotFromPatternCenter(blueprint, base.EntityData, nearestNodeXZUnwalkable, actualCastNode, m_TargetNode);
		}
		scriptZonePattern.SetPattern(actualCastNode, actualCastNode.Vector3Position.y, in pattern);
		AdjustPositionFromPattern(actualCastNode);
	}

	private void AdjustPositionFromPattern(CustomGridNodeBase applicationNode)
	{
		base.ViewTransform.position = applicationNode.Vector3Position;
	}

	private static OrientedPatternData SetupAreaEffectPatternNotFromPatternCenter(BlueprintAbilityAreaEffect blueprint, MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase applicationNode, [NotNull] CustomGridNodeBase targetNode)
	{
		CustomGridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(casterNode, targetNode.Vector3Position);
		CustomGridNodeBase outerNodeNearestToTarget = caster.GetOuterNodeNearestToTarget(casterNode, targetNode.Vector3Position);
		using (ProfileScope.New("GetOriented from AreaEffectView"))
		{
			Vector3 castDirection = AoEPattern.GetCastDirection(blueprint.Pattern.Type, innerNodeNearestToTarget, applicationNode, targetNode);
			return blueprint.Pattern.GetOriented(innerNodeNearestToTarget, outerNodeNearestToTarget, castDirection, ((IAbilityAoEPatternProvider)blueprint).IsIgnoreLos, ((IAbilityAoEPatternProvider)blueprint).IsIgnoreLevelDifference, isDirectional: true, coveredTargetsOnly: false, blueprint.UseMeleeLos);
		}
	}

	public bool Contains(BaseUnitEntity unit)
	{
		return Shape.Contains(unit.Position, unit.SizeRect, unit.Forward);
	}

	public bool Contains(Vector3 point, IntRect size = default(IntRect))
	{
		return Shape.Contains(point, size, Vector3.forward);
	}

	public bool Contains(CustomGridNodeBase node, IntRect size = default(IntRect))
	{
		return Shape.Contains(node, size, Vector3.forward);
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
