using UnityEngine;

namespace Kingmaker.View.Animation;

public class DecoratorObjectLink : MonoBehaviour, DecoratorObjectLink.IEditor
{
	public interface IEditor
	{
		GameObject EntryGameObject { get; }

		DecoratorEntry SourceEntry { get; }

		UnitAnimationDecoratorObject DecoratorObject { get; }
	}

	private UnitAnimationDecoratorObject m_DecoratorObject;

	private GameObject m_EntryGameObject;

	private DecoratorEntry m_SourceEntry;

	UnitAnimationDecoratorObject IEditor.DecoratorObject => m_DecoratorObject;

	GameObject IEditor.EntryGameObject => m_EntryGameObject;

	DecoratorEntry IEditor.SourceEntry => m_SourceEntry;

	public void Init(GameObject entryGameObject, DecoratorEntry sourceEntry, UnitAnimationDecoratorObject decoratorObject)
	{
		m_EntryGameObject = entryGameObject;
		m_SourceEntry = sourceEntry;
		m_DecoratorObject = decoratorObject;
	}
}
