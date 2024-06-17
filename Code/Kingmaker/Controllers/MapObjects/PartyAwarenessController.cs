using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.Controllers.MapObjects;

public class PartyAwarenessController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (!partyAndPet.Movable.HasMotionThisSimulationTick)
			{
				continue;
			}
			foreach (MapObjectEntity mapObject in Game.Instance.State.MapObjects)
			{
				if (mapObject.IsInFogOfWar)
				{
					continue;
				}
				float num = partyAndPet.DistanceTo(mapObject.View.ViewTransform.position);
				if (!mapObject.IsAwarenessCheckPassed && mapObject.View.AwarenessCheckComponent == null)
				{
					mapObject.IsAwarenessCheckPassed = true;
					continue;
				}
				bool isAwarenessCheckPassed = mapObject.IsAwarenessCheckPassed;
				if (mapObject.IsAwarenessCheckPassed)
				{
					if (!mapObject.WasHighlightedOnReveal && num < (float)BlueprintRoot.Instance.StandartPerceptionRadius && !Game.Instance.Player.IsInCombat && mapObject.Interactions.Any(IsInteractionForForceHighlight))
					{
						mapObject.View.ForceHighlightOnReveal(mapObject.View.AwarenessCheckComponent != null);
					}
				}
				else if (mapObject.IsAwarenessRollAllowed(partyAndPet) && num < mapObject.View.AwarenessCheckComponent.Radius && partyAndPet.Vision.HasLOS(mapObject.View))
				{
					RollAwareness(partyAndPet, mapObject);
				}
				if (!isAwarenessCheckPassed && mapObject.IsAwarenessCheckPassed && mapObject.View is TrapObjectView trapObjectView)
				{
					if (trapObjectView.LinkedTrap != null)
					{
						trapObjectView.LinkedTrap.Data.IsAwarenessCheckPassed = true;
					}
					if (trapObjectView.Device != null)
					{
						trapObjectView.Device.Data.IsAwarenessCheckPassed = true;
					}
				}
			}
		}
	}

	private static bool IsInteractionForForceHighlight(InteractionPart i)
	{
		if (i is InteractionLootPart)
		{
			return i.Enabled;
		}
		return false;
	}

	private static void RollAwareness(BaseUnitEntity character, MapObjectEntity data)
	{
		int dC = data.View.AwarenessCheckComponent.DC;
		RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(character, StatType.SkillAwareness, dC)
		{
			Reason = data
		});
		data.LastAwarenessRollRank[character.FromBaseUnitEntity()] = character.Skills.SkillAwareness.BaseValue;
		data.IsAwarenessCheckPassed = rulePerformSkillCheck.ResultIsSuccess;
		if (rulePerformSkillCheck.ResultIsSuccess)
		{
			EventBus.RaiseEvent((IMapObjectEntity)data, (Action<IAwarenessHandler>)delegate(IAwarenessHandler h)
			{
				h.OnEntityNoticed(character);
			}, isCheckRuntime: true);
		}
		else if (BuildModeUtility.IsDevelopment)
		{
			UIUtility.SendWarning($"Perception failed on {data}");
		}
	}
}
