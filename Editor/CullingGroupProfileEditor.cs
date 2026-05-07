using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using TW.CullingGroups;

[CustomEditor(typeof(CullingGroupProfile))]
public class CullingGroupProfileEditor : Editor
{
	protected SerializedProperty m_distance;
	protected static SerializedProperty m_ShadowCascade2SplitProp;

	protected virtual void OnEnable()
	{
		m_ShadowCascade2SplitProp = serializedObject.FindProperty("m_Cascade2Split");
		m_distance = serializedObject.FindProperty("distance");

	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(m_distance);
		DrawCascadeSplitGUI<float>(ref m_ShadowCascade2SplitProp);

		if (serializedObject.hasModifiedProperties)
			serializedObject.ApplyModifiedProperties();

	}

	public static void DrawCascadeSplitGUI<T>(ref SerializedProperty shadowCascadeSplit)
	{
		float[] cascadePartitionSizes = null;
		Type type = typeof(T);
		if (type == typeof(float))
		{
			cascadePartitionSizes = new float[] { shadowCascadeSplit.floatValue };
		}
		else if (type == typeof(Vector3))
		{
			Vector3 splits = shadowCascadeSplit.vector3Value;
			cascadePartitionSizes = new float[]
			{
					Mathf.Clamp(splits[0], 0.0f, 1.0f),
					Mathf.Clamp(splits[1] - splits[0], 0.0f, 1.0f),
					Mathf.Clamp(splits[2] - splits[1], 0.0f, 1.0f)
			};
		}
		if (cascadePartitionSizes != null)
		{
			EditorGUI.BeginChangeCheck();
			CullingCascadeSplitGUI.HandleCascadeSliderGUI(ref cascadePartitionSizes);
			if (EditorGUI.EndChangeCheck())
			{
				if (type == typeof(float))
					shadowCascadeSplit.floatValue = cascadePartitionSizes[0];
				else
				{
					Vector3 updatedValue = new Vector3();
					updatedValue[0] = cascadePartitionSizes[0];
					updatedValue[1] = updatedValue[0] + cascadePartitionSizes[1];
					updatedValue[2] = updatedValue[1] + cascadePartitionSizes[2];
					shadowCascadeSplit.vector3Value = updatedValue;
				}
			}
		}
	}
}
