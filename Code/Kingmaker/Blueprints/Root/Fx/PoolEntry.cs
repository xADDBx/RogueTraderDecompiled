using System;
using Kingmaker.Visual.Particles;

namespace Kingmaker.Blueprints.Root.Fx;

[Serializable]
public class PoolEntry
{
	public PooledFx Fx;

	public int Count;
}
