using System.Collections;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Cheats;

internal static class CheatSaveLogic
{
	[CanBeNull]
	public static string UploadedLink { get; private set; }

	public static void SaveAndUpload(string name, string description, SaveInfo.SaveType type, int mode = 0)
	{
		if (SavesStorageAccess.UploadEnabled)
		{
			UploadedLink = null;
			SaveInfo saveInfo = Game.Instance.SaveManager.CreateNewSave(name);
			saveInfo.Type = type;
			Game.Instance.GameCommandQueue.SaveGame(saveInfo, null, delegate
			{
				LoadingProcess.Instance.StartLoadingProcess(UploadProcess(saveInfo, description, Game.Instance.Player, mode));
			});
		}
	}

	private static IEnumerator UploadProcess([NotNull] SaveInfo saveInfo, string description, [CanBeNull] Player player, int mode)
	{
		while (saveInfo.OperationState != 0)
		{
			yield return null;
		}
		while (SavesStorageAccess.IsUploading)
		{
			yield return null;
		}
		Task<string> task = StartUploadTask(saveInfo, description, player);
		while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
		{
			yield return null;
		}
		if (!task.IsCompleted)
		{
			yield break;
		}
		string result = task.Result;
		if (!string.IsNullOrWhiteSpace(result))
		{
			string text2 = (UploadedLink = "owlcat://remote-save?" + result);
			switch (mode)
			{
			case 1:
				GUIUtility.systemCopyBuffer = text2;
				break;
			case 2:
				GUIUtility.systemCopyBuffer = "{HTML}<a href=\"" + text2 + "\">load</a>{HTML}";
				break;
			}
			PFLog.Default.Log("SaveStorage: " + text2);
		}
		string msg = (string.IsNullOrWhiteSpace(result) ? "Upload failed" : ("Uploaded: " + task.Result));
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(msg);
		});
	}

	private static Task<string> StartUploadTask([NotNull] SaveInfo saveInfo, string description, [CanBeNull] Player player)
	{
		SaveCreateDTO dto = SaveCreateDTO.Build(saveInfo, player);
		return SavesStorageAccess.StartUploadTask(saveInfo, description, dto);
	}
}
