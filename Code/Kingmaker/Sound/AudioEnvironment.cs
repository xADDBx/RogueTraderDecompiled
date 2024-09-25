using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Sound;

[RequireComponent(typeof(AudioZone))]
public class AudioEnvironment : MonoBehaviour
{
	public class CompareByPriorityComparer : IComparer<AudioEnvironment>
	{
		public virtual int Compare(AudioEnvironment a, AudioEnvironment b)
		{
			int num = a.Priority.CompareTo(b.Priority);
			if (num != 0 || !(a != b))
			{
				return num;
			}
			return 1;
		}
	}

	public class CompareBySelectionAlgorithmComparer : CompareByPriorityComparer
	{
		public override int Compare(AudioEnvironment a, AudioEnvironment b)
		{
			if (a.IsDefault)
			{
				if (!b.IsDefault)
				{
					return 1;
				}
				return base.Compare(a, b);
			}
			if (b.IsDefault)
			{
				return -1;
			}
			if (a.ExcludeOthers)
			{
				if (!b.ExcludeOthers)
				{
					return -1;
				}
				return base.Compare(a, b);
			}
			if (!b.ExcludeOthers)
			{
				return base.Compare(a, b);
			}
			return 1;
		}
	}

	public static CompareBySelectionAlgorithmComparer Comparer = new CompareBySelectionAlgorithmComparer();

	public bool ExcludeOthers;

	public bool IsDefault;

	public AkAuxBusReference Bus;

	public int Priority;

	public float GetAuxSendValueForPosition(Vector3 in_position)
	{
		return 1f;
	}
}
