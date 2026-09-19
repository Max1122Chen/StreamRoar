#if UNITY_EDITOR
using StreamRoar.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace StreamRoar.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public sealed class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldDraw(property))
                return;

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShouldDraw(property))
                return 0f;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        bool ShouldDraw(SerializedProperty property)
        {
            ShowIfAttribute condition = (ShowIfAttribute)attribute;
            SerializedProperty source = FindSiblingProperty(property, condition.FieldName);
            if (source == null)
                return true;

            return Matches(source, condition.ExpectedValue);
        }

        static bool Matches(SerializedProperty source, object expectedValue)
        {
            switch (source.propertyType)
            {
                case SerializedPropertyType.Enum:
                    int enumIndex = source.enumValueIndex;
                    if (enumIndex < 0 || enumIndex >= source.enumNames.Length)
                        return true;
                    return source.enumNames[enumIndex] == expectedValue.ToString();
                case SerializedPropertyType.Integer:
                    return expectedValue is int expectedInt && source.intValue == expectedInt;
                case SerializedPropertyType.Boolean:
                    return expectedValue is bool expectedBool && source.boolValue == expectedBool;
                case SerializedPropertyType.Float:
                    return expectedValue is float expectedFloat &&
                        Mathf.Approximately(source.floatValue, expectedFloat);
                case SerializedPropertyType.String:
                    return expectedValue is string expectedString && source.stringValue == expectedString;
                default:
                    return true;
            }
        }

        static SerializedProperty FindSiblingProperty(SerializedProperty property, string siblingName)
        {
            string path = property.propertyPath;
            int lastSeparatorIndex = path.LastIndexOf('.');
            string siblingPath = lastSeparatorIndex >= 0
                ? path.Substring(0, lastSeparatorIndex + 1) + siblingName
                : siblingName;

            return property.serializedObject.FindProperty(siblingPath);
        }
    }
}
#endif
