using System;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[Serializable]
public class CameraFollowTaskParams
{
	[Tooltip("Дефолтные параметры")]
	public CameraFollowTaskParamsEntry DefaultParams;

	public bool HasMeleeParams;

	[ShowIf("HasMeleeParams")]
	[Tooltip("Параметры при атаке оружием ближнего боя")]
	public CameraFollowTaskParamsEntry MeleeParams;
}
