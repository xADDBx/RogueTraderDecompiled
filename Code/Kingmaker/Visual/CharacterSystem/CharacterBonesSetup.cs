using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterBonesSetup : ScriptableObject
{
	[InfoBox("This is a list of all skeleton bones and all FX locators for characters. If you add/change anything in Skeletons or CharacterFXBonesMaps make sure to update this object too (use context menu in the inspector)")]
	[InspectorReadOnly]
	public List<string> KnownTransformNames = new List<string>();

	private Dictionary<string, int> m_NameToIndex;

	public static CharacterBonesSetup Instance => BlueprintRoot.Instance.Prefabs.CharacterBonesSetup;

	public int GetIndex(string s)
	{
		if (m_NameToIndex == null)
		{
			m_NameToIndex = new Dictionary<string, int>();
			for (int i = 0; i < KnownTransformNames.Count; i++)
			{
				m_NameToIndex[KnownTransformNames[i]] = i;
			}
		}
		return m_NameToIndex.Get(s, 0);
	}
}
