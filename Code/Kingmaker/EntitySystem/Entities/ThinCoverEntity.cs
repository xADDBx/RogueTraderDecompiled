using System;
using System.Linq;
using Kingmaker.Code.View.Mechanics.Entities.Covers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class ThinCoverEntity : DestructibleEntity, PartCover.IOwner, IEntityPartOwner<PartCover>, IEntityPartOwner, IHashable
{
	public new ThinCoverEntityView View => (ThinCoverEntityView)base.View;

	public PartCover Cover => GetRequired<PartCover>();

	protected ThinCoverEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public ThinCoverEntity(string uniqueId, bool isInGame, BlueprintDestructibleObject blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartCover>();
	}

	public bool IsCoverBetween(CustomGridNodeBase node1, CustomGridNodeBase node2)
	{
		Rect rect = View.Or(null)?.Bounds ?? default(Rect);
		bool num = rect.width > rect.height;
		Vector2 vector = node1.Vector3Position.To2D();
		Vector2 vector2 = node2.Vector3Position.To2D();
		Vector2 vector3 = vector - vector2;
		if (!num)
		{
			if (Math.Abs(vector3.x) > 0.5f)
			{
				if (!(vector.y < rect.yMax) || !(vector.y > rect.yMin))
				{
					if (vector2.y < rect.yMax)
					{
						return vector2.y > rect.yMin;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		if (Math.Abs(vector3.y) > 0.5f)
		{
			if (!(vector.x < rect.xMax) || !(vector.x > rect.xMin))
			{
				if (vector2.x < rect.xMax)
				{
					return vector2.x > rect.xMin;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static DestructibleEntity FindThinCover(CustomGridNodeBase node, int coverDirection)
	{
		CustomGridNodeBase other = node.GetNeighbourAlongDirection(coverDirection, checkConnectivity: false);
		return Game.Instance.State.DestructibleEntities.OfType<ThinCoverEntity>().FirstOrDefault((ThinCoverEntity i) => i.IsCoverBetween(node, other));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
