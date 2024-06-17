using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/InteractCommand")]
[TypeId("4abdc22fa828c274d85188188ea2f040")]
public class InteractCommand : ClockworkCommand
{
	public enum EntityType
	{
		Unit,
		MapObject
	}

	[HideInInspector]
	public bool Done;

	public EntityType entityType = EntityType.MapObject;

	[ShowIf("IsEntityUnit")]
	public BlueprintUnitReference UnitBlueprint;

	[ShowIf("IsEntityMapObject")]
	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference MapObject;

	private Entity m_Target;

	public bool IsEntityUnit => entityType == EntityType.Unit;

	public bool IsEntityMapObject => entityType == EntityType.MapObject;

	public Entity Target
	{
		get
		{
			if (m_Target?.View != null)
			{
				return m_Target;
			}
			if (IsEntityMapObject)
			{
				m_Target = MapObject?.FindData().ToEntity();
			}
			if (IsEntityUnit)
			{
				foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
				{
					if (UnitBlueprint.Is(allUnit.Blueprint))
					{
						m_Target = allUnit;
						break;
					}
				}
			}
			return m_Target;
		}
	}

	public InteractCommand()
	{
	}

	public InteractCommand(BlueprintUnit unitBlueprint)
	{
		UnitBlueprint = unitBlueprint.ToReference<BlueprintUnitReference>();
		entityType = EntityType.Unit;
	}

	public InteractCommand(EntityReference mapObject)
	{
		MapObject = mapObject;
		entityType = EntityType.MapObject;
	}

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		if (Target == null)
		{
			string text = (IsEntityUnit ? UnitBlueprint.ToString() : MapObject?.EntityNameInEditor);
			Clockwork.Instance.Reporter.HandleError("Cannot create interaction task - object " + text + " not found in scene");
			return null;
		}
		ClockworkRunnerTask clockworkRunnerTask = AreaTaskSelector.CreateInteractionTask(runner, Target);
		clockworkRunnerTask.SetSourceCommand(this);
		return clockworkRunnerTask;
	}

	public override string GetCaption()
	{
		string text = ((!IsEntityUnit) ? MapObject?.EntityNameInEditor : UnitBlueprint?.ToString());
		return GetStatusString() + "Interact with " + text;
	}
}
