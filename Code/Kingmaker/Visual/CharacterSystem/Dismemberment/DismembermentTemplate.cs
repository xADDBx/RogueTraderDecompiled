using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

public class DismembermentTemplate : ScriptableObject
{
	[Serializable]
	public class DismembermentTemplateBone
	{
		public string TransformName;

		public float SliceOffset;

		public Vector3 SliceOrientationEuler;
	}

	[Serializable]
	public class DismembermentTemplateSet
	{
		public DismembermentLimbsApartType Type;

		public List<DismembermentTemplateBone> SliceBones = new List<DismembermentTemplateBone>();
	}

	public List<DismembermentTemplateSet> Sets = new List<DismembermentTemplateSet>();
}
