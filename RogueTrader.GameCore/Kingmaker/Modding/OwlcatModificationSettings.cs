using System;
using System.Collections.Generic;
using Kingmaker.BundlesLoading;
using UnityEngine;

namespace Kingmaker.Modding;

[Serializable]
public class OwlcatModificationSettings : ISerializationCallbackReceiver
{
	[Serializable]
	public class BlueprintChangeData
	{
		public string Guid;

		public string Filename;

		public BlueprintPatchType PatchType;
	}

	public enum BlueprintPatchType
	{
		Replace,
		Edit
	}

	public LocationList BundlesLayout = new LocationList();

	public DependencyData BundleDependencies = new DependencyData();

	public List<BlueprintChangeData> BlueprintPatches = new List<BlueprintChangeData>();

	public readonly Dictionary<string, string> BlueprintReplacementsDictionary = new Dictionary<string, string>();

	public readonly Dictionary<string, string> BlueprintPatchesDictionary = new Dictionary<string, string>();

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		BlueprintReplacementsDictionary.Clear();
		BlueprintPatchesDictionary.Clear();
		foreach (BlueprintChangeData blueprintPatch in BlueprintPatches)
		{
			switch (blueprintPatch.PatchType)
			{
			case BlueprintPatchType.Replace:
				BlueprintReplacementsDictionary[blueprintPatch.Guid] = blueprintPatch.Filename;
				break;
			case BlueprintPatchType.Edit:
				BlueprintPatchesDictionary[blueprintPatch.Guid] = blueprintPatch.Filename;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
