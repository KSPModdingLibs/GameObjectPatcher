using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Log = KSPBuildTools.Log;

namespace GameObjectPatcher
{
	internal class DeleteObject : ScenePatch
	{
		[Persistent] public string[] ObjectNames;

		public override void Execute()
		{
			foreach (var objectName in ObjectNames)
			{
				var gameObject = GameObject.Find(objectName);
				if (gameObject == null)
				{
					Log.Error($"GameObject {objectName} not found");
				}
				else
				{
					GameObject.Destroy(gameObject);
				}
			}
		}
	}
}
