using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.AccessControl;
using UnityEngine;
using Log = KSPBuildTools.Log;

namespace GameObjectPatcher
{
	internal class ModifyObject : ScenePatch
	{
		Dictionary<string, List<Action<object>>> m_gameObjectMutators = new Dictionary<string, List<Action<object>>>();

		public override void Load(ConfigNode configNode)
		{
			base.Load(configNode);

			foreach (var childNode in configNode.nodes.nodes)
			{
				m_gameObjectMutators.Add(childNode.name, CreateMutators(childNode, typeof(GameObject)));
			}
		}

		private static MemberInfo GetMemberInfo(Type objectType, string memberName)
		{
			MemberInfo memberInfo = objectType.GetField(memberName);
			if (memberInfo == null)
			{
				memberInfo = objectType.GetProperty(memberName);
			}

			if (memberInfo == null)
			{
				Log.Error($"Member {memberName} not found in type {objectType.FullName}");
			}

			return memberInfo;
		}

		private static Action<object> CreateMutator(ConfigNode.Value configValue, Type objectType)
		{
			var memberInfo = GetMemberInfo(objectType, configValue.name);

			if (memberInfo != null)
			{
				if (memberInfo is FieldInfo fieldInfo)
				{
					object newValue = ConfigNode.ReadValue(fieldInfo.FieldType, configValue.value);
					return obj => fieldInfo.SetValue(obj, newValue);
				}
				else if (memberInfo is PropertyInfo propertyInfo)
				{
					object newValue = ConfigNode.ReadValue(propertyInfo.PropertyType, configValue.value);
					return obj => propertyInfo.SetValue(obj, newValue);
				}
			}

			return null;
		}

		private static List<Action<object>> CreateMutators(ConfigNode configNode, Type objectType)
		{
			List<Action<object>> mutators = new List<Action<object>>();

			foreach (ConfigNode.Value configValue in configNode.values.values)
			{
				var mutator = CreateMutator(configValue, objectType);
				if (mutator != null)
				{
					mutators.Add(mutator);
				}
			}

			return mutators;
		}

		public override void Execute()
		{
			foreach (var objectMutator in m_gameObjectMutators)
			{
				GameObject gameObject = GameObject.Find(objectMutator.Key);

				if (gameObject == null)
				{
					Log.Error("GameObject {objectName} not found");
					continue;
				}

				foreach (var mutator in objectMutator.Value)
				{
					mutator.Invoke(gameObject);
				}
			}
		}
	}
}
