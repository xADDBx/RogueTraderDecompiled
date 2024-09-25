using Kingmaker.EntitySystem.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

public class ViewBasedPart : EntityPart<Entity>, IHashable
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string SourceType { get; private set; }

	public IAbstractEntityPartComponent Source { get; private set; }

	public IEntityViewBase View => base.Owner.View;

	public virtual bool ShouldCheckSourceComponent => true;

	public virtual void SetSource(IAbstractEntityPartComponent source)
	{
		Source = source;
		SourceType = source.GetType().Name;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(SourceType);
		return result;
	}
}
public abstract class ViewBasedPart<TSettings> : ViewBasedPart, IHashable where TSettings : class, new()
{
	public TSettings Settings { get; private set; } = new TSettings();


	public override void SetSource(IAbstractEntityPartComponent source)
	{
		IAbstractEntityPartComponent source2 = base.Source;
		base.SetSource(source);
		Settings = source.GetSettings() as TSettings;
		OnSettingsDidSet(source2 != source);
	}

	protected virtual void OnSettingsDidSet(bool isNewSettings)
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
