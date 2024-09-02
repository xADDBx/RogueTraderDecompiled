using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Warhammer.SpaceCombat.StarshipLogic.Weapon;

public class WeaponSlot : ItemSlot, IHashable
{
	private readonly struct FiringArcSourceNode : IEquatable<FiringArcSourceNode>
	{
		public readonly CustomGridNodeBase Node;

		public readonly RestrictedFiringArc FiringArc;

		private readonly Vector3 m_ArcLeftSide;

		private readonly Vector3 m_ArcRightSide;

		public FiringArcSourceNode(CustomGridNodeBase node, int starshipDirection, RestrictedFiringArc firingArc)
		{
			Node = node;
			FiringArc = firingArc;
			int[] validDirections = FiringArcHelper.GetValidDirections(firingArc, starshipDirection);
			m_ArcLeftSide = CustomGraphHelper.GetVector3Direction(validDirections.First());
			m_ArcRightSide = CustomGraphHelper.GetVector3Direction(validDirections.Last());
		}

		public bool CheckDirection(Vector3 direction)
		{
			Vector3 from = new Vector3(MathF.Round(m_ArcLeftSide.x, 6), m_ArcLeftSide.y, MathF.Round(m_ArcLeftSide.z, 6));
			Vector3 to = new Vector3(MathF.Round(m_ArcRightSide.x, 6), m_ArcRightSide.y, MathF.Round(m_ArcRightSide.z, 6));
			float num = Vector3.SignedAngle(from, direction, Vector3.up);
			float x = Vector3.SignedAngle(from, to, Vector3.up);
			if (num >= 0f)
			{
				return MathF.Round(num, 2) <= MathF.Round(x, 2);
			}
			return false;
		}

		public bool Equals(FiringArcSourceNode other)
		{
			if (object.Equals(Node, other.Node))
			{
				return FiringArc == other.FiringArc;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FiringArcSourceNode other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Node, (int)FiringArc);
		}
	}

	private readonly struct SourceNodesKey : IEquatable<SourceNodesKey>
	{
		public readonly RestrictedFiringArc Arc;

		public readonly IntRect StarshipRect;

		public readonly int Direction;

		public readonly int Offset;

		public readonly int BatteryWidth;

		public SourceNodesKey(RestrictedFiringArc arc, IntRect starshipRect, int direction, int offset, int batteryWidth)
		{
			Arc = arc;
			StarshipRect = starshipRect;
			Direction = direction;
			Offset = offset;
			BatteryWidth = batteryWidth;
		}

		public bool Equals(SourceNodesKey other)
		{
			if (Arc == other.Arc && StarshipRect == other.StarshipRect && Direction == other.Direction && Offset == other.Offset)
			{
				return BatteryWidth == other.BatteryWidth;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is SourceNodesKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)Arc, StarshipRect, Direction, Offset, BatteryWidth);
		}
	}

	[JsonProperty]
	public WeaponSlotType Type;

	[JsonProperty]
	public int OffsetFromProw;

	[JsonProperty]
	public int BatteryWidth;

	[JsonProperty]
	public AmmoSlot AmmoSlot;

	[JsonProperty]
	private readonly List<ItemEntityStarshipWeapon> ArsenalWeapons = new List<ItemEntityStarshipWeapon>();

	[JsonProperty]
	private readonly List<BlueprintStarshipAmmo> ArsenalAmmo = new List<BlueprintStarshipAmmo>();

	[JsonProperty]
	private int m_ActiveWeaponIndex;

	private static readonly ThreadLocal<Dictionary<FiringArcSourceNode, PooledList<CustomGridNodeBase>>> Nodes = new ThreadLocal<Dictionary<FiringArcSourceNode, PooledList<CustomGridNodeBase>>>(() => new Dictionary<FiringArcSourceNode, PooledList<CustomGridNodeBase>>());

	private static readonly StaticCache<SourceNodesKey, Vector2Int[]> Offsets = new StaticCache<SourceNodesKey, Vector2Int[]>(ArcSourceNodesImpl);

	public ItemEntityStarshipWeapon Weapon
	{
		get
		{
			if (ActiveWeaponIndex == 0 || ArsenalWeapons.Count < ActiveWeaponIndex)
			{
				return base.MaybeItem as ItemEntityStarshipWeapon;
			}
			return ArsenalWeapons[ActiveWeaponIndex - 1];
		}
	}

	public RestrictedFiringArc FiringArc => Type switch
	{
		WeaponSlotType.Dorsal => RestrictedFiringArc.Dorsal, 
		WeaponSlotType.Prow => RestrictedFiringArc.Fore, 
		WeaponSlotType.Port => RestrictedFiringArc.Port, 
		WeaponSlotType.Starboard => RestrictedFiringArc.Starboard, 
		WeaponSlotType.Keel => RestrictedFiringArc.Any, 
		_ => RestrictedFiringArc.None, 
	};

	public int ActiveWeaponIndex
	{
		get
		{
			return m_ActiveWeaponIndex;
		}
		set
		{
			int num = Math.Clamp(value, 0, ArsenalWeapons.Count);
			if (m_ActiveWeaponIndex != num)
			{
				m_ActiveWeaponIndex = num;
				Weapon.UpdateAbilities(base.Owner, base.Item);
				SetupAmmo();
				EventBus.RaiseEvent(delegate(IWeaponSlotHandler h)
				{
					h.HandleActiveWeaponIndexChanged(this);
				});
			}
		}
	}

	public IEnumerable<ItemEntityStarshipWeapon> WeaponVariants
	{
		get
		{
			yield return base.Item as ItemEntityStarshipWeapon;
			foreach (ItemEntityStarshipWeapon arsenalWeapon in ArsenalWeapons)
			{
				yield return arsenalWeapon;
			}
		}
	}

	public IEnumerable<Ability> AbilityVariants => WeaponVariants.Select((ItemEntityStarshipWeapon item) => item.Abilities.FirstOrDefault());

	public Ability ActiveAbility => Weapon?.Abilities.FirstOrDefault((Ability a) => a != null);

	public WeaponSlot(BaseUnitEntity owner, WeaponSlotData slotData)
		: base(owner)
	{
		Type = slotData.Type;
		OffsetFromProw = slotData.OffsetFromProw;
		BatteryWidth = slotData.Width;
		AmmoSlot = new AmmoSlot(owner, this);
	}

	[JsonConstructor]
	public WeaponSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		if (item is ItemEntityStarshipWeapon itemEntityStarshipWeapon)
		{
			return itemEntityStarshipWeapon.Blueprint.AllowedSlots.Contains(Type);
		}
		return false;
	}

	public void Block()
	{
		bool num = Weapon.IsBlocked;
		Weapon.IsBlocked.Retain();
		if (!num && (bool)Weapon.IsBlocked)
		{
			EventBus.RaiseEvent(delegate(IStarshipComponentHandler h)
			{
				h.HandleComponentBlocked(this);
			});
		}
	}

	public void Unblock()
	{
		Weapon.IsBlocked.Release();
	}

	public bool CanEquipAmmo(ItemEntityStarshipAmmo ammo)
	{
		return AmmoSlot.CanInsertItem(ammo);
	}

	public void EquipAmmo(ItemEntityStarshipAmmo ammo, bool reloadInstantly = false, bool force = false)
	{
		AmmoSlot.InsertItem(ammo, force);
		Weapon.Charges = (reloadInstantly ? Weapon.Blueprint.Charges : 0);
	}

	public void UnequipAmmo()
	{
		AmmoSlot.RemoveItem();
		Weapon.Charges = 0;
	}

	public void SetupAmmo()
	{
		BlueprintStarshipAmmo blueprintStarshipAmmo = ((ActiveWeaponIndex > 0 && ArsenalAmmo.Count >= ActiveWeaponIndex) ? ArsenalAmmo[ActiveWeaponIndex - 1] : Weapon.Blueprint.DefaultAmmo);
		if (blueprintStarshipAmmo != null)
		{
			ItemEntityStarshipAmmo itemEntityStarshipAmmo = Entity.Initialize(new ItemEntityStarshipAmmo(blueprintStarshipAmmo));
			(base.Owner as StarshipEntity)?.Inventory.Add(itemEntityStarshipAmmo);
			EquipAmmo(itemEntityStarshipAmmo, reloadInstantly: true, force: true);
		}
	}

	protected override void OnItemInserted()
	{
		RefreshArsenals();
		SetupAmmo();
	}

	public void RefreshArsenals()
	{
		if (!(base.Owner is StarshipEntity starshipEntity))
		{
			return;
		}
		DisposeArsenals();
		ArsenalWeapons.Clear();
		ArsenalAmmo.Clear();
		if (base.MaybeItem == null)
		{
			return;
		}
		List<ArsenalSlot> arsenals = starshipEntity.Hull.HullSlots.Arsenals;
		if ((arsenals?.Count ?? 0) <= 0)
		{
			return;
		}
		BlueprintStarshipWeapon wbp = (base.MaybeItem as ItemEntityStarshipWeapon)?.Blueprint;
		if (wbp == null)
		{
			return;
		}
		foreach (BlueprintItemArsenal item in from aSlot in arsenals
			where aSlot.HasItem
			select aSlot.MaybeItem?.Blueprint as BlueprintItemArsenal into a
			where a != null && a.AppliedWeaponType == wbp.WeaponType && (a.FilterDamageType == DamageType.None || wbp.DefaultAmmo?.DamageType.Type == a.FilterDamageType) && (a.VariantWeapon == null || a.VariantWeapon.AllowedSlots.Contains(Type))
			select a)
		{
			ItemEntityStarshipWeapon itemEntityStarshipWeapon = Entity.Initialize(new ItemEntityStarshipWeapon(item.VariantWeapon ?? wbp));
			itemEntityStarshipWeapon.HoldingSlot = this;
			itemEntityStarshipWeapon.PrepareAbilities(base.Owner, base.MaybeItem);
			ArsenalWeapons.Add(itemEntityStarshipWeapon);
			ArsenalAmmo.Add(item.VariantAmmo ?? itemEntityStarshipWeapon.Blueprint.DefaultAmmo ?? wbp.DefaultAmmo);
		}
	}

	public override bool RemoveItem(bool autoMerge = true, bool force = false)
	{
		DisposeArsenals();
		return base.RemoveItem(autoMerge);
	}

	public void DisposeArsenals()
	{
		if (!(base.Owner is StarshipEntity starshipEntity))
		{
			return;
		}
		using (ContextData<ItemsCollection.DoNotRemoveFromSlot>.Request())
		{
			foreach (ItemEntityStarshipWeapon arsenalWeapon in ArsenalWeapons)
			{
				starshipEntity.Inventory.Remove(arsenalWeapon);
				arsenalWeapon.HoldingSlot = null;
			}
		}
	}

	public void Reload()
	{
		(base.Item as ItemEntityStarshipWeapon)?.Reload();
	}

	public bool IsTargetInsideRestrictedFiringArc(TargetWrapper target, int range, RestrictedFiringAreaComponent restrictedFiringAreaComponent, CustomGridNodeBase overridePosition = null, int? overrideDirection = null)
	{
		CustomGridNodeBase customGridNodeBase = overridePosition ?? ObstacleAnalyzer.GetNearestNodeXZUnwalkable(base.Owner.Position);
		int starshipDirection = overrideDirection ?? CustomGraphHelper.GuessDirection(base.Owner.Forward);
		CustomGridNodeBase[] array2;
		if (target.Entity == null)
		{
			CustomGridNodeBase[] array = new CustomGridNode[1] { ObstacleAnalyzer.GetNearestNodeXZUnwalkable(target.Point) };
			array2 = array;
		}
		else
		{
			array2 = target.Entity.GetOccupiedNodes().ToArray();
		}
		CustomGridNodeBase[] array3 = array2;
		if (customGridNodeBase == null || array3.Length == 0)
		{
			return false;
		}
		Vector2Int startNodeCoord = customGridNodeBase.CoordinatesInGrid;
		CustomGridGraph graph = (CustomGridGraph)customGridNodeBase.Graph;
		HashSet<FiringArcSourceNode> arcSourceNodes = TempHashSet.Get<FiringArcSourceNode>();
		using (ProfileScope.New("Gather FiringArcSourceNodes"))
		{
			switch (Type)
			{
			case WeaponSlotType.Prow:
				GetAdjustedSourceNodes(RestrictedFiringArc.Fore);
				break;
			case WeaponSlotType.Port:
				GetAdjustedSourceNodes(RestrictedFiringArc.Port);
				break;
			case WeaponSlotType.Starboard:
				GetAdjustedSourceNodes(RestrictedFiringArc.Starboard);
				break;
			case WeaponSlotType.Dorsal:
				GetAdjustedSourceNodes(RestrictedFiringArc.Fore);
				GetAdjustedSourceNodes(RestrictedFiringArc.Port);
				GetAdjustedSourceNodes(RestrictedFiringArc.Starboard);
				break;
			}
		}
		Dictionary<FiringArcSourceNode, PooledList<CustomGridNodeBase>> value = Nodes.Value;
		try
		{
			using (ProfileScope.New("Exclude not-in-range nodes"))
			{
				foreach (FiringArcSourceNode item in arcSourceNodes)
				{
					CustomGridNodeBase[] array = array3;
					foreach (CustomGridNodeBase customGridNodeBase2 in array)
					{
						int warhammerCellDistance = CustomGraphHelper.GetWarhammerCellDistance(item.Node, customGridNodeBase2);
						if (warhammerCellDistance == 0)
						{
							if (value.TryGetValue(item, out var value2))
							{
								value2.Add(customGridNodeBase2);
							}
							else
							{
								PooledList<CustomGridNodeBase> pooledList = PooledList<CustomGridNodeBase>.Get();
								pooledList.Add(customGridNodeBase2);
								value[item] = pooledList;
							}
						}
						Vector3 direction = customGridNodeBase2.Vector3Position - item.Node.Vector3Position;
						if (warhammerCellDistance <= range && item.CheckDirection(direction))
						{
							if (value.TryGetValue(item, out var value3))
							{
								value3.Add(customGridNodeBase2);
								continue;
							}
							PooledList<CustomGridNodeBase> pooledList2 = PooledList<CustomGridNodeBase>.Get();
							pooledList2.Add(customGridNodeBase2);
							value[item] = pooledList2;
						}
					}
				}
			}
			if (value.Count == 0)
			{
				return false;
			}
			if (restrictedFiringAreaComponent == null)
			{
				return true;
			}
			using (ProfileScope.New("CheckIsInsideFiringArc"))
			{
				Vector3 vector3Direction = CustomGraphHelper.GetVector3Direction(starshipDirection);
				foreach (KeyValuePair<FiringArcSourceNode, PooledList<CustomGridNodeBase>> item2 in value)
				{
					HashSet<CustomGridNodeBase> restrictedArea = restrictedFiringAreaComponent.GetRestrictedArea(item2.Key.Node, item2.Key.FiringArc, vector3Direction);
					foreach (CustomGridNodeBase item3 in item2.Value)
					{
						if (restrictedArea.Contains(item3))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		finally
		{
			foreach (KeyValuePair<FiringArcSourceNode, PooledList<CustomGridNodeBase>> item4 in value)
			{
				PooledList<CustomGridNodeBase>.Return(item4.Value);
			}
			value.Clear();
		}
		void GetAdjustedSourceNodes(RestrictedFiringArc arc)
		{
			Vector2Int[] firingArcSourceNodesOffsets = GetFiringArcSourceNodesOffsets(starshipDirection, base.Owner.SizeRect, arc, OffsetFromProw, BatteryWidth);
			foreach (Vector2Int vector2Int in firingArcSourceNodesOffsets)
			{
				Vector2Int vector2Int2 = startNodeCoord + vector2Int;
				CustomGridNodeBase node = FiringArcHelper.AdjustStartNode(graph.GetNode(vector2Int2.x, vector2Int2.y), arc, starshipDirection);
				arcSourceNodes.Add(new FiringArcSourceNode(node, starshipDirection, arc));
			}
		}
	}

	public HashSet<CustomGridNodeBase> GetRestrictedFiringArcNodes(int range, RestrictedFiringAreaComponent restrictedFiringAreaComponent, CustomGridNodeBase overridePosition = null, int? overrideDirection = null)
	{
		HashSet<CustomGridNodeBase> hashSet = TempHashSet.Get<CustomGridNodeBase>();
		CustomGridNodeBase customGridNodeBase = overridePosition ?? base.Owner.Position.GetNearestNodeXZUnwalkable();
		if (customGridNodeBase == null)
		{
			return hashSet;
		}
		int num = overrideDirection ?? CustomGraphHelper.GuessDirection(base.Owner.Forward);
		switch (Type)
		{
		case WeaponSlotType.Prow:
			GetFiringArcNodes(hashSet, customGridNodeBase, num, base.Owner.SizeRect, RestrictedFiringArc.Fore, 0, 0, range, restrictedFiringAreaComponent);
			break;
		case WeaponSlotType.Port:
			GetFiringArcNodes(hashSet, customGridNodeBase, num, base.Owner.SizeRect, RestrictedFiringArc.Port, OffsetFromProw, BatteryWidth, range, restrictedFiringAreaComponent);
			break;
		case WeaponSlotType.Starboard:
			GetFiringArcNodes(hashSet, customGridNodeBase, num, base.Owner.SizeRect, RestrictedFiringArc.Starboard, OffsetFromProw, BatteryWidth, range, restrictedFiringAreaComponent);
			break;
		case WeaponSlotType.Dorsal:
			GetFiringArcNodes(hashSet, customGridNodeBase, num, base.Owner.SizeRect, RestrictedFiringArc.Fore, OffsetFromProw, 0, range, restrictedFiringAreaComponent);
			GetFiringArcNodes(hashSet, customGridNodeBase, num, base.Owner.SizeRect, RestrictedFiringArc.Port, OffsetFromProw, BatteryWidth, range, restrictedFiringAreaComponent);
			GetFiringArcNodes(hashSet, customGridNodeBase, num, base.Owner.SizeRect, RestrictedFiringArc.Starboard, OffsetFromProw, BatteryWidth, range, restrictedFiringAreaComponent);
			break;
		}
		foreach (CustomGridNodeBase node in GridAreaHelper.GetNodes(customGridNodeBase.Vector3Position, base.Owner.SizeRect, CustomGraphHelper.GetVector3Direction(num)))
		{
			hashSet.Remove(node);
		}
		return hashSet;
	}

	private void GetFiringArcNodes(HashSet<CustomGridNodeBase> result, CustomGridNodeBase position, int direction, IntRect sizeRect, RestrictedFiringArc firingArc, int offsetFromProw, int batteryWidth, int range, RestrictedFiringAreaComponent restrictedAreaComponent = null)
	{
		HashSet<CustomGridNodeBase> hashSet = TempHashSet.Get<CustomGridNodeBase>();
		GetFiringArcSourceNodes(hashSet, position, direction, sizeRect, firingArc, offsetFromProw, batteryWidth);
		HashSet<CustomGridNodeBase> hashSet2 = TempHashSet.Get<CustomGridNodeBase>();
		Vector3 vector3Direction = CustomGraphHelper.GetVector3Direction(direction);
		foreach (CustomGridNodeBase item in hashSet)
		{
			hashSet2.Clear();
			FiringArcHelper.TraverseGraph(item, firingArc, direction, range, hashSet2);
			HashSet<CustomGridNodeBase> hashSet3 = restrictedAreaComponent?.GetRestrictedArea(item, firingArc, vector3Direction);
			if (hashSet3 != null)
			{
				hashSet2.IntersectWith(hashSet3);
			}
			result.UnionWith(hashSet2);
		}
	}

	private static void GetFiringArcSourceNodes(ICollection<CustomGridNodeBase> result, CustomGridNodeBase starshipNode, int direction, IntRect sizeRect, RestrictedFiringArc firingArc, int offsetFromProw, int batteryWidth)
	{
		CustomGridGraph customGridGraph = (CustomGridGraph)starshipNode.Graph;
		Vector2Int coordinatesInGrid = starshipNode.CoordinatesInGrid;
		Vector2Int[] firingArcSourceNodesOffsets = GetFiringArcSourceNodesOffsets(direction, sizeRect, firingArc, offsetFromProw, batteryWidth);
		foreach (Vector2Int vector2Int in firingArcSourceNodesOffsets)
		{
			Vector2Int vector2Int2 = coordinatesInGrid + vector2Int;
			CustomGridNodeBase node = customGridGraph.GetNode(vector2Int2.x, vector2Int2.y);
			result.Add(node);
		}
	}

	private static Vector2Int[] GetFiringArcSourceNodesOffsets(int direction, IntRect sizeRect, RestrictedFiringArc firingArc, int offsetFromProw, int batteryWidth)
	{
		return Offsets.Get(new SourceNodesKey(firingArc, sizeRect, direction, offsetFromProw, batteryWidth));
	}

	private static Vector2Int GetNeighbourAlongDirection(Vector2Int node, int direction)
	{
		return node + direction switch
		{
			0 => new Vector2Int(0, -1), 
			1 => new Vector2Int(1, 0), 
			2 => new Vector2Int(0, 1), 
			3 => new Vector2Int(-1, 0), 
			4 => new Vector2Int(1, -1), 
			5 => new Vector2Int(1, 1), 
			6 => new Vector2Int(-1, 1), 
			7 => new Vector2Int(-1, -1), 
			_ => throw new ArgumentOutOfRangeException("direction", direction, null), 
		};
	}

	private static Vector2Int[] ArcSourceNodesImpl(SourceNodesKey key)
	{
		return (key.Arc switch
		{
			RestrictedFiringArc.Fore => GetForeArcSourceNodes(key.Direction, key.StarshipRect, key.Offset), 
			RestrictedFiringArc.Port => GetPortArcSourceNodes(key.Direction, key.StarshipRect, key.Offset, key.BatteryWidth), 
			RestrictedFiringArc.Starboard => GetStarboardArcSourceNodes(key.Direction, key.StarshipRect, key.Offset, key.BatteryWidth), 
			_ => throw new ArgumentOutOfRangeException("Arc", key.Arc, null), 
		}).ToArray();
	}

	public static IEnumerable<Vector2Int> GetForeArcSourceNodes(int direction, IntRect starshipRect, int offset = 0)
	{
		Vector2Int startNodeOffset = GetForeArcStartNodeOffset(direction, starshipRect);
		Vector2Int a = startNodeOffset;
		if (offset > 0)
		{
			int direction2 = (direction + 2) % 4 + direction / 4 * 4;
			for (int j = 0; j < offset; j++)
			{
				startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, direction2);
				if (CustomGraphHelper.GetWarhammerCellDistance(a, startNodeOffset) >= offset)
				{
					break;
				}
			}
		}
		yield return startNodeOffset;
		int neighbourDir;
		int i;
		if (direction < 4)
		{
			neighbourDir = (direction + 1) % 4;
			for (i = 1; i < starshipRect.Width; i++)
			{
				startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDir);
				yield return startNodeOffset;
			}
			yield break;
		}
		Vector2Int neighbour1 = startNodeOffset;
		Vector2Int neighbour2 = startNodeOffset;
		neighbourDir = (direction + 2) % 4;
		i = (neighbourDir + 1) % 4;
		for (i = 1; i < starshipRect.Width; i++)
		{
			neighbour1 = GetNeighbourAlongDirection(neighbour1, neighbourDir);
			neighbour2 = GetNeighbourAlongDirection(neighbour2, i);
			yield return neighbour1;
			yield return neighbour2;
		}
	}

	public static IEnumerable<Vector2Int> GetPortArcSourceNodes(int direction, IntRect starshipRect, int offset, int width)
	{
		if (offset >= starshipRect.Height)
		{
			yield break;
		}
		width = Math.Min((width == 0) ? int.MaxValue : width, starshipRect.Height - offset);
		Vector2Int startNodeOffset = GetPortArcStartNodeOffset(direction, starshipRect);
		int neighbourDir;
		if (direction < 4)
		{
			neighbourDir = (direction + 2) % 4;
			for (int j = 0; j < offset; j++)
			{
				startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDir);
			}
			for (int i = 0; i < width; i++)
			{
				yield return startNodeOffset;
				startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDir);
			}
			yield break;
		}
		int[] neighbourDirs = new int[2]
		{
			(direction + 3) % 4,
			(direction + 2) % 4
		};
		neighbourDir = 0;
		for (int k = 0; k < offset; k++)
		{
			startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDirs[neighbourDir]);
			neighbourDir = (neighbourDir + 1) % 2;
		}
		width = ((starshipRect.Height != 1) ? width : 0);
		for (int i = 0; i <= width; i++)
		{
			yield return startNodeOffset;
			startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDirs[neighbourDir]);
			neighbourDir = (neighbourDir + 1) % 2;
		}
	}

	public static IEnumerable<Vector2Int> GetStarboardArcSourceNodes(int direction, IntRect starshipRect, int offset, int width)
	{
		if (offset >= starshipRect.Height)
		{
			yield break;
		}
		width = Math.Min((width == 0) ? int.MaxValue : width, starshipRect.Height - offset);
		Vector2Int startNodeOffset = GetStarboardArcStartNodeOffset(direction, starshipRect);
		int neighbourDir;
		if (direction < 4)
		{
			neighbourDir = (direction + 2) % 4;
			for (int j = 0; j < offset; j++)
			{
				startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDir);
			}
			for (int i = 0; i < width; i++)
			{
				yield return startNodeOffset;
				startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDir);
			}
			yield break;
		}
		int[] neighbourDirs = new int[2]
		{
			(direction + 2) % 4,
			(direction + 3) % 4
		};
		neighbourDir = 0;
		for (int k = 0; k < offset; k++)
		{
			startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDirs[neighbourDir]);
			neighbourDir = (neighbourDir + 1) % 2;
		}
		width = ((starshipRect.Height != 1) ? width : 0);
		for (int i = 0; i <= width; i++)
		{
			yield return startNodeOffset;
			startNodeOffset = GetNeighbourAlongDirection(startNodeOffset, neighbourDirs[neighbourDir]);
			neighbourDir = (neighbourDir + 1) % 2;
		}
	}

	private static Vector2Int GetForeArcStartNodeOffset(int direction, IntRect starshipRect)
	{
		starshipRect.ymax = starshipRect.ymin + starshipRect.Width - 1;
		int x = starshipRect.xmin;
		int y = starshipRect.ymin;
		switch (direction)
		{
		case 1:
		case 4:
			x = starshipRect.xmax;
			break;
		case 3:
		case 6:
			y = starshipRect.ymax;
			break;
		case 2:
		case 5:
			x = starshipRect.xmax;
			y = starshipRect.ymax;
			break;
		}
		return new Vector2Int(x, y);
	}

	private static Vector2Int GetPortArcStartNodeOffset(int direction, IntRect starshipRect)
	{
		starshipRect.ymax = starshipRect.ymin + starshipRect.Width - 1;
		int x = starshipRect.xmin;
		int y = starshipRect.ymin;
		switch (direction)
		{
		case 0:
		case 7:
			x = starshipRect.xmax;
			break;
		case 2:
		case 5:
			y = starshipRect.ymax;
			break;
		case 1:
		case 4:
			x = starshipRect.xmax;
			y = starshipRect.ymax;
			break;
		}
		return new Vector2Int(x, y);
	}

	private static Vector2Int GetStarboardArcStartNodeOffset(int direction, IntRect starshipRect)
	{
		starshipRect.ymax = starshipRect.ymin + starshipRect.Width - 1;
		int x = starshipRect.xmin;
		int y = starshipRect.ymin;
		if (direction % 4 == 1)
		{
			x = starshipRect.xmax;
		}
		else if (direction % 4 == 3)
		{
			y = starshipRect.ymax;
		}
		else if (direction % 4 == 2)
		{
			x = starshipRect.xmax;
			y = starshipRect.ymax;
		}
		return new Vector2Int(x, y);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref Type);
		result.Append(ref OffsetFromProw);
		result.Append(ref BatteryWidth);
		Hash128 val2 = ClassHasher<AmmoSlot>.GetHash128(AmmoSlot);
		result.Append(ref val2);
		List<ItemEntityStarshipWeapon> arsenalWeapons = ArsenalWeapons;
		if (arsenalWeapons != null)
		{
			for (int i = 0; i < arsenalWeapons.Count; i++)
			{
				Hash128 val3 = ClassHasher<ItemEntityStarshipWeapon>.GetHash128(arsenalWeapons[i]);
				result.Append(ref val3);
			}
		}
		List<BlueprintStarshipAmmo> arsenalAmmo = ArsenalAmmo;
		if (arsenalAmmo != null)
		{
			for (int j = 0; j < arsenalAmmo.Count; j++)
			{
				Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(arsenalAmmo[j]);
				result.Append(ref val4);
			}
		}
		result.Append(ref m_ActiveWeaponIndex);
		return result;
	}
}
