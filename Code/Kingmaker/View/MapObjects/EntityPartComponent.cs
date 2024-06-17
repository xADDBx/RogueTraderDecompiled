using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public abstract class EntityPartComponent<TPart> : AbstractEntityPartComponent where TPart : ViewBasedPart, new()
{
	public sealed override void EnsureEntityPart()
	{
		EntityViewBase component = GetComponent<EntityViewBase>();
		EntityViewBase entityViewBase = component.Or(null);
		TPart obj = (((object)entityViewBase != null) ? entityViewBase.Data.ToEntity().GetOrCreate<TPart>() : null);
		obj?.SetSource(this);
		if (obj == null)
		{
			PFLog.Default.Warning(this, $"Could not create part: {this}. Owner: {component} data {component.Or(null)?.Data != null}");
		}
	}

	public override object GetSettings()
	{
		return null;
	}
}
public abstract class EntityPartComponent<TPart, TSettings> : AbstractEntityPartComponent where TPart : ViewBasedPart, new() where TSettings : class
{
	[SerializeField]
	public TSettings Settings;

	public override object GetSettings()
	{
		return Settings;
	}

	public sealed override void EnsureEntityPart()
	{
		EntityViewBase entityViewBase = GetComponent<EntityViewBase>().Or(null);
		(((object)entityViewBase != null) ? entityViewBase.Data.ToEntity().GetOrCreate<TPart>() : null)?.SetSource(this);
	}

	public TPart EnsurePart()
	{
		EntityViewBase entityViewBase = GetComponent<EntityViewBase>().Or(null);
		if ((object)entityViewBase == null)
		{
			return null;
		}
		return entityViewBase.Data.ToEntity().GetOrCreate<TPart>();
	}
}
