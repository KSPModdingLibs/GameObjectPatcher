using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Log = KSPBuildTools.Log;

namespace GameObjectPatcher
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class GameObjectPatcherAddon : MonoBehaviour
	{
		void Awake()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			Log.Message("OnSceneLoaded: " + scene.name);
		}
	}
}
