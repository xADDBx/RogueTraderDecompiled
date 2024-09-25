using UnityEngine;

namespace Kingmaker.Localization;

[CreateAssetMenu(menuName = "Localization/FastPack String List", order = -10000)]
public class FastPackStringList : ScriptableObject
{
	public SharedStringAsset[] Strings;
}
