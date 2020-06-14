using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UDButton), true)]
public class UDButtonEditor : ButtonEditor
{
    SerializedProperty onDownProperty;
    SerializedProperty onUpProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        onDownProperty = serializedObject.FindProperty("onDown");
        onUpProperty = serializedObject.FindProperty("onUp");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
        EditorGUILayout.PropertyField(onDownProperty);
        EditorGUILayout.PropertyField(onUpProperty);
        serializedObject.ApplyModifiedProperties();
    }
}