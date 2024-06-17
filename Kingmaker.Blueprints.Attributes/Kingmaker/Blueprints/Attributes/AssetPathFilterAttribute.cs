using UnityEngine;

namespace Kingmaker.Blueprints.Attributes;

public class AssetPathFilterAttribute : PropertyAttribute
{
	public string[] Filters;

	public AssetPathFilterAttribute(params string[] filters)
	{
		Filters = filters;
	}
}
