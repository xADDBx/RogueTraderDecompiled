using JetBrains.Annotations;

namespace Kingmaker.ResourceManagement;

public interface IResourceStorage<T>
{
	[CanBeNull]
	IResourceLoadingRequest<T> Load(string id);

	IResourceLoadingRequest<T> LoadAsync(string id);

	void Unload(string id);
}
