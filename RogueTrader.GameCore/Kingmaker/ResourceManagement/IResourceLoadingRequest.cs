using System.Collections;

namespace Kingmaker.ResourceManagement;

public interface IResourceLoadingRequest<T>
{
	T Resource { get; }

	bool CanLoad { get; }

	IEnumerator LoadRoutine();

	void Load();

	void SetPriority(ResourceLoadingPriority priority);
}
