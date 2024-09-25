using Kingmaker.Code.View.Mechanics.Entities.Covers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class CoverEntity : DestructibleEntity, PartCover.IOwner, IEntityPartOwner<PartCover>, IEntityPartOwner, IHashable
{
	public new CoverEntityView View => (CoverEntityView)base.View;

	public PartCover Cover => GetRequired<PartCover>();

	protected CoverEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public CoverEntity(string uniqueId, bool isInGame, BlueprintDestructibleObject blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartCover>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
