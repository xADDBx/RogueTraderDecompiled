using UnityEngine;

namespace Kingmaker.Utility.Attributes;

public class AssetPickerAttribute : PropertyAttribute
{
	public string Path;

	public AssetPickerAttribute(string path = "")
	{
		Path = path;
	}
}
