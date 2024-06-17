using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using UnityEngine;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

[Serializable]
public class BlueprintComponentsPatchOperation : BlueprintPatchOperation
{
	[SerializeField]
	public string FieldValue;

	[SerializeField]
	public BlueprintPatchOperationType OperationType;

	public override void Apply(SimpleBlueprint bp)
	{
		BlueprintScriptableObject blueprintScriptableObject = (BlueprintScriptableObject)bp;
		BlueprintComponentPatchData blueprintComponentPatchData = JsonConvert.DeserializeObject<BlueprintComponentPatchData>(FieldValue, BlueprintPatcher.Settings);
		if (blueprintComponentPatchData == null)
		{
			PFLog.Mods.Error("Blueprint component patch failed to deserialize BlueprintComponentPatchData from " + FieldValue);
			return;
		}
		BlueprintComponent newComponent = blueprintComponentPatchData.ComponentValue;
		List<BlueprintComponent> list = blueprintScriptableObject.ComponentsArray.ToList();
		switch (OperationType)
		{
		case BlueprintPatchOperationType.InsertLast:
			PFLog.Mods.Log("Patching " + TargetGuid + " with AddComponent " + newComponent.name);
			list.Add(newComponent);
			blueprintScriptableObject.ComponentsArray = list.ToArray();
			break;
		case BlueprintPatchOperationType.RemoveElement:
			PFLog.Mods.Log("Patching " + TargetGuid + " with RemoveComponent " + newComponent.name);
			if (list.Any((BlueprintComponent x) => x.name == newComponent.name))
			{
				PFLog.Mods.Log("Component " + newComponent.name + " exists on " + TargetGuid + ", removing.");
			}
			else
			{
				PFLog.Mods.Log("Trying to remove component " + newComponent.name + " which doesn't exits on " + TargetGuid + " ");
			}
			list.Remove((BlueprintComponent x) => x.name == newComponent.name);
			blueprintScriptableObject.ComponentsArray = list.ToArray();
			break;
		default:
			PFLog.Mods.Error($"{OperationType} is not applicable to Components patch operation");
			break;
		}
	}

	public override string ToString()
	{
		return base.ToString() + " \n BlueprintComponentPatchOperation: type " + OperationType.ToString() + ", \n value: " + FieldValue + " ";
	}
}
