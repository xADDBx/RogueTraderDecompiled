using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.EntitySystem.Properties;

public class PropertyContextPreviewCasterData : ContextData<PropertyContextPreviewCasterData>
{
	[CanBeNull]
	public MechanicEntity Caster { get; private set; }

	public PropertyContextPreviewCasterData Setup([CanBeNull] MechanicEntity caster)
	{
		Caster = caster;
		return this;
	}

	protected override void Reset()
	{
		Caster = null;
	}
}
