using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties;

namespace Kingmaker.Visual.Sound;

[Serializable]
public abstract class GameSyncSettings : Element
{
	public abstract void Sync(PropertyContext context, uint playingId);
}
