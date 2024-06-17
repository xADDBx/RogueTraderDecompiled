using System;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.EntitySystem;

public struct BlueprintComponentAndRuntime<TComponent> where TComponent : BlueprintComponent
{
	public readonly TComponent Component;

	public readonly EntityFactComponent Runtime;

	public BlueprintComponentAndRuntime(TComponent component, EntityFactComponent runtime)
	{
		Component = component;
		Runtime = runtime;
	}

	public TData GetData<TData>() where TData : class
	{
		return Runtime.GetData<TData>();
	}

	public IDisposable RequestEventContext()
	{
		return (Runtime as ISubscriptionProxy)?.RequestEventContext();
	}
}
