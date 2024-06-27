using System;
using UnityEngine;

namespace Kingmaker.Utility.ModsInfo;

[Serializable]
public class ModInfo
{
	[SerializeField]
	public string Id;

	[SerializeField]
	public string Version;
}
