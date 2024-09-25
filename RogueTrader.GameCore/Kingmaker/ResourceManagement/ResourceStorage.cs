using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.ResourceManagement;

public class ResourceStorage<T> : IResourceStorage<T>
{
	private readonly Func<string, IResourceLoadingRequest<T>> m_CreateFunc;

	private readonly Dictionary<string, IResourceLoadingRequest<T>> m_Requests = new Dictionary<string, IResourceLoadingRequest<T>>();

	public IEnumerable<string> GetAll => m_Requests.Keys;

	public ResourceStorage(Func<string, IResourceLoadingRequest<T>> createFunc)
	{
		m_CreateFunc = createFunc;
	}

	public IResourceLoadingRequest<T> Load(string id)
	{
		IResourceLoadingRequest<T> resourceLoadingRequest = EnsureRequest(id);
		if (resourceLoadingRequest.Resource == null)
		{
			if (!resourceLoadingRequest.CanLoad)
			{
				PFLog.Default.Log("Can't load resource " + id);
				return resourceLoadingRequest;
			}
			resourceLoadingRequest.Load();
		}
		return resourceLoadingRequest;
	}

	public IResourceLoadingRequest<T> LoadAsync(string id)
	{
		IResourceLoadingRequest<T> resourceLoadingRequest = m_Requests.Get(id);
		if (resourceLoadingRequest == null)
		{
			resourceLoadingRequest = m_CreateFunc(id);
			m_Requests.Add(id, resourceLoadingRequest);
			if (!resourceLoadingRequest.CanLoad)
			{
				PFLog.Default.Log("Can't load resource " + id);
				return resourceLoadingRequest;
			}
			CoroutineRunner.Start(resourceLoadingRequest.LoadRoutine());
		}
		return resourceLoadingRequest;
	}

	public void Unload(string id)
	{
		if (m_Requests.Get(id) != null)
		{
			m_Requests.Remove(id);
		}
	}

	private IResourceLoadingRequest<T> EnsureRequest(string path)
	{
		IResourceLoadingRequest<T> resourceLoadingRequest = m_Requests.Get(path);
		if (resourceLoadingRequest == null)
		{
			resourceLoadingRequest = m_CreateFunc(path);
			m_Requests.Add(path, resourceLoadingRequest);
		}
		return resourceLoadingRequest;
	}
}
