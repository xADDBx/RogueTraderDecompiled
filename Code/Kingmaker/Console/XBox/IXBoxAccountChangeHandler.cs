using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Console.XBox;

public interface IXBoxAccountChangeHandler : ISubscriber
{
	void OnUserDetailsChanged(string newGamerTag, Texture2D newGamerPic);
}
