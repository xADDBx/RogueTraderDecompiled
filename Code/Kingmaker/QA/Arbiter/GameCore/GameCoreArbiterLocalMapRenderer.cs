using Kingmaker.QA.Arbiter.Code.QA.Arbiter.Service.Interfaces;
using Kingmaker.Visual.LocalMap;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.GameCore;

public class GameCoreArbiterLocalMapRenderer : IArbiterLocalMapRenderer
{
	public RenderTexture GetTexture()
	{
		return WarhammerLocalMapRenderer.Instance.Draw().ColorRT;
	}
}
