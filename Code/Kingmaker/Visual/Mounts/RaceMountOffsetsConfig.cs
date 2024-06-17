using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks.BaseInterfaces;
using UnityEngine;

namespace Kingmaker.Visual.Mounts;

[CreateAssetMenu(fileName = "RaceMountOffsetsConfig", menuName = "Techart/Race Mount Offsets Config")]
public class RaceMountOffsetsConfig : ScriptableObject, IResource
{
	[Serializable]
	public class MountOffsetData
	{
		public List<BlueprintRaceReference> Races;

		public Vector3 RootPosition;

		public Vector3 SaddleRootPosition;

		public Vector3 SaddleRootScale = Vector3.one;

		public Vector4 SaddleRootRotation;

		public Vector3 PelvisPosition;

		public Vector4 PelvisRotation;

		public Vector3 LeftFootPosition;

		public Vector4 LeftFootRotation;

		public Vector3 LeftKneePosition;

		public Vector3 RightFootPosition;

		public Vector4 RightFootRotation;

		public Vector3 RightKneePosition;

		public Vector3 HandsPosition;
	}

	[SerializeField]
	public MountOffsetData[] offsets;
}
