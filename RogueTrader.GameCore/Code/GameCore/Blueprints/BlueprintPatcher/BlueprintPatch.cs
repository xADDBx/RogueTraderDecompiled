using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

[Serializable]
public class BlueprintPatch
{
	[SerializeField]
	public string TargetGuid;

	[SerializeField]
	public BlueprintFieldOverrideOperation[] FieldOverrides;

	[SerializeField]
	public BlueprintSimpleArrayPatchOperation[] ArrayPatches;

	[SerializeField]
	public BlueprintComponentsPatchOperation[] ComponentsPatches;

	[JsonIgnore]
	private List<BlueprintPatchOperation> patchingOperations;

	[JsonIgnore]
	public List<BlueprintPatchOperation> PatchingOperations
	{
		get
		{
			if (patchingOperations != null)
			{
				return patchingOperations;
			}
			patchingOperations = new List<BlueprintPatchOperation>();
			patchingOperations.AddRange(FieldOverrides);
			patchingOperations.AddRange(ArrayPatches);
			patchingOperations.AddRange(ComponentsPatches);
			foreach (BlueprintPatchOperation patchingOperation in patchingOperations)
			{
				patchingOperation.TargetGuid = TargetGuid;
			}
			return patchingOperations;
		}
	}

	public override string ToString()
	{
		string text = "BlueprintPatch info: \n target GUID " + TargetGuid;
		foreach (BlueprintPatchOperation patchingOperation in PatchingOperations)
		{
			text = text + "\n " + patchingOperation.ToString();
		}
		return text;
	}
}
