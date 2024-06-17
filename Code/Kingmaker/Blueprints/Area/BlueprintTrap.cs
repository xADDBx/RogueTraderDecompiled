using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Area;

[TypeId("bfc5b72ffecf0044b9bf958a6c027901")]
public class BlueprintTrap : BlueprintMapObject
{
	public class ElementsData : ContextData<ElementsData>
	{
		public BaseUnitEntity TriggeringUnit { get; private set; }

		public TrapObjectView TrapObject { get; private set; }

		public ElementsData Setup(BaseUnitEntity unit, TrapObjectView obj)
		{
			TriggeringUnit = unit;
			TrapObject = obj;
			return this;
		}

		protected override void Reset()
		{
			TriggeringUnit = null;
			TrapObject = null;
		}
	}

	public int AwarenessDC = 15;

	public float AwarenessRadius = 7f;

	public int DisableDC = 25;

	public int DisableTriggerMargin = 5;

	public bool IsHiddenWhenInactive = true;

	public bool AllowedForRandomEncounters;

	public UnitAnimationInteractionType DisarmAnimation = UnitAnimationInteractionType.DisarmTrap;

	[SerializeField]
	[FormerlySerializedAs("Actor")]
	private BlueprintUnitReference m_Actor;

	public ConditionsChecker TriggerConditions;

	public ConditionsChecker DisableConditions;

	public ActionList TrapActions;

	public ActionList DisableActions;

	public StatType DisarmSkill = StatType.SkillDemolition;

	public BlueprintUnit Actor => m_Actor?.Get();

	protected override Type GetFactType()
	{
		return null;
	}
}
