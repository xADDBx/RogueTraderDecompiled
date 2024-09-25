using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintDestructibleObject))]
[TypeId("6a496c8a07bf4a05b17e9d5e3afdc886")]
public class ReapplyStats : MechanicEntityFactComponentDelegate, IHashable
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
			base.Owner.GetStatsContainerOptional()?.Container.ReinitializeBaseValues();
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
