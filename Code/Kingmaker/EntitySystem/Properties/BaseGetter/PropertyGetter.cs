using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.BaseGetter;

[Serializable]
[TypeId("36fbb8639be34e619c481ac1adc134e2")]
public abstract class PropertyGetter : Element
{
	public PropertyGetterSettings Settings;

	protected virtual Type RequiredCurrentEntityType => typeof(Entity);

	public bool IsCurrentEntityHasRequiredType => RequiredCurrentEntityType.IsInstanceOfType(CurrentEntity);

	protected PropertyContext PropertyContext => (ContextData<PropertyContextData>.Current ?? throw new Exception("PropertyContextData is missing")).Context;

	[NotNull]
	protected MechanicEntity CurrentEntity => ContextData<PropertyContextData>.Current?.Context.CurrentEntity ?? throw new Exception("PropertyContextData is missing");

	public virtual bool AddBracketsAroundInnerCaption => true;

	protected abstract int GetBaseValue();

	protected abstract string GetInnerCaption();

	public sealed override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		if (Settings.Limit == PropertyGetterSettings.LimitType.Min)
		{
			builder.Append("Max(");
			builder.Append(Settings.Min);
			builder.Append(", ");
		}
		if (Settings.Limit == PropertyGetterSettings.LimitType.Max)
		{
			builder.Append("Min(");
			builder.Append(Settings.Max);
			builder.Append(", ");
		}
		if (Settings.Limit == PropertyGetterSettings.LimitType.MinMax)
		{
			builder.Append("Clamp(");
			builder.Append(Settings.Min);
			builder.Append(", ");
			builder.Append(Settings.Max);
			builder.Append(", ");
		}
		if (Settings.Negate)
		{
			builder.Append('-');
		}
		if (Settings.Progression != 0)
		{
			builder.Append(Settings.Progression);
		}
		if (AddBracketsAroundInnerCaption)
		{
			builder.Append('[');
		}
		builder.Append(GetInnerCaption());
		if (AddBracketsAroundInnerCaption)
		{
			builder.Append(']');
		}
		if (Settings.Limit != 0)
		{
			builder.Append(')');
		}
		return builder.ToString();
	}

	public int GetValue()
	{
		if (!IsCurrentEntityHasRequiredType)
		{
			return 0;
		}
		try
		{
			int baseValue = GetBaseValue();
			return Settings?.Apply(baseValue) ?? baseValue;
		}
		catch (Exception innerException)
		{
			throw new PropertyGetterException(this, innerException);
		}
	}
}
[TypeId("36fbb8639be34e619c481ac1adc134e2")]
public abstract class PropertyGetter<TEntity> : PropertyGetter where TEntity : MechanicEntity
{
	protected override Type RequiredCurrentEntityType => typeof(TEntity);

	protected new TEntity CurrentEntity => (TEntity)base.CurrentEntity;
}
