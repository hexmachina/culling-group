using TW.CullingGroups;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
//[CustomEditor(typeof(CullingGroupObject), true)]
public class CullingGroupObjectEditor : Editor
{
	SerializedProperty m_Center;
	SerializedProperty m_Radius;
	//SerializedProperty m_dynamic;
	//SerializedProperty m_profile;
	SerializedProperty onStateChanged;



	static Color s_HandleColor = new Color(187f, 138f, 240f, 210f) / 255;
	static Color s_HandleColorDisabled = new Color(187f * 0.75f, 138f * 0.75f, 240f * 0.75f, 100f) / 255;

	SphereBoundsHandle m_BoundsHandle = new SphereBoundsHandle();

	bool editingCollider => EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this);

	private void OnEnable()
	{

		//m_profile = serializedObject.FindProperty("profile");
		m_Center = serializedObject.FindProperty("m_Center");
		m_Radius = serializedObject.FindProperty("m_Radius");
		//m_dynamic = serializedObject.FindProperty("_dynamic");
		onStateChanged = serializedObject.FindProperty("onCullingStateChange");


	}

	Bounds GetBounds()
	{
		var obj = (CullingGroupObject)target;

		return new Bounds(obj.transform.position, new Vector3(obj.Radius, obj.Radius, obj.Radius));
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		//EditorGUILayout.PropertyField(m_profile);

		EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Volume",
			EditorGUIUtility.IconContent("EditCollider"), GetBounds, this);

		EditorGUILayout.PropertyField(m_Center);
		EditorGUILayout.PropertyField(m_Radius);
		//EditorGUILayout.PropertyField(m_dynamic);

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(onStateChanged);

		if (serializedObject.hasModifiedProperties)
			serializedObject.ApplyModifiedProperties();

	}

	[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
	static void RenderBoxGizmo(CullingGroupObject obj, GizmoType gizmoType)
	{
		var color = obj.enabled ? s_HandleColor : s_HandleColorDisabled;
		var colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);

		var oldColor = Gizmos.color;
		var oldMatrix = Gizmos.matrix;

		var matrix = new Matrix4x4();
		matrix.SetTRS(obj.transform.position, obj.transform.rotation, Vector3.one);
		Gizmos.matrix = matrix;
		Gizmos.color = colorTrans;
		Gizmos.DrawSphere(obj.Center, obj.Radius);

		Gizmos.color = color;
		Gizmos.DrawWireSphere(obj.Center, obj.Radius);

		Gizmos.matrix = oldMatrix;
		Gizmos.color = oldColor;

		//Gizmos.DrawIcon(navModifier.transform.position, "NavMeshModifierVolume Icon", true);
	}

	void OnSceneGUI()
	{
		if (!editingCollider)
			return;

		var vol = (CullingGroupObject)target;
		var color = vol.enabled ? s_HandleColor : s_HandleColorDisabled;
		var matrix = new Matrix4x4();
		matrix.SetTRS(vol.transform.position, vol.transform.rotation, Vector3.one);
		using (new Handles.DrawingScope(color, matrix))
		{
			m_BoundsHandle.center = vol.Center;
			m_BoundsHandle.radius = vol.Radius;

			EditorGUI.BeginChangeCheck();
			m_BoundsHandle.DrawHandle();
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(vol, "Modified Culling Group Object Modifier Volume");
				Vector3 center = m_BoundsHandle.center;
				float radius = m_BoundsHandle.radius;
				vol.Center = center;
				vol.Radius = radius;
				EditorUtility.SetDirty(target);
			}
		}
	}

	[MenuItem("GameObject/Culling Group/Culling Group Object", false, 2002)]
	static public void CreateNavMeshModifierVolume(MenuCommand menuCommand)
	{
		var parent = menuCommand.context as GameObject;
		var go = CreateAndSelectGameObject("Cull Group Object", parent);
		go.AddComponent<CullingGroupObject>();
		var view = SceneView.lastActiveSceneView;
		if (view != null)
			view.MoveToView(go.transform);
	}

	public static GameObject CreateAndSelectGameObject(string suggestedName, GameObject parent)
	{
		var parentTransform = parent != null ? parent.transform : null;
		var uniqueName = GameObjectUtility.GetUniqueNameForSibling(parentTransform, suggestedName);
		var child = new GameObject(uniqueName);

		Undo.RegisterCreatedObjectUndo(child, "Create " + uniqueName);
		if (parentTransform != null)
			Undo.SetTransformParent(child.transform, parentTransform, "Parent " + uniqueName);

		Selection.activeGameObject = child;

		return child;
	}
}
