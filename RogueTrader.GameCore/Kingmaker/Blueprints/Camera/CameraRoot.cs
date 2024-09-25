using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[TypeId("c502cb732e5f4cd0ad2ce2c24b48c82c")]
public class CameraRoot : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<CameraRoot>
	{
	}

	[SerializeField]
	public BlueprintCameraSettings.Reference StarSystemMapSettings;

	[SerializeField]
	public BlueprintCameraSettings.Reference GroundMapSettings;

	[SerializeField]
	public BlueprintCameraSettings.Reference GlobalMapSettings;

	[SerializeField]
	public BlueprintCameraSettings.Reference SpaceCombatSettings;
}
