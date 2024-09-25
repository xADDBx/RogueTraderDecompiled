using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Common.DebugInformation;

public class BlueprintDebugInformationBubble : MonoBehaviour
{
	private CompositeDisposable m_Disposable;

	public void Initialize(BlueprintScriptableObject blueprint)
	{
		if (m_Disposable == null)
		{
			m_Disposable = new CompositeDisposable();
			m_Disposable.Add(this.SetHint(GetHint(blueprint)));
		}
	}

	public void Dispose()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	private string GetHint(BlueprintScriptableObject blueprint)
	{
		string blueprintPath = Utilities.GetBlueprintPath(blueprint);
		if (!string.IsNullOrWhiteSpace(blueprintPath))
		{
			return blueprintPath;
		}
		return blueprint.name;
	}

	private void ShowBlueprintInEditor(BlueprintScriptableObject blueprint)
	{
	}
}
