using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("120df4726e71c854e95f84b87a99a3c5")]
public class ContextActionSpawnFx : ContextAction
{
	public PrefabLink PrefabLink;

	public override string GetCaption()
	{
		GameObject gameObject = PrefabLink.Load();
		return "Spawn FX: " + ((gameObject != null) ? gameObject.name : "unspecified");
	}

	protected override void RunAction()
	{
		if (!base.Context.DisableFx)
		{
			GameObject prefab = PrefabLink.Load();
			if (base.Target.Entity != null)
			{
				FxHelper.SpawnFxOnEntity(prefab, base.Target.Entity.View);
			}
			else
			{
				FxHelper.SpawnFxOnPoint(prefab, base.Target.Point);
			}
		}
	}
}
