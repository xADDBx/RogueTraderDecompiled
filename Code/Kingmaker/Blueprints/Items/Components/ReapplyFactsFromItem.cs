using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Serializable]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("ee483046b4df4eda8e96cd762bbdf594")]
public class ReapplyFactsFromItem : EntityFactComponentDelegate<ItemEntity>, IHashable
{
	private class ComponentData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int Version = -1;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref Version);
			return result;
		}
	}

	public int Version;

	protected override void OnApplyPostLoadFixes()
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		if (Version > componentData.Version)
		{
			base.Owner.ReapplyFactsForWielder();
			componentData.Version = Version;
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
