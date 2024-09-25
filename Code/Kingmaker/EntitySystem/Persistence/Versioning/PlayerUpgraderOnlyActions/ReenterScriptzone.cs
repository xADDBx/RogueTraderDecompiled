using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[PlayerUpgraderAllowed(true)]
[TypeId("d2c5e93d90af42e4596df62ff9e55d24")]
public class ReenterScriptzone : PlayerUpgraderOnlyAction
{
	[AllowedEntityType(typeof(ScriptZone))]
	[ValidateNotEmpty]
	public EntityReference m_ScriptZone;

	public override string GetCaption()
	{
		return $"Reset units in {m_ScriptZone} scriptzone";
	}

	protected override void RunActionOverride()
	{
		if (m_ScriptZone.FindData() is ScriptZoneEntity scriptZoneEntity)
		{
			scriptZoneEntity.RemoveAll();
		}
	}
}
