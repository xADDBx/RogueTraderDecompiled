using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f76fb89c2e514ffeb5b5ecf695390890")]
public class AddLocalMapMarker : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<MechanicEntity, AddLocalMapMarker>, ILocalMapMarker, IHashable
	{
		protected override void OnActivateOrPostLoad()
		{
			base.OnActivateOrPostLoad();
			LocalMapModel.Markers.Add(this);
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			LocalMapModel.Markers.Remove(this);
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			LocalMapModel.Markers.Remove(this);
		}

		public LocalMapMarkType GetMarkerType()
		{
			return base.Settings.Type;
		}

		public string GetDescription()
		{
			return base.Owner?.GetDescriptionOptional()?.Name ?? "";
		}

		public Vector3 GetPosition()
		{
			return base.Owner?.Position ?? Vector3.zero;
		}

		public bool IsVisible()
		{
			bool num = base.Owner?.IsInGame ?? false;
			bool flag = base.Owner?.IsRevealed ?? false;
			bool flag2 = base.Owner?.IsDeadOrUnconscious ?? false;
			if (num && (flag || base.Settings.ShowIfNotRevealed))
			{
				return !flag2;
			}
			return false;
		}

		public bool IsMapObject()
		{
			return false;
		}

		public Entity GetEntity()
		{
			return base.Owner;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public LocalMapMarkType Type = LocalMapMarkType.VeryImportantThing;

	public bool ShowIfNotRevealed;

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}
}
