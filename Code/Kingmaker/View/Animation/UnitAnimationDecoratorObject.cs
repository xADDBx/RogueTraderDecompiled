using System;
using Kingmaker.Blueprints.Base;
using UnityEngine;

namespace Kingmaker.View.Animation;

[Serializable]
[CreateAssetMenu(menuName = "Character System/Decorator Object")]
public class UnitAnimationDecoratorObject : ScriptableObject
{
	public bool UseGender;

	public Gender gender = Gender.Female;

	[AnimationDuration(30)]
	public float Duration;

	public DecoratorEntry[] Entries;
}
