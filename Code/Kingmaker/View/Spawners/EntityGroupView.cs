using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("9e19d847eaec460999dc26bf3ff2020d")]
public class EntityGroupView : EntityViewBase
{
	public class UnitGroupData : SimpleEntity, IHashable
	{
		public UnitGroupData(EntityViewBase view)
			: base(view)
		{
		}

		protected UnitGroupData(JsonConstructorMark _)
			: base(_)
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public Color GizmosColor = Color.red;

	public override bool CreatesDataOnLoad => true;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new UnitGroupData(this));
	}
}
