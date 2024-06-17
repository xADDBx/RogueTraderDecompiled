using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.Visual.Mounts;

public class MountOffsets : MonoBehaviour
{
	private string m_OffsetsConfigGuid;

	private string m_MediumOffsetsConfigGuid;

	private string m_LargeOffsetsConfigGuid;

	private string m_HugeOffsetsConfigGuid;

	[SerializeField]
	public RaceMountOffsetsConfig OffsetsConfig;

	public RaceMountOffsetsConfig MediumOffsetsConfig;

	public RaceMountOffsetsConfig LargeOffsetsConfig;

	public RaceMountOffsetsConfig HugeOffsetsConfig;

	public Transform Root;

	public Transform PelvisIkTarget;

	public Transform LeftFootIkTarget;

	public Transform LeftKneeIkTarget;

	public Transform RightFootIkTarget;

	public Transform RightKneeIkTarget;

	public Transform Hands;

	public Transform SaddleRoot;

	public RaceMountOffsetsConfig.MountOffsetData GetMountOffsets(BlueprintRace race)
	{
		if (OffsetsConfig == null)
		{
			OffsetsConfig = ResourcesLibrary.TryGetResource<RaceMountOffsetsConfig>(m_OffsetsConfigGuid);
		}
		if (MediumOffsetsConfig == null)
		{
			MediumOffsetsConfig = ResourcesLibrary.TryGetResource<RaceMountOffsetsConfig>(m_MediumOffsetsConfigGuid);
		}
		if (LargeOffsetsConfig == null)
		{
			LargeOffsetsConfig = ResourcesLibrary.TryGetResource<RaceMountOffsetsConfig>(m_LargeOffsetsConfigGuid);
		}
		if (HugeOffsetsConfig == null)
		{
			HugeOffsetsConfig = ResourcesLibrary.TryGetResource<RaceMountOffsetsConfig>(m_HugeOffsetsConfigGuid);
		}
		if (OffsetsConfig == null || OffsetsConfig.offsets.Length == 0)
		{
			return null;
		}
		RaceMountOffsetsConfig.MountOffsetData[] offsets = OffsetsConfig.offsets;
		foreach (RaceMountOffsetsConfig.MountOffsetData mountOffsetData in offsets)
		{
			if (mountOffsetData == null || mountOffsetData.Races.Count <= 0)
			{
				continue;
			}
			foreach (BlueprintRaceReference race2 in mountOffsetData.Races)
			{
				if (race2.Is(race))
				{
					return mountOffsetData;
				}
			}
		}
		return null;
	}

	public void SetSizeConfig(Size mountSize)
	{
		if (mountSize == Size.Medium && MediumOffsetsConfig != null)
		{
			OffsetsConfig = MediumOffsetsConfig;
		}
		else if (mountSize == Size.Large && LargeOffsetsConfig != null)
		{
			OffsetsConfig = LargeOffsetsConfig;
		}
		else if (mountSize == Size.Huge && HugeOffsetsConfig != null)
		{
			OffsetsConfig = HugeOffsetsConfig;
		}
	}

	public bool ApplyConfigToMount(MountOffsets mount, RaceMountOffsetsConfig.MountOffsetData config)
	{
		mount.Root.localPosition = config.RootPosition;
		mount.SaddleRoot.localPosition = config.SaddleRootPosition;
		mount.SaddleRoot.localScale = config.SaddleRootScale;
		mount.PelvisIkTarget.localPosition = config.PelvisPosition;
		mount.PelvisIkTarget.localRotation = new Quaternion(config.PelvisRotation.x, config.PelvisRotation.y, config.PelvisRotation.z, config.PelvisRotation.w);
		mount.LeftFootIkTarget.localPosition = config.LeftFootPosition;
		mount.LeftFootIkTarget.localRotation = new Quaternion(config.LeftFootRotation.x, config.LeftFootRotation.y, config.LeftFootRotation.z, config.LeftFootRotation.w);
		mount.LeftKneeIkTarget.localPosition = config.LeftKneePosition;
		mount.RightFootIkTarget.localPosition = config.RightFootPosition;
		mount.RightFootIkTarget.localRotation = new Quaternion(config.RightFootRotation.x, config.RightFootRotation.y, config.RightFootRotation.z, config.RightFootRotation.w);
		mount.RightKneeIkTarget.localPosition = config.RightKneePosition;
		mount.Hands.localPosition = config.HandsPosition;
		return true;
	}
}
