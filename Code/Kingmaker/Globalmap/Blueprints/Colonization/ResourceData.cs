using System;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Serializable]
public class ResourceData
{
	[SerializeField]
	public BlueprintResourceReference Resource;

	[SerializeField]
	public int Count;
}
