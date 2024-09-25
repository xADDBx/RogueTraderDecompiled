using System.Collections.Generic;

namespace Kingmaker.Visual.Animation;

public interface IAddableAnimation
{
	IEnumerable<PlayableInfo> PlayableInfos { get; }

	void AddPlayableInfo(PlayableInfo playableInfo);
}
