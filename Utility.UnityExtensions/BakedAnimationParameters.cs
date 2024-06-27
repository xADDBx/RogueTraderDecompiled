using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "BakedAnimParams", menuName = "ScriptableObjects/BakedAnimationParameters", order = 1)]
public class BakedAnimationParameters : ScriptableObject
{
	[SerializeField]
	public List<ObjectAnimationHolder> ObjectAnimHolder;

	[HideInInspector]
	public float ImpulsePower;
}
