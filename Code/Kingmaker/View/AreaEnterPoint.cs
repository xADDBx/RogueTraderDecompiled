using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

[ExecuteAlways]
public class AreaEnterPoint : MonoBehaviour
{
	private static readonly List<AreaEnterPoint> Instances = new List<AreaEnterPoint>();

	[SerializeField]
	private BlueprintAreaEnterPointReference m_Blueprint;

	public bool RotateCameraOnEnter;

	public Transform CameraRotationTransform;

	public BlueprintAreaEnterPoint Blueprint
	{
		get
		{
			return m_Blueprint.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintAreaEnterPointReference>();
		}
	}

	[CanBeNull]
	public static AreaEnterPoint FindAreaEnterPointOnScene([NotNull] BlueprintAreaEnterPoint blueprint)
	{
		return Instances.FirstOrDefault((AreaEnterPoint point) => point.m_Blueprint.Is(blueprint));
	}

	public static AreaEnterPoint FindAreaEnterPointOnScene([NotNull] Func<AreaEnterPoint, bool> predicate)
	{
		return Instances.FirstOrDefault((AreaEnterPoint point) => predicate(point));
	}

	protected void OnEnable()
	{
		Instances.Add(this);
	}

	protected void OnDisable()
	{
		Instances.Remove(this);
	}

	public void PositionCharacters()
	{
		if (RotateCameraOnEnter)
		{
			Transform transform = (CameraRotationTransform ? CameraRotationTransform : base.transform);
			CameraRig.Instance.RotateToImmediately(180f + transform.rotation.eulerAngles.y);
		}
		if (BuildModeUtility.IsDevelopment && Game.Instance.CurrentlyLoadedArea.IsNavmeshArea)
		{
			if (AstarPath.active == null)
			{
				throw new Exception("Cannot place characters - NavMesh prefab is not active");
			}
			NavGraph[] graphs = AstarPath.active.graphs;
			if (graphs == null || graphs.All((NavGraph g) => g.CountNodes() <= 0))
			{
				throw new Exception("Cannot place characters - NavMesh is empty on " + Utilities.GetBlueprintName(Blueprint));
			}
		}
		if (!Game.Instance.CurrentlyLoadedArea.IsPartyArea)
		{
			HideCharacters();
		}
		else if (base.transform.childCount > 0)
		{
			PositionCharactersByPreset();
		}
		else
		{
			PositionCharactersByFormation();
		}
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.SpaceCombat)
		{
			PlaceShip();
		}
		else
		{
			HideShip();
		}
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem)
		{
			Game.Instance.StarSystemMapController.EnterPointStartedFrom = this;
		}
	}

	private void PlaceShip()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		if (playerShip != null)
		{
			playerShip.IsInGame = true;
			float x = 0f;
			if ((bool)GetComponent<StarSystemObjectView>())
			{
				SphereCollider component = GetComponent<SphereCollider>();
				x = ((component != null) ? component.radius : 0f);
			}
			if (!Game.Instance.CurrentlyLoadedArea.IsGlobalmapArea)
			{
				Vector3 vector = Quaternion.Euler(0f, PFStatefulRandom.View.value * 180f, 0f) * new Vector3(x, 0f, 0f);
				Vector3 p = AdjustPosition(base.transform.position) + vector;
				PositionCharacter(playerShip, p, base.transform.rotation);
			}
			else
			{
				PositionCharacter(playerShip, Vector3.zero, base.transform.rotation);
			}
		}
	}

	private static void HideShip()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		if (playerShip != null)
		{
			playerShip.IsInGame = false;
		}
	}

	private static void HideCharacters()
	{
		List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (ShouldMoveCharacterOnAreaEnterPoint(partyAndPet))
			{
				list.Add(partyAndPet);
			}
		}
		foreach (BaseUnitEntity item in list)
		{
			item.IsInGame = false;
		}
	}

	private void PositionCharactersByPreset()
	{
		Vector3 vector = AdjustPosition(base.transform.position);
		PositionCharacter(Game.Instance.Player.MainCharacterEntity, vector, base.transform);
		int num = 0;
		foreach (BaseUnitEntity item in Game.Instance.Player.PartyAndPets.Where(ShouldMoveCharacterOnAreaEnterPoint))
		{
			item.IsInGame = true;
			if (item != Game.Instance.Player.MainCharacterEntity && !item.LifeState.IsFinallyDead)
			{
				num++;
				Transform transform = ((base.transform.childCount < num) ? null : base.transform.GetChild(num - 1));
				if ((bool)transform)
				{
					PositionCharacter(item, vector, transform);
					continue;
				}
				PositionCharacter(item, vector, base.transform);
				Vector2 vector2 = PFStatefulRandom.View.insideUnitCircle.normalized * 2f;
				Vector3 targetPosition = item.Position + new Vector3(vector2.x, 0f, vector2.y);
				item.Position = TracePosition(vector, targetPosition);
				PFLog.Default.Warning("Area entry point unable to position character " + item, this);
			}
		}
	}

	private void PositionCharactersByFormation()
	{
		List<BaseUnitEntity> list = Game.Instance.Player.PartyAndPets.Where(ShouldMoveCharacterOnAreaEnterPoint).ToList();
		Span<Vector3> resultPositions = stackalloc Vector3[list.Count];
		Vector3 direction = base.transform.rotation * Vector3.forward;
		Game.Instance.Player.FormationManager.UpdateAutoFormation();
		PartyFormationHelper.FillFormationPositions(base.transform.position, FormationAnchor.Center, direction, list, list, Game.Instance.Player.FormationManager.CurrentFormation, resultPositions);
		for (int i = 0; i < list.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = list[i];
			baseUnitEntity.IsInGame = true;
			if (!baseUnitEntity.LifeState.IsFinallyDead)
			{
				Vector3 p = resultPositions[i];
				Transform transform = base.transform;
				p.y = transform.position.y;
				PositionCharacter(baseUnitEntity, p, transform.rotation);
				if (baseUnitEntity.MovementAgent.Position.GetNearestNodeXZUnwalkable()?.Area != base.transform.position.GetNearestNodeXZUnwalkable()?.Area)
				{
					PFLog.Pathfinding.Error("Character still on previous zone! Teleporting to enter point");
					PositionCharacter(baseUnitEntity, base.transform.position, transform.rotation);
					baseUnitEntity.SnapToGrid();
				}
			}
		}
	}

	private static void PositionCharacter(BaseUnitEntity character, Vector3 basePos, Transform t)
	{
		Vector3 p = TracePosition(basePos, t.position);
		PositionCharacter(character, p, t.rotation);
	}

	private static void PositionCharacter(BaseUnitEntity character, Vector3 p, Quaternion rot)
	{
		character.Commands.InterruptAllInterruptible();
		ObjectExtensions.Or(character.View, null)?.StopMoving();
		character.Translocate(p, rot.eulerAngles.y);
	}

	private static Vector3 AdjustPosition(Vector3 position)
	{
		if (!(AstarPath.active != null))
		{
			return position;
		}
		return ObstacleAnalyzer.GetNearestNode(position).position;
	}

	private static Vector3 TracePosition(Vector3 basePosition, Vector3 targetPosition)
	{
		if (!(AstarPath.active != null))
		{
			return targetPosition;
		}
		return ObstacleAnalyzer.TraceAlongNavmesh(basePosition, targetPosition);
	}

	public static bool ShouldMoveCharacterOnAreaEnterPoint(BaseUnitEntity character)
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode)
		{
			if (character != Game.Instance.Player.MainCharacterEntity)
			{
				return character.Master == Game.Instance.Player.MainCharacterEntity;
			}
			return true;
		}
		BaseUnitEntity baseUnitEntity = character.Master ?? character;
		if (baseUnitEntity.Faction.IsDirectlyControllable && !character.LifeState.IsFinallyDead && !baseUnitEntity.IsDetached)
		{
			UnitPartCompanion optional = baseUnitEntity.GetOptional<UnitPartCompanion>();
			if (optional == null)
			{
				return true;
			}
			return optional.State != CompanionState.ExCompanion;
		}
		return false;
	}

	public static AreaEnterPoint GetClosest(Transform transform)
	{
		if (Instances.Count != 0)
		{
			return Instances.MinBy((AreaEnterPoint v) => (v.transform.position - transform.position).sqrMagnitude);
		}
		return null;
	}
}
