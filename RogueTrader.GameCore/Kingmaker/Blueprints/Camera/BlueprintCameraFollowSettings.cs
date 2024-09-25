using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[TypeId("fdd33cf2c5394802ae975acfd6de374d")]
public class BlueprintCameraFollowSettings : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintCameraFollowSettings>
	{
	}

	[Header("Phases")]
	public CameraFollowTaskParams NewTurn;

	public CameraFollowTaskParams ReadyToAttack;

	public CameraFollowTaskParams Targeting;

	public CameraFollowTaskParams Attacked;

	public CameraFollowTaskParams AttackOfOpportunity;

	public CameraFollowTaskParams CommandDidEnd;

	public CameraFollowTaskParams UnitDeath;
}
