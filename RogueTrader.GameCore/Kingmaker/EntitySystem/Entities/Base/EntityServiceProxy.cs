using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.EntitySystem.Entities.Base;

public class EntityServiceProxy : IDisposable
{
	public readonly string Id;

	[CanBeNull]
	public IEntity Entity { get; set; }

	public bool IsDisposed { get; private set; }

	public override int GetHashCode()
	{
		return Id?.GetHashCode() ?? 0;
	}

	public EntityServiceProxy(string id)
	{
		Id = id;
	}

	public override string ToString()
	{
		return "proxy#" + Entity;
	}

	public void Dispose()
	{
		IsDisposed = true;
		Entity = null;
	}
}
