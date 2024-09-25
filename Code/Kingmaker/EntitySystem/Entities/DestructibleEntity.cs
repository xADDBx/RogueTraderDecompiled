using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class DestructibleEntity : MapObjectEntity, PartStatsContainer.IOwner, IEntityPartOwner<PartStatsContainer>, IEntityPartOwner, PartHealth.IOwner, IEntityPartOwner<PartHealth>, PartDestructionStagesManager.IOwner, IEntityPartOwner<PartDestructionStagesManager>, IHashable
{
	public new BlueprintDestructibleObject Blueprint => (BlueprintDestructibleObject)base.Blueprint;

	public new DestructibleEntityView View => (DestructibleEntityView)base.View;

	public override ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy => ViewHandlingOnDisposePolicyType.Deactivate;

	public override bool CanBeAttackedDirectly => View.Or(null)?.CanBeAttackedDirectly ?? base.CanBeAttackedDirectly;

	public PartStatsContainer Stats => GetRequired<PartStatsContainer>();

	public PartHealth Health => GetRequired<PartHealth>();

	public PartDestructionStagesManager DestructionStages => GetRequired<PartDestructionStagesManager>();

	public SurfaceType SurfaceType => Blueprint.SurfaceType;

	public override IntRect SizeRect
	{
		get
		{
			Vector2 vector = View.Or(null)?.Bounds.size ?? default(Vector2);
			int num = Math.Max(0, Mathf.RoundToInt(vector.x / GraphParamsMechanicsCache.GridCellSize) - 1);
			int num2 = Math.Max(0, Mathf.RoundToInt(vector.y / GraphParamsMechanicsCache.GridCellSize) - 1);
			int num3 = ((num >= 2) ? (-(num / 2)) : 0);
			int num4 = ((num < 2) ? num : (num + num3));
			int num5 = ((num2 >= 2) ? (-(num2 / 2)) : 0);
			int num6 = ((num2 < 2) ? num2 : (num2 + num5));
			int val = num4 - num3;
			int val2 = num6 - num5;
			return new IntRect(0, 0, Math.Min(val, val2), Math.Max(val, val2));
		}
	}

	public override Vector3 Forward
	{
		get
		{
			Vector2 vector = View.Or(null)?.Bounds.size ?? default(Vector2);
			if (!(vector.x > vector.y))
			{
				return Vector3.back;
			}
			return Vector3.left;
		}
	}

	public override Vector3 Position
	{
		get
		{
			return (View.Or(null)?.Bounds ?? default(Rect)).min.To3D();
		}
		set
		{
			Rect rect = View.Or(null)?.Bounds ?? default(Rect);
			base.Position = value + (base.Position - rect.min.To3D());
		}
	}

	public DestructibleEntity(DestructibleEntityView view)
		: base(view.UniqueId, view.IsInGameBySettings, view.Blueprint)
	{
	}

	public DestructibleEntity(string uniqueId, bool isInGame, BlueprintDestructibleObject blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected DestructibleEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartStatsContainer>();
		GetOrCreate<PartHealth>();
		GetOrCreate<PartDestructionStagesManager>();
		Stats.Container.Register(StatType.DamageAbsorption);
		Stats.Container.Register(StatType.DamageDeflection);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		AddFact(BlueprintWarhammerRoot.Instance.CommonDestructibleEntityFact);
	}

	public override StatBaseValue GetStatBaseValue(StatType type)
	{
		return type switch
		{
			StatType.HitPoints => Blueprint.HitPoints, 
			StatType.WarhammerToughness => Blueprint.Toughness, 
			StatType.DamageDeflection => Blueprint.DamageDeflection, 
			StatType.DamageAbsorption => Blueprint.DamageAbsorption, 
			_ => base.GetStatBaseValue(type), 
		};
	}

	[CanBeNull]
	public static DestructibleEntity FindByNode(CustomGridNodeBase node)
	{
		return Game.Instance.State.DestructibleEntities.FirstOrDefault((DestructibleEntity i) => i.GetOccupiedNodes().Contains(node));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
