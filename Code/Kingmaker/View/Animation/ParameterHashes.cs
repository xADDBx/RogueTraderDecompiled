using System;
using System.Linq;
using UnityEngine;

namespace Kingmaker.View.Animation;

public static class ParameterHashes
{
	public static readonly int[] FlagHashes;

	public static readonly int[] FloatHashes;

	public static readonly int[] TriggerHashes;

	public static readonly int[] IntHashes;

	private static int[] GetHashes<T>()
	{
		return (from T v in Enum.GetValues(typeof(T))
			select v.ToString() into s
			select Animator.StringToHash(s)).ToArray();
	}

	static ParameterHashes()
	{
		FlagHashes = GetHashes<AnimationFlagType>();
		FloatHashes = GetHashes<AnimationFloatType>();
		TriggerHashes = GetHashes<AnimationTriggerType>();
		IntHashes = GetHashes<AnimationIntType>();
	}

	public static int Get(AnimationFlagType v)
	{
		return FlagHashes[(int)v];
	}

	public static int Get(AnimationFloatType v)
	{
		return FloatHashes[(int)v];
	}

	public static int Get(AnimationTriggerType v)
	{
		return TriggerHashes[(int)v];
	}

	public static int Get(AnimationIntType v)
	{
		return IntHashes[(int)v];
	}
}
