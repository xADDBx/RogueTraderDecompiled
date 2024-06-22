using CatmullRomSplines;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[RequireComponent(typeof(VectorSpline))]
[KnowledgeDatabaseID("4c9d90daff9d5a84ab92821564bbaad2")]
public class CutscenePath : EntityViewBase
{
	public override bool CreatesDataOnLoad => true;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CutscenePathEntity(this));
	}
}
