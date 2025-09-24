using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.MapObjects.SriptZones;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5c768b244f20da64aab2fcfe4c3b796d")]
public class HideScriptZoneUnit : GameAction
{
	public bool Fade = true;

	public bool HideDeadOnly = true;

	public bool HideEnemiesOnly = true;

	public bool HidePartyMembers;

	public bool LogAction = true;

	[Tooltip("Если true, показывает ранее скрытые юниты вместо скрытия")]
	public bool Unhide;

	[ShowIf("Unhide")]
	[AllowedEntityType(typeof(ScriptZone))]
	[Tooltip("Script Zone, из которой нужно восстановить скрытые юниты. Если не указана, используется текущая зона из контекста")]
	public EntityReference TargetScriptZone;

	public override string GetCaption()
	{
		if (!Unhide)
		{
			return "Hide Script Zone Unit";
		}
		return "Show Script Zone Units";
	}

	public override string GetDescription()
	{
		if (Unhide)
		{
			return "Показывает ранее скрытые юниты из Script Zone" + ((TargetScriptZone != null) ? $" ({TargetScriptZone})" : " (текущей)");
		}
		return "Скрывает юнит, который вошел в Script Zone" + (HideDeadOnly ? " (только мертвых)" : "") + (HideEnemiesOnly ? " (только врагов)" : "") + (HidePartyMembers ? " (включая членов партии)" : " (исключая членов партии)");
	}

	protected override void RunAction()
	{
		if (Unhide)
		{
			UnhideUnits();
			return;
		}
		BaseUnitEntity baseUnitEntity = ContextData<ScriptZoneTriggerData>.Current?.Unit;
		if (baseUnitEntity == null)
		{
			if (LogAction)
			{
				PFLog.Default.Error("HideScriptZoneUnit: Unit is null");
			}
			return;
		}
		bool flag = baseUnitEntity.IsPlayerFaction || Game.Instance.Player.PartyAndPets.Contains(baseUnitEntity);
		if (LogAction)
		{
			PFLog.Default.Log($"HideScriptZoneUnit: Processing unit {baseUnitEntity.CharacterName}, IsDead: {baseUnitEntity.LifeState.IsDead}, IsEnemy: {baseUnitEntity.IsPlayerEnemy}, IsPartyMember: {flag}");
		}
		if (HideDeadOnly && !baseUnitEntity.LifeState.IsDead)
		{
			if (LogAction)
			{
				PFLog.Default.Log("HideScriptZoneUnit: Unit " + baseUnitEntity.CharacterName + " is not dead, skipping");
			}
			return;
		}
		if (HideEnemiesOnly && !baseUnitEntity.IsPlayerEnemy)
		{
			if (LogAction)
			{
				PFLog.Default.Log("HideScriptZoneUnit: Unit " + baseUnitEntity.CharacterName + " is not an enemy, skipping");
			}
			return;
		}
		if (!HidePartyMembers && flag)
		{
			if (LogAction)
			{
				PFLog.Default.Log("HideScriptZoneUnit: Unit " + baseUnitEntity.CharacterName + " is a party member, skipping");
			}
			return;
		}
		if (LogAction)
		{
			PFLog.Default.Log("HideScriptZoneUnit: Hiding unit " + baseUnitEntity.CharacterName);
		}
		UnitEntityView view = baseUnitEntity.View;
		if (Fade && view?.Fader != null)
		{
			baseUnitEntity.Commands.InterruptAllInterruptible();
			view.FadeHide();
			if (view.MovementAgent?.Blocker != null)
			{
				view.MovementAgent.Blocker.Unblock();
			}
		}
		baseUnitEntity.IsInGame = false;
		SceneEntitiesState sceneEntitiesState = ContextData<ScriptZoneTriggerData>.Current?.State;
		if (sceneEntitiesState != null)
		{
			ScriptZoneEntity scriptZoneEntity = sceneEntitiesState.AllEntityData.OfType<ScriptZoneEntity>().FirstOrDefault();
			if (scriptZoneEntity != null)
			{
				scriptZoneEntity.AddHiddenUnit(baseUnitEntity);
				if (LogAction)
				{
					PFLog.Default.Log("HideScriptZoneUnit: Added unit " + baseUnitEntity.CharacterName + " to zone's hidden list");
				}
			}
			else if (LogAction)
			{
				PFLog.Default.Warning("HideScriptZoneUnit: Could not find script zone entity to store hidden unit");
			}
		}
		if (LogAction)
		{
			PFLog.Default.Log("HideScriptZoneUnit: Unit " + baseUnitEntity.CharacterName + " hidden successfully");
		}
	}

	private void UnhideUnits()
	{
		ScriptZoneEntity scriptZoneEntity = null;
		scriptZoneEntity = ((TargetScriptZone == null) ? ContextData<ScriptZoneTriggerData>.Current?.State?.AllEntityData.OfType<ScriptZoneEntity>().FirstOrDefault() : (TargetScriptZone.FindData() as ScriptZoneEntity));
		if (scriptZoneEntity == null)
		{
			if (LogAction)
			{
				PFLog.Default.Error("HideScriptZoneUnit: Cannot find script zone for unhiding units");
			}
			return;
		}
		List<BaseUnitEntity> hiddenUnits = scriptZoneEntity.GetHiddenUnits();
		if (hiddenUnits.Count == 0)
		{
			if (LogAction)
			{
				PFLog.Default.Log("HideScriptZoneUnit: No hidden units to restore");
			}
			return;
		}
		if (LogAction)
		{
			PFLog.Default.Log($"HideScriptZoneUnit: Found {hiddenUnits.Count} hidden units to restore");
		}
		int num = 0;
		foreach (BaseUnitEntity item in hiddenUnits.ToList())
		{
			if (LogAction)
			{
				PFLog.Default.Log($"HideScriptZoneUnit: Attempting to restore unit {item.CharacterName} (ID: {item.UniqueId}), IsDead: {item.LifeState.IsDead}, IsInGame: {item.IsInGame}");
			}
			UnitEntityView view = item.View;
			if (view != null)
			{
				if (LogAction)
				{
					PFLog.Default.Log("HideScriptZoneUnit: Unit " + item.CharacterName + " has view, calling UnFade and setting IsInGame=true");
				}
				view.UnFade();
				if (item.IsInGame)
				{
					view.UpdateViewActive();
				}
				else
				{
					item.IsInGame = true;
				}
			}
			else
			{
				if (LogAction)
				{
					PFLog.Default.Log("HideScriptZoneUnit: Unit " + item.CharacterName + " has no view, only setting IsInGame=true");
				}
				item.IsInGame = true;
			}
			num++;
			scriptZoneEntity.RemoveHiddenUnit(item);
		}
		if (LogAction)
		{
			PFLog.Default.Log($"HideScriptZoneUnit: Restored {num} units");
		}
	}
}
