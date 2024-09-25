using Kingmaker.Blueprints.Camera;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public struct CameraFollowTaskContext
{
	public TargetWrapper Owner;

	public Vector3? Position;

	public int? Priority;

	public bool IsMelee;

	public CameraFollowTaskParams Params;

	public BlueprintActionCameraSettings ActionCameraSettings;

	public AbstractUnitEntity Caster;

	public AbstractUnitEntity Target;

	public float Time;
}
