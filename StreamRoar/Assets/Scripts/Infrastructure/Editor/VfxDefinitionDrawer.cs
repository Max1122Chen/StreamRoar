#if UNITY_EDITOR
using StreamRoar.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace StreamRoar.Editor
{
    [CustomPropertyDrawer(typeof(VfxDefinition), true)]
    public sealed class VfxDefinitionDrawer : PropertyDrawer
    {
        const string PresetFieldName = "m_Preset";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect line = position;
            line.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(line, label);

            line.y += line.height + EditorGUIUtility.standardVerticalSpacing;
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            bool isValid = IsValid(property);
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;

                if (!ShouldDrawChild(iterator, isValid))
                    continue;

                float height = EditorGUI.GetPropertyHeight(iterator, true);
                if (height <= 0f)
                    continue;

                Rect childPosition = line;
                childPosition.height = height;
                EditorGUI.PropertyField(childPosition, iterator, true);
                line.y += height + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            bool isValid = IsValid(property);

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;

                if (!ShouldDrawChild(iterator, isValid))
                    continue;

                float childHeight = EditorGUI.GetPropertyHeight(iterator, true);
                if (childHeight <= 0f)
                    continue;

                height += EditorGUIUtility.standardVerticalSpacing + childHeight;
            }

            return height;
        }

        static bool IsValid(SerializedProperty property)
        {
            SerializedProperty preset = property.FindPropertyRelative(PresetFieldName);
            return preset != null && preset.objectReferenceValue != null;
        }

        static bool ShouldDrawChild(SerializedProperty child, bool isValid)
        {
            if (child.name == PresetFieldName)
                return true;

            return isValid;
        }
    }
}
#endif
