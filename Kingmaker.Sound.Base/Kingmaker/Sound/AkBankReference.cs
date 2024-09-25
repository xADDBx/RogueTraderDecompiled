using System;

namespace Kingmaker.Sound;

[Serializable]
public class AkBankReference : AkReferenceBase
{
	public void Load()
	{
		if (!string.IsNullOrEmpty(Value))
		{
			AkBankManager.LoadBank(Value, decodeBank: false, saveDecodedBank: false);
		}
	}

	public void Unload()
	{
		if (!string.IsNullOrEmpty(Value))
		{
			AkBankManager.UnloadBank(Value);
		}
	}
}
