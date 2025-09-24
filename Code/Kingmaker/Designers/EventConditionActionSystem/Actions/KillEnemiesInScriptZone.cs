using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/KillEnemiesInScriptZone")]
[AllowMultipleComponents]
[TypeId("3d6f2d92ec4464347beed431c0f755ff")]
public class KillEnemiesInScriptZone : GameAction
{
	[SerializeReference]
	public MechanicEntityEvaluator Killer;

	[Tooltip("Works only if the Killer is set. If 0, body just falls on the ground, 1 is a standard impulse. For bigger impulse, try set it up a bit higher.")]
	public int ImpulseMultiplier = 1;

	public UnitDismemberType Dismember;

	[ShowIf("LimpsApartSelected")]
	[SerializeField]
	private DismembermentLimbsApartType m_DismemberingAnimation;

	public bool DisableBattleLog;

	public bool RemoveExp = true;

	[Tooltip("If true, kills only enemies. If false, kills all non-party units.")]
	public bool OnlyEnemies = true;

	[Tooltip("If true, includes dead units in the kill list.")]
	public bool IncludeDead;

	private bool LimpsApartSelected => Dismember == UnitDismemberType.LimbsApart;

	public override string GetDescription()
	{
		return "Убивает всех врагов в текущей script zone\n" + $"OnlyEnemies: {OnlyEnemies}\n" + $"IncludeDead: {IncludeDead}\n" + (DisableBattleLog ? "Log disabled" : "Log enabled");
	}

	protected override void RunAction()
	{
		SceneEntitiesState sceneEntitiesState = ContextData<ScriptZoneTriggerData>.Current?.State;
		if (sceneEntitiesState == null)
		{
			PFLog.Default.Error("KillEnemiesInScriptZone: No script zone context found");
			return;
		}
		ScriptZoneEntity scriptZoneEntity = sceneEntitiesState.AllEntityData.OfType<ScriptZoneEntity>().FirstOrDefault();
		if (scriptZoneEntity == null)
		{
			PFLog.Default.Error("KillEnemiesInScriptZone: Script zone entity not found");
			return;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		if (mainCharacterEntity == null)
		{
			PFLog.Default.Error("KillEnemiesInScriptZone: Player character not found");
			return;
		}
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (ScriptZoneEntity.UnitInfo item in scriptZoneEntity.InsideUnits.ToTempList())
		{
			BaseUnitEntity baseUnitEntity = item.Reference.ToBaseUnitEntity();
			if (baseUnitEntity == null || !item.IsValid || (!IncludeDead && baseUnitEntity.LifeState.IsDead) || baseUnitEntity.IsPlayerFaction || Game.Instance.Player.PartyAndPets.Contains(baseUnitEntity))
			{
				continue;
			}
			if (OnlyEnemies)
			{
				if (mainCharacterEntity.IsEnemy(baseUnitEntity))
				{
					list.Add(baseUnitEntity);
				}
			}
			else
			{
				list.Add(baseUnitEntity);
			}
		}
		MechanicEntity killer = Killer?.GetValue();
		foreach (BaseUnitEntity item2 in list)
		{
			try
			{
				if (DisableBattleLog)
				{
					item2.GetOrCreate<Kill.SilentDeathUnitPart>();
				}
				if (RemoveExp)
				{
					item2.GiveExperienceOnDeath = false;
				}
				GameHelper.KillUnit(item2, killer, ImpulseMultiplier, Dismember, LimpsApartSelected ? new DismembermentLimbsApartType?(m_DismemberingAnimation) : null);
			}
			catch (Exception ex)
			{
				PFLog.Default.Error("KillEnemiesInScriptZone: Error killing unit " + item2.CharacterName + ": " + ex.Message);
			}
		}
		if (list.Count > 0)
		{
			PFLog.Default.Log($"KillEnemiesInScriptZone: Killed {list.Count} units");
		}
	}

	public override string GetCaption()
	{
		return "Kill Enemies in Script Zone (" + (OnlyEnemies ? "enemies only" : "all non-party") + ")";
	}
}
