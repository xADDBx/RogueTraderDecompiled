using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterBonesList : MonoBehaviour
{
	[InspectorReadOnly]
	public Transform[] Bones;

	public Transform GetByName(string n)
	{
		int index = CharacterBonesSetup.Instance.GetIndex(n);
		return Bones.Get(index);
	}

	public void UpdateCache(CharacterBonesSetup setup)
	{
		Bones = new Transform[setup.KnownTransformNames.Count];
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(base.transform);
		while (queue.Count > 0)
		{
			Transform transform = queue.Dequeue();
			int index = setup.GetIndex(transform.name);
			if (index >= 0)
			{
				Bones[index] = transform;
			}
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				queue.Enqueue(child);
			}
		}
	}
}
