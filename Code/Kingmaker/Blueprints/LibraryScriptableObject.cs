using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints;

public class LibraryScriptableObject : ScriptableObject
{
	[SerializeField]
	private List<BlueprintScriptableObject> m_AllBlueprints;

	[SerializeField]
	private List<ElementsScriptableObject> m_AllScriptables;

	[SerializeField]
	private BlueprintRoot m_Root;

	private bool m_Initialized;

	public BlueprintRoot Root => m_Root;

	[CanBeNull]
	public Dictionary<string, BlueprintScriptableObject> BlueprintsByAssetId { get; private set; }

	[CanBeNull]
	public Dictionary<string, ElementsScriptableObject> ScriptablesByAssetId { get; private set; }

	public List<BlueprintScriptableObject> GetAllBlueprints()
	{
		return m_AllBlueprints;
	}

	public void BuildLibrary(bool buildLibraryBundle, bool includeTests = false)
	{
		using (new BlueprintValidationHelper.Disable())
		{
			CollectScriptables();
			CollectBlueprints(buildLibraryBundle, includeTests);
		}
	}

	private void CollectBlueprints(bool buildLibraryBundle, bool includeTests = false)
	{
		m_AllBlueprints = AllBlueprints(includeTests);
		m_Initialized = false;
		LoadDictionary();
	}

	private void CollectScriptables()
	{
	}

	public static List<BlueprintScriptableObject> AllBlueprints(bool includeTests)
	{
		return new List<BlueprintScriptableObject>();
	}

	public void Clear()
	{
		m_AllBlueprints.Clear();
	}

	public void LoadDictionary()
	{
		if (m_Initialized && BlueprintsByAssetId != null)
		{
			return;
		}
		BlueprintsByAssetId = new Dictionary<string, BlueprintScriptableObject>();
		foreach (BlueprintScriptableObject allBlueprint in m_AllBlueprints)
		{
			if (!allBlueprint)
			{
				PFLog.Default.Error("Blueprint library has NULL reference!");
			}
			else if (string.IsNullOrEmpty(allBlueprint.AssetGuid))
			{
				PFLog.Default.Error("Blueprint library has unidentified blueprint " + allBlueprint.name, allBlueprint);
			}
			else if (BlueprintsByAssetId.ContainsKey(allBlueprint.AssetGuid))
			{
				BlueprintScriptableObject blueprintScriptableObject = BlueprintsByAssetId[allBlueprint.AssetGuid];
				if (allBlueprint != blueprintScriptableObject)
				{
					PFLog.Default.Error("Blueprint library has duplicate blueprint id " + allBlueprint.AssetGuid + " on " + allBlueprint.name + " and " + blueprintScriptableObject.NameSafe(), allBlueprint);
				}
			}
			else
			{
				BlueprintsByAssetId[allBlueprint.AssetGuid] = allBlueprint;
			}
		}
		ScriptablesByAssetId = new Dictionary<string, ElementsScriptableObject>();
		foreach (ElementsScriptableObject allScriptable in m_AllScriptables)
		{
			if (!allScriptable)
			{
				PFLog.Default.Error("Blueprint library has NULL reference!");
			}
			else if (string.IsNullOrEmpty(allScriptable.AssetGuid))
			{
				PFLog.Default.Error("Blueprint library has unidentified blueprint " + allScriptable.name, allScriptable);
			}
			else if (ScriptablesByAssetId.ContainsKey(allScriptable.AssetGuid))
			{
				ElementsScriptableObject elementsScriptableObject = ScriptablesByAssetId[allScriptable.AssetGuid];
				if (allScriptable != elementsScriptableObject)
				{
					PFLog.Default.Error("Blueprint library has duplicate blueprint id " + allScriptable.AssetGuid + " on " + allScriptable.name + " and " + elementsScriptableObject.NameSafe(), allScriptable);
				}
			}
			else
			{
				ScriptablesByAssetId[allScriptable.AssetGuid] = allScriptable;
			}
		}
		foreach (BlueprintScriptableObject allBlueprint2 in m_AllBlueprints)
		{
			allBlueprint2.OnEnableWithLibrary();
		}
		m_Initialized = true;
	}
}
