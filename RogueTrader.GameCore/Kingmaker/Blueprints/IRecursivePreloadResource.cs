using Kingmaker.ResourceLinks.BaseInterfaces;

namespace Kingmaker.Blueprints;

public interface IRecursivePreloadResource : IResource
{
	void DoRecursivePreload();
}
