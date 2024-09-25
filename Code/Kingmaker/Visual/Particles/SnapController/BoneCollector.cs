using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Visual.Particles.Blueprints;
using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal readonly struct BoneCollector
{
	public struct Result
	{
		public FxBone rotationRootBone;

		public bool hasRotatableBones;
	}

	private readonly SnapMapBase m_SnapMap;

	private readonly List<string> m_BoneNames;

	private readonly ReferenceArrayProxy<BlueprintFxLocatorGroup> m_LocatorGroups;

	private readonly bool m_IgnoreSpecialBones;

	private readonly bool m_IgnoreRotatableBones;

	private readonly bool m_IgnoreNonRotatableBones;

	private readonly bool m_FindRotationRootBone;

	private readonly string m_RotationRootBoneName;

	private readonly bool m_UseRandomBonesAmount;

	private readonly float m_RandomPercent;

	public BoneCollector([NotNull] SnapMapBase snapMap, List<string> boneNames, ReferenceArrayProxy<BlueprintFxLocatorGroup> locatorGroups, bool ignoreSpecialBones, bool ignoreRotatableBones, bool ignoreNonRotatableBones, string rotationRootBoneName, bool useRandomBonesAmount = false, float randomPercent = 1f)
	{
		if (snapMap == null)
		{
			throw new ArgumentNullException("snapMap");
		}
		m_SnapMap = snapMap;
		m_BoneNames = boneNames;
		m_LocatorGroups = locatorGroups;
		m_IgnoreSpecialBones = ignoreSpecialBones;
		m_IgnoreRotatableBones = ignoreRotatableBones;
		m_IgnoreNonRotatableBones = ignoreNonRotatableBones;
		m_FindRotationRootBone = !string.IsNullOrEmpty(rotationRootBoneName);
		m_RotationRootBoneName = rotationRootBoneName;
		m_UseRandomBonesAmount = useRandomBonesAmount;
		m_RandomPercent = randomPercent;
	}

	public Result Collect(ICollection<FxBone> results)
	{
		results.Clear();
		Result collectionResult = default(Result);
		if (m_BoneNames != null)
		{
			foreach (string boneName in m_BoneNames)
			{
				CollectBone(m_SnapMap[boneName], FxBoneFlags.Disabled, (FxBoneFlags)0, results, ref collectionResult);
			}
		}
		foreach (BlueprintFxLocatorGroup locatorGroup in m_LocatorGroups)
		{
			IReadOnlyList<FxBone> locators = m_SnapMap.GetLocators(locatorGroup);
			if (locators == null)
			{
				continue;
			}
			foreach (FxBone item in locators)
			{
				CollectBone(item, FxBoneFlags.Disabled, (FxBoneFlags)0, results, ref collectionResult);
			}
		}
		if (!m_IgnoreSpecialBones && m_SnapMap.Bones != null)
		{
			FxBoneFlags boneFlagsFilterMask = FxBoneFlags.Special | FxBoneFlags.Disabled;
			FxBoneFlags boneFlagsFilter = FxBoneFlags.Special;
			foreach (FxBone bone in m_SnapMap.Bones)
			{
				CollectBone(bone, boneFlagsFilterMask, boneFlagsFilter, results, ref collectionResult);
			}
		}
		if (m_UseRandomBonesAmount && results.Count > 1)
		{
			int amount = Mathf.CeilToInt((float)results.Count * m_RandomPercent);
			GetRandomBones(ref results, amount);
			if (collectionResult.hasRotatableBones && !results.Contains(collectionResult.rotationRootBone))
			{
				results.Add(collectionResult.rotationRootBone);
			}
		}
		return collectionResult;
	}

	private void GetRandomBones<T>(ref ICollection<T> result, int amount)
	{
		if (amount < result.Count)
		{
			int num = result.Count - amount;
			for (int i = 0; i < num; i++)
			{
				int index = UnityEngine.Random.Range(0, result.Count);
				T item = result.ElementAt(index);
				result.Remove(item);
			}
		}
	}

	private void CollectBone([CanBeNull] FxBone bone, FxBoneFlags boneFlagsFilterMask, FxBoneFlags boneFlagsFilter, ICollection<FxBone> results, ref Result collectionResult)
	{
		if (bone == null || (bone.Flags & boneFlagsFilterMask) != boneFlagsFilter)
		{
			return;
		}
		if (bone.Rotate)
		{
			collectionResult.hasRotatableBones = true;
			if (m_IgnoreRotatableBones)
			{
				return;
			}
		}
		else if (m_IgnoreNonRotatableBones)
		{
			return;
		}
		if (m_FindRotationRootBone && string.Equals(bone.Name, m_RotationRootBoneName, StringComparison.Ordinal))
		{
			collectionResult.rotationRootBone = bone;
		}
		results.Add(bone);
	}
}
