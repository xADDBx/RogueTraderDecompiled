using Code.Visual.Animation;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Spawners;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Roaming;

[RequireComponent(typeof(UnitSpawnerBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("35323d72d20e37f4ab636e75ceeadda3")]
public class SpawnerRoamingSettings : EntityPartComponent<SpawnerRoamingSettings.Part>
{
	public enum ModeType
	{
		Patrol,
		Radius,
		Cutscene
	}

	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerRoamingSettings Source => (SpawnerRoamingSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			UnitPartRoaming orCreate = unit.GetOrCreate<UnitPartRoaming>();
			if (Source.Sleepless)
			{
				unit.Sleepless.Retain();
			}
			if (Source.Mode == ModeType.Patrol && Source.FirstWaypoint?.FindData() is RoamingWaypointData nextPoint)
			{
				orCreate.NextPoint = nextPoint;
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			UnitPartRoaming orCreate = unit.GetOrCreate<UnitPartRoaming>();
			UnitPartRoaming unitPartRoaming = orCreate;
			if (unitPartRoaming.Settings == null)
			{
				RoamingUnitSettings roamingUnitSettings2 = (unitPartRoaming.Settings = new RoamingUnitSettings());
			}
			orCreate.Settings.MovementType = Source.MovementType;
			orCreate.Settings.MovementSpeed = Source.MovementSpeed;
			orCreate.Settings.Sleepless = Source.Sleepless;
			orCreate.OriginalPoint = Source.transform.position;
			switch (Source.Mode)
			{
			case ModeType.Radius:
				orCreate.Settings.Radius = Source.Radius;
				orCreate.Settings.MinIdleTime = Source.MinIdleTime;
				orCreate.Settings.MaxIdleTime = Source.MaxIdleTime;
				break;
			case ModeType.Cutscene:
				orCreate.Settings.SetCutscenes(Source.IdleCutscenes);
				orCreate.Settings.MinIdleTime = Source.MinIdleTime;
				orCreate.Settings.MaxIdleTime = Source.MaxIdleTime;
				break;
			}
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
			unit.GetOptional<UnitPartRoaming>()?.IdleCutscene?.Stop();
			unit.Remove<UnitPartRoaming>();
			if (Source.Sleepless)
			{
				unit.Sleepless.Release();
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public ModeType Mode;

	[AllowedEntityType(typeof(RoamingWaypointView))]
	[ShowIf("IsPatrolMode")]
	public EntityReference FirstWaypoint;

	[ShowIf("IsRadiusMode")]
	public float Radius = 1f;

	[ShowIf("IsCutsceneMode")]
	public CutsceneReference[] IdleCutscenes = new CutsceneReference[0];

	[ShowIf("IsRadiusOrCutsceneMode")]
	public float MinIdleTime;

	[ShowIf("IsRadiusOrCutsceneMode")]
	public float MaxIdleTime;

	public WalkSpeedType MovementType = WalkSpeedType.Walk;

	public float MovementSpeed;

	public bool Sleepless;

	private bool IsPatrolMode => Mode == ModeType.Patrol;

	private bool IsRadiusMode => Mode == ModeType.Radius;

	private bool IsCutsceneMode => Mode == ModeType.Cutscene;

	private bool IsRadiusOrCutsceneMode
	{
		get
		{
			ModeType mode = Mode;
			return mode == ModeType.Radius || mode == ModeType.Cutscene;
		}
	}
}
