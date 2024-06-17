using Kingmaker.View.Animation;
using UnityEngine;
using UnityEngine.Animations;

namespace Kingmaker.View;

public static class UnitAnimatorExtension
{
	public static void SetFlag(this Animator animtator, AnimationFlagType p, bool v)
	{
		if ((bool)animtator && (bool)animtator.runtimeAnimatorController)
		{
			animtator.SetBool(ParameterHashes.Get(p), v);
		}
	}

	public static void SetFloat(this Animator animtator, AnimationFloatType p, float v)
	{
		if ((bool)animtator && (bool)animtator.runtimeAnimatorController)
		{
			animtator.SetFloat(ParameterHashes.Get(p), v);
		}
	}

	public static void SetInt(this Animator animtator, AnimationIntType p, int v)
	{
		if ((bool)animtator && (bool)animtator.runtimeAnimatorController)
		{
			animtator.SetInteger(ParameterHashes.Get(p), v);
		}
	}

	public static void SetTrigger(this Animator animtator, AnimationTriggerType p)
	{
		if ((bool)animtator && (bool)animtator.runtimeAnimatorController)
		{
			animtator.SetTrigger(ParameterHashes.Get(p));
		}
	}

	public static void SetFlag(this AnimatorControllerPlayable animtator, AnimationFlagType p, bool v)
	{
		animtator.SetBool(ParameterHashes.Get(p), v);
	}

	public static void SetFloat(this AnimatorControllerPlayable animtator, AnimationFloatType p, float v)
	{
		animtator.SetFloat(ParameterHashes.Get(p), v);
	}

	public static void SetInt(this AnimatorControllerPlayable animtator, AnimationIntType p, int v)
	{
		animtator.SetInteger(ParameterHashes.Get(p), v);
	}

	public static void SetTrigger(this AnimatorControllerPlayable animtator, AnimationTriggerType p)
	{
		animtator.SetTrigger(ParameterHashes.Get(p));
	}
}
