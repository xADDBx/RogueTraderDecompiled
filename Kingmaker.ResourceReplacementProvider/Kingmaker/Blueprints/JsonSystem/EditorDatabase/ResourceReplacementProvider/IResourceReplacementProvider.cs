using Kingmaker.BundlesLoading;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;

public interface IResourceReplacementProvider
{
	object OnResourceLoaded(object resource, string guid);

	string GetBundleNameForAsset(string guid);

	AssetBundle TryLoadBundle(string bundleName);

	DependencyData GetDependenciesForBundle(string bundleName);
}
