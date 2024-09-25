using UnityEngine;

namespace Owlcat.Runtime.Core.Registry;

public class RegisteredBehaviour : MonoBehaviour
{
	internal void OnEnable()
	{
		Repository.Instance?.GetRegistry(GetType()).Register(this);
		OnEnabled();
	}

	internal void OnDisable()
	{
		try
		{
			OnDisabled();
		}
		finally
		{
			Repository.Instance?.GetRegistry(GetType()).Delete(this);
		}
	}

	protected virtual void OnEnabled()
	{
	}

	protected virtual void OnDisabled()
	{
	}
}
public class RegisteredBehaviour<T> : MonoBehaviour where T : RegisteredBehaviour<T>
{
	internal void OnEnable()
	{
		((IObjectRegistryBase)(Repository.Instance?.GetRegistry<T>()))?.Register((object)this);
		OnEnabled();
	}

	internal void OnDisable()
	{
		try
		{
			OnDisabled();
		}
		finally
		{
			((IObjectRegistryBase)(Repository.Instance?.GetRegistry<T>()))?.Delete((object)this);
		}
	}

	protected virtual void OnEnabled()
	{
	}

	protected virtual void OnDisabled()
	{
	}
}
