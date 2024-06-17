using UnityEngine;

namespace Kingmaker.Localization;

[CreateAssetMenu(menuName = "Localization/Shared String", order = -10000)]
public class SharedStringAsset : ScriptableObject
{
	public LocalizedString String;
}
