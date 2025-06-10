using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5c768b244f20da64aab2fcfe4c3b796d")]
public class HideScriptZoneUnit : GameAction
{
	public bool Fade = true;

	public bool HideDeadOnly = true;

	public bool HideEnemiesOnly = true;

	public bool HidePartyMembers;

	public bool LogAction = true;

	public override string GetCaption()
	{
		return "Hide Script Zone Unit";
	}

	public override string GetDescription()
	{
		return "Скрывает юнит, который вошел в Script Zone" + (HideDeadOnly ? " (только мертвых)" : "") + (HideEnemiesOnly ? " (только врагов)" : "") + (HidePartyMembers ? " (включая членов партии)" : " (исключая членов партии)");
	}

	protected override void RunAction()
	{
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
		if (LogAction)
		{
			PFLog.Default.Log("HideScriptZoneUnit: Unit " + baseUnitEntity.CharacterName + " hidden successfully");
		}
	}
}
