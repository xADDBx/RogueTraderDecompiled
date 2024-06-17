using CatmullRomSplines;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[RequireComponent(typeof(VectorSpline))]
public class CutscenePath : EntityViewBase
{
	public override bool CreatesDataOnLoad => true;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CutscenePathEntity(this));
	}
}
