using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

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

	public SkillCheckDifficulty AwarenessDifficulty;

	[SerializeField]
	[HideIf("AwarenessDifficultyIsCustom")]
	public int AwarenessDC = 15;

	public float AwarenessRadius = 7f;

	public int DisableDC = 25;

	public int DisableTriggerMargin = 5;

	public bool IsHiddenWhenInactive = true;

	public bool AllowedForRandomEncounters;

	public UnitAnimationInteractionType DisarmAnimation = UnitAnimationInteractionType.DisarmTrap;

	public ConditionsChecker TriggerConditions;

	public ConditionsChecker DisableConditions;

	public ActionList TrapActions;

	public ActionList DisableActions;

	public StatType DisarmSkill = StatType.SkillDemolition;

	private bool AwarenessDifficultyIsCustom => AwarenessDifficulty == SkillCheckDifficulty.Custom;

	protected override Type GetFactType()
	{
		return null;
	}
}
