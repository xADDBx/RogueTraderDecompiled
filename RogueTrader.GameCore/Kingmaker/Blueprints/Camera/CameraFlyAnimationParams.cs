using System;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[Serializable]
public class CameraFlyAnimationParams
{
	[Tooltip("Максимальное время, за которое камера должна долететь до цели")]
	public float MaxTime;

	[Tooltip("Скорость камеры считается автоматически, в зависимости от расстояния и времени")]
	public bool AutoSpeed;

	[HideIf("AutoSpeed")]
	[Tooltip("Максимальная скорость, с которой камера должна долететь до цели")]
	public float MaxSpeed;

	[HideIf("AutoSpeed")]
	[Tooltip("Скорость, с которой камера должна долететь до цели")]
	public float Speed;

	[Tooltip("Анимационная кривая скорости полёта камеры")]
	public AnimationCurve AnimationCurve;
}
