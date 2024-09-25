using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class LocalMapMarkerPart : ViewBasedPart<LocalMapMarkerSettings>, ILocalMapMarker, IHashable
{
	[JsonProperty]
	public bool Hidden { get; private set; }

	public string NonLocalizedDescription { get; set; }

	public override bool ShouldCheckSourceComponent
	{
		get
		{
			if (base.ShouldCheckSourceComponent)
			{
				return !IsRuntimeCreated;
			}
			return false;
		}
	}

	public bool IsRuntimeCreated { get; set; }

	public LocalMapMarkType GetMarkerType()
	{
		return base.Settings.Type;
	}

	public string GetDescription()
	{
		if (base.Settings.DescriptionUnit != null)
		{
			return base.Settings.DescriptionUnit.CharacterName;
		}
		if (base.Settings.Description != null)
		{
			return base.Settings.Description.String;
		}
		return NonLocalizedDescription;
	}

	public Vector3 GetPosition()
	{
		return ((EntityViewBase)base.View).Or(null)?.ViewTransform.position ?? Vector3.zero;
	}

	bool ILocalMapMarker.IsMapObject()
	{
		return base.View is MapObjectView;
	}

	public Entity GetEntity()
	{
		return base.Owner;
	}

	bool ILocalMapMarker.IsVisible()
	{
		if (Hidden)
		{
			return false;
		}
		if (base.Owner.IsRevealed && base.Owner.IsInGame)
		{
			return ((MapObjectEntity)base.Owner).IsAwarenessCheckPassed;
		}
		return false;
	}

	public void SetHidden(bool v)
	{
		Hidden = v;
	}

	protected override void OnAttachOrPostLoad()
	{
		LocalMapModel.Markers.Add(this);
	}

	protected override void OnDetach()
	{
		LocalMapModel.Markers.Remove(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Hidden;
		result.Append(ref val2);
		return result;
	}
}
