using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.Random;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.SpaceCombat.MeteorStream;

public class MeteorStreamEntity : MechanicEntity<BlueprintMeteorStream>, PartMeteorCombatState.IOwner, IEntityPartOwner<PartMeteorCombatState>, IEntityPartOwner, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[JsonProperty]
	private List<MeteorEntity> m_Meteors = new List<MeteorEntity>();

	[JsonProperty]
	private Vector2 m_CurrentPosition;

	[JsonProperty]
	private float m_PreviousDistance;

	public override bool IsInCombat => CombatState.IsInCombat;

	public new MeteorStreamView View => base.View as MeteorStreamView;

	public PartMeteorCombatState CombatState => GetRequired<PartMeteorCombatState>();

	public Vector2 StreamDirection => base.Blueprint.Direction;

	public Vector2 StreamPosition => base.Blueprint.Position;

	public int GridWidth => base.Blueprint.Width;

	public float CellSize => GraphParamsMechanicsCache.GridCellSize;

	public float RealWidth => (float)GridWidth * CellSize;

	public float FloatingSpeed => base.Blueprint.MeteorsFloatingSpeed;

	public int StreamLengthInCells => base.Blueprint.StreamLengthInCells;

	public MeteorStreamEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public MeteorStreamEntity(string uniqueId, bool isInGame, BlueprintMeteorStream blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		MeteorStreamView meteorStreamView = new GameObject().AddComponent<MeteorStreamView>();
		AttachView(meteorStreamView);
		Game.Instance.DynamicRoot.Add(meteorStreamView.transform);
		return meteorStreamView;
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartMeteorCombatState>();
	}

	public void CreateAndAttachView()
	{
		CreateViewForData();
	}

	public void InitMeteors(List<BlueprintMeteor> meteorBlueprints)
	{
		foreach (Vector2 item in InitMeteorsPosition())
		{
			MeteorEntity meteorEntity = CreateMeteorEntity(meteorBlueprints, item);
			CreateMeteorView(meteorEntity, item);
			meteorEntity.CalculatePath();
		}
	}

	public void SpawnImmediately()
	{
		foreach (MeteorEntity meteor in m_Meteors)
		{
			Game.Instance.EntitySpawner.SpawnEntityImmediately(meteor, Game.Instance.LoadedAreaState.MainState);
		}
	}

	private MeteorEntity CreateMeteorEntity(List<BlueprintMeteor> meteors, Vector2 position)
	{
		BlueprintMeteor blueprint = meteors[PFStatefulRandom.SpaceCombat.Range(0, meteors.Count)];
		MeteorEntity meteorEntity = Entity.Initialize(new MeteorEntity(Uuid.Instance.CreateString(), isInGame: true, blueprint));
		meteorEntity.Init(this);
		m_Meteors.Add(meteorEntity);
		return meteorEntity;
	}

	public void CreateMeteorView(MeteorEntity meteor, Vector2 position)
	{
		meteor.CreateAndAttachView();
		meteor.SetHoldingState(Game.Instance.LoadedAreaState.MainState);
		meteor.SetPosition(position);
		meteor.SnapToGrid();
	}

	public Dictionary<CustomGridNodeBase, MeteorEntity> GetMeteorsDangerZones()
	{
		Dictionary<CustomGridNodeBase, MeteorEntity> dictionary = new Dictionary<CustomGridNodeBase, MeteorEntity>();
		foreach (MeteorEntity meteor in m_Meteors)
		{
			foreach (CustomGridNodeBase getDangerNode in meteor.GetDangerNodes)
			{
				dictionary.TryAdd(getDangerNode, meteor);
			}
		}
		return dictionary;
	}

	private Vector2 NearestPointOnLine(Vector2 origin, Vector2 direction, Vector2 point)
	{
		direction.Normalize();
		float num = Vector2.Dot(point - origin, direction);
		return origin + direction * num;
	}

	private Vector2 PerpendicularClockwise(Vector2 dir)
	{
		return new Vector2(dir.y, 0f - dir.x);
	}

	private List<Vector2> InitMeteorsPosition()
	{
		Vector2 vector = StreamPosition + (float)(StreamLengthInCells / 2) * CellSize * -StreamDirection.normalized + Vector2.Perpendicular(StreamDirection.normalized) * RealWidth / 2f;
		Vector2 startOfLine = vector;
		float num = 0f;
		List<Vector2> list = new List<Vector2>();
		do
		{
			num += GeneratePositionForLine(startOfLine, list);
			startOfLine = vector + StreamDirection.normalized * num;
		}
		while (!(num / CellSize > (float)StreamLengthInCells) && list.Count <= 1000);
		return list;
	}

	private float GeneratePositionForLine(Vector2 startOfLine, List<Vector2> positions)
	{
		Vector2 normalized = PerpendicularClockwise(StreamDirection).normalized;
		int num = 0;
		int num2 = 0;
		while (true)
		{
			int num3 = PFStatefulRandom.SpaceCombat.Range(base.Blueprint.DensityFrom, base.Blueprint.DensityTo + 1);
			int num4 = PFStatefulRandom.SpaceCombat.Range(base.Blueprint.DensityFrom, base.Blueprint.DensityTo + 1);
			num += num3;
			num2 = Math.Max(num2, num4);
			if (num >= GridWidth)
			{
				break;
			}
			Vector2 item = startOfLine + normalized * (CellSize * (float)num) + (float)num4 * CellSize * StreamDirection.normalized;
			positions.Add(item);
		}
		return (float)num2 * CellSize;
	}

	public void HandleTick(float deltaTime)
	{
		MoveMeteors(deltaTime);
		CheckForCollidedMeteors();
	}

	private void MoveMeteors(float deltaTime)
	{
		Vector2 vector = StreamDirection.normalized * (FloatingSpeed * deltaTime);
		m_CurrentPosition += vector;
		Vector2 b = StreamDirection * CellSize;
		float num = Vector2.Distance(m_CurrentPosition, b);
		float distance = 1f - num / Vector2.Distance(Vector2.zero, b);
		foreach (MeteorEntity meteor in m_Meteors)
		{
			meteor.Move(vector);
			meteor.CheckCollision(distance);
		}
		if (num - m_PreviousDistance >= 0f)
		{
			EndOfTurn();
		}
		else
		{
			m_PreviousDistance = num;
		}
	}

	private void CheckForCollidedMeteors()
	{
		foreach (MeteorEntity item in m_Meteors.Where((MeteorEntity x) => x.NeedToBeDestroyed))
		{
			Game.Instance.EntityDestroyer.Destroy(item);
		}
		m_Meteors.RemoveAll((MeteorEntity x) => x.NeedToBeDestroyed);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (Game.Instance.TurnController.CurrentUnit == this)
		{
			ResetMovementTracker();
			UnblockMeteorNodes();
		}
	}

	private void ResetMovementTracker()
	{
		m_CurrentPosition = Vector2.zero;
		m_PreviousDistance = float.MaxValue;
	}

	private void UnblockMeteorNodes()
	{
		foreach (MeteorEntity meteor in m_Meteors)
		{
			meteor.UnblockCurrentNode();
		}
	}

	private void TeleportOutOfRangeMeteors()
	{
		Vector2 point = Game.Instance.Player.PlayerShip.Position.To2D();
		Vector2 vector = NearestPointOnLine(StreamPosition, StreamDirection, point);
		float num = (float)StreamLengthInCells * CellSize / 2f;
		foreach (MeteorEntity meteor in m_Meteors)
		{
			float num2 = Vector2.Dot(StreamDirection.normalized, meteor.Position.To2D() - vector);
			if (num2 > num)
			{
				meteor.Move(-StreamDirection.normalized * ((float)StreamLengthInCells * CellSize));
			}
			else if (num2 < 0f - num)
			{
				meteor.Move(StreamDirection.normalized * ((float)StreamLengthInCells * CellSize));
			}
		}
	}

	private void EndOfTurn()
	{
		TeleportOutOfRangeMeteors();
		foreach (MeteorEntity meteor in m_Meteors)
		{
			meteor.SnapToGrid();
			meteor.CalculatePath();
		}
		Game.Instance.TurnController.RequestEndTurn();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<MeteorEntity> meteors = m_Meteors;
		if (meteors != null)
		{
			for (int i = 0; i < meteors.Count; i++)
			{
				Hash128 val2 = ClassHasher<MeteorEntity>.GetHash128(meteors[i]);
				result.Append(ref val2);
			}
		}
		result.Append(ref m_CurrentPosition);
		result.Append(ref m_PreviousDistance);
		return result;
	}
}
