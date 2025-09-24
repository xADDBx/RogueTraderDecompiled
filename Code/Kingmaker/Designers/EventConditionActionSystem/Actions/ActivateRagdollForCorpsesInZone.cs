using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.Visual.CharactersRigidbody;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ActivateRagdollForCorpsesInZone")]
[AllowMultipleComponents]
[TypeId("616ddb70be13e30438ed9c46aa45e1d1")]
[PlayerUpgraderAllowed(true)]
public class ActivateRagdollForCorpsesInZone : GameAction
{
	[ValidateNotNull]
	[AllowedEntityType(typeof(ScriptZone))]
	public EntityReference ScriptZone;

	[SerializeField]
	[Tooltip("Duration in seconds to keep ragdoll active")]
	public float Duration = 5f;

	[SerializeField]
	[Tooltip("Apply impulse when activating ragdoll")]
	public bool ApplyImpulse = true;

	[SerializeField]
	[ConditionalShow("ApplyImpulse")]
	[Tooltip("Direction of the impulse (will be normalized). Use Vector3.zero for random direction")]
	public Vector3 ImpulseDirection = Vector3.zero;

	[SerializeField]
	[ConditionalShow("ApplyImpulse")]
	[Tooltip("Magnitude of the impulse")]
	public float ImpulseMagnitude = 15f;

	[SerializeField]
	[Tooltip("Force ragdoll even if unit is already in ragdoll state")]
	public bool ForceRestart = true;

	[SerializeField]
	[Tooltip("Only affect units that are dead")]
	public bool OnlyDeadUnits = true;

	[SerializeField]
	[Tooltip("Include player faction units")]
	public bool IncludePlayerFaction;

	[SerializeField]
	[Tooltip("Random impulse variation (0-1, where 1 = 100% variation)")]
	[Range(0f, 1f)]
	public float ImpulseVariation = 0.3f;

	public override string GetDescription()
	{
		return $"Активирует регдолл для всех трупов в ScriptZone {ScriptZone?.EntityNameInEditor} на {Duration} секунд";
	}

	protected override void RunAction()
	{
		ScriptZone scriptZone = ScriptZone.FindView() as ScriptZone;
		if (scriptZone?.Data == null)
		{
			PFLog.Default.Warning("ActivateRagdollForCorpsesInZone: ScriptZone not found or has no data: " + ScriptZone?.EntityNameInEditor);
			return;
		}
		PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Processing ScriptZone " + scriptZone.name);
		List<BaseUnitEntity> unitsInZone = GetUnitsInZone(scriptZone);
		PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Found {unitsInZone.Count} units in zone");
		if (unitsInZone.Count == 0)
		{
			PFLog.Default.Log("ActivateRagdollForCorpsesInZone: No units found in zone");
			return;
		}
		int num = 0;
		foreach (BaseUnitEntity item in unitsInZone)
		{
			PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Checking unit {item.CharacterName}, IsDead: {item.LifeState.IsDead}, IsPlayer: {item.Faction.IsPlayer}");
			if (ShouldActivateRagdollForUnit(item))
			{
				ActivateRagdollForUnit(item);
				num++;
			}
		}
		PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Activated ragdoll for {num} units");
	}

	private List<BaseUnitEntity> GetUnitsInZone(ScriptZone scriptZone)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		List<ScriptZoneEntity.UnitInfo> insideUnits = scriptZone.Data.InsideUnits;
		PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: ScriptZone has {insideUnits.Count} units inside");
		foreach (ScriptZoneEntity.UnitInfo item in insideUnits)
		{
			if (!item.IsValid)
			{
				continue;
			}
			BaseUnitEntity baseUnitEntity = item.Reference.Entity?.ToBaseUnitEntity();
			if (baseUnitEntity != null && baseUnitEntity.IsInState)
			{
				PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Found unit in zone: {baseUnitEntity.CharacterName}, IsDead: {baseUnitEntity.LifeState.IsDead}");
				if (OnlyDeadUnits && !baseUnitEntity.LifeState.IsDead)
				{
					PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Skipping " + baseUnitEntity.CharacterName + " - not dead");
				}
				else if (!OnlyDeadUnits && baseUnitEntity.LifeState.IsDead)
				{
					PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Skipping " + baseUnitEntity.CharacterName + " - is dead but OnlyDeadUnits=false");
				}
				else
				{
					list.Add(baseUnitEntity);
				}
			}
		}
		return list;
	}

	private bool ShouldActivateRagdollForUnit(BaseUnitEntity unit)
	{
		if (OnlyDeadUnits && !unit.LifeState.IsDead)
		{
			PFLog.Default.Log("ActivateRagdollForCorpsesInZone: " + unit.CharacterName + " - not dead, skipping");
			return false;
		}
		if (!IncludePlayerFaction && unit.Faction.IsPlayer)
		{
			PFLog.Default.Log("ActivateRagdollForCorpsesInZone: " + unit.CharacterName + " - player faction, skipping");
			return false;
		}
		RigidbodyCreatureController rigidbodyCreatureController = unit.View?.GetComponent<RigidbodyCreatureController>();
		if (rigidbodyCreatureController == null)
		{
			rigidbodyCreatureController = unit.View?.GetComponentInChildren<RigidbodyCreatureController>();
		}
		if (rigidbodyCreatureController == null)
		{
			PFLog.Default.Log("ActivateRagdollForCorpsesInZone: " + unit.CharacterName + " - no RigidbodyCreatureController found");
			return false;
		}
		PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: {unit.CharacterName} - RigidbodyCreatureController found, RagdollWorking: {rigidbodyCreatureController.RagdollWorking}");
		if (!ForceRestart && rigidbodyCreatureController.RagdollWorking)
		{
			PFLog.Default.Log("ActivateRagdollForCorpsesInZone: " + unit.CharacterName + " - ragdoll already working, skipping");
			return false;
		}
		return true;
	}

	private void ActivateRagdollForUnit(BaseUnitEntity unit)
	{
		RigidbodyCreatureController rigidbodyCreatureController = unit.View?.GetComponent<RigidbodyCreatureController>() ?? unit.View?.GetComponentInChildren<RigidbodyCreatureController>();
		if (rigidbodyCreatureController == null)
		{
			PFLog.Default.Warning("ActivateRagdollForCorpsesInZone: " + unit.CharacterName + " - RigidbodyCreatureController not found for activation");
			return;
		}
		PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Starting ragdoll coroutine for " + unit.CharacterName);
		CoroutineRunner.Start(ActivateRagdollCoroutine(rigidbodyCreatureController, unit));
	}

	private IEnumerator ActivateRagdollCoroutine(RigidbodyCreatureController ragdollController, BaseUnitEntity unit)
	{
		bool wasRagdollActive = ragdollController.RagdollWorking;
		PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Starting ragdoll for {unit.CharacterName}, wasActive: {wasRagdollActive}");
		if (wasRagdollActive && ForceRestart)
		{
			PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Stopping existing ragdoll for " + unit.CharacterName);
			ragdollController.StopRagdoll();
			yield return null;
		}
		if (ApplyImpulse)
		{
			Vector3 impulseDirection = GetImpulseDirection(unit);
			float impulseMagnitude = GetImpulseMagnitude();
			PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Applying impulse to {unit.CharacterName}, direction: {impulseDirection}, magnitude: {impulseMagnitude}");
			if (impulseDirection != Vector3.zero)
			{
				ragdollController.ApplyImpulse(impulseDirection * impulseMagnitude, impulseMagnitude);
			}
		}
		PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Starting ragdoll for " + unit.CharacterName);
		ragdollController.StartRagdoll();
		PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Ragdoll started for {unit.CharacterName}, working: {ragdollController.RagdollWorking}");
		float elapsedTime = 0f;
		while (elapsedTime < Duration)
		{
			if (ragdollController == null || unit == null || !unit.IsInState)
			{
				PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Unit " + unit?.CharacterName + " or controller destroyed, stopping coroutine");
				yield break;
			}
			if (!ragdollController.RagdollWorking)
			{
				PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Ragdoll stopped externally for " + unit.CharacterName + ", stopping coroutine");
				yield break;
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (ragdollController != null && (!wasRagdollActive || ForceRestart))
		{
			PFLog.Default.Log($"ActivateRagdollForCorpsesInZone: Stopping ragdoll for {unit.CharacterName} after {Duration} seconds");
			ragdollController.StopRagdoll();
		}
		PFLog.Default.Log("ActivateRagdollForCorpsesInZone: Ragdoll coroutine finished for " + unit.CharacterName);
	}

	private Vector3 GetImpulseDirection(BaseUnitEntity unit)
	{
		Vector3 vector = ((!(ImpulseDirection == Vector3.zero)) ? ImpulseDirection.normalized : new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 0.5f), Random.Range(-1f, 1f)).normalized);
		if (ImpulseVariation > 0f)
		{
			Vector3 vector2 = new Vector3(Random.Range(0f - ImpulseVariation, ImpulseVariation), Random.Range((0f - ImpulseVariation) * 0.5f, ImpulseVariation * 0.5f), Random.Range(0f - ImpulseVariation, ImpulseVariation));
			vector = (vector + vector2).normalized;
		}
		return vector;
	}

	private float GetImpulseMagnitude()
	{
		if (ImpulseVariation > 0f)
		{
			float num = Random.Range(0f - ImpulseVariation, ImpulseVariation);
			return ImpulseMagnitude * (1f + num);
		}
		return ImpulseMagnitude;
	}

	public override string GetCaption()
	{
		string text = ScriptZone?.EntityNameInEditor ?? "???";
		string text2 = (ApplyImpulse ? $" with impulse ({ImpulseMagnitude})" : "");
		string text3 = (OnlyDeadUnits ? " (corpses only)" : "");
		return $"Activate Ragdoll for units in {text} for {Duration}s{text2}{text3}";
	}
}
