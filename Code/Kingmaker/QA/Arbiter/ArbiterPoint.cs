using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

[TypeId("c5df78d16039466a9c6a8466a9847270")]
public class ArbiterPoint : ArbiterElement
{
	public Vector3 Position;

	public float Rotation;

	public float Zoom;

	public override string GetCaption()
	{
		int num = BlueprintComponentExtendAsObject.Or(SimpleBlueprintExtendAsObject.Or(base.Owner as BlueprintArbiterInstruction, null)?.Test as AreaCheckerComponent, null)?.GetSampleId(this) ?? (-1);
		return $"Point {num}: {Position}";
	}
}
