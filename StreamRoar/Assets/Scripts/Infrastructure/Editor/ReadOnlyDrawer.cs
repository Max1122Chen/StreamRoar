#if UNITY_EDITOR
using System.Collections.Generic;
using StreamRoar.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace StreamRoar.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        const float ButtonWidth = 44f;
        const float ButtonSpacing = 4f;

        static readonly HashSet<string> s_UnlockedProperties = new();
        static readonly GUIContent s_EditButton = new("Edit", "Temporarily unlock this read-only field.");
        static readonly GUIContent s_LockButton = new("Lock", "Return this field to read-only.");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var propertyKey = GetPropertyKey(property);
            var isUnlocked = s_UnlockedProperties.Contains(propertyKey);
            var currentEvent = Event.current;
            var shouldRelockAfterDraw = ShouldRelock(position, isUnlocked, currentEvent);

            var propertyPosition = position;
            propertyPosition.width = Mathf.Max(0f, position.width - ButtonWidth - ButtonSpacing);

            var buttonPosition = new Rect(
                position.xMax - ButtonWidth,
                position.y,
                ButtonWidth,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);

            using (new EditorGUI.DisabledScope(!isUnlocked))
            {
                EditorGUI.PropertyField(propertyPosition, property, label, true);
            }

            if (isUnlocked)
            {
                if (DrawLockButton(buttonPosition))
                {
                    s_UnlockedProperties.Remove(propertyKey);
                    GUI.FocusControl(null);
                }
            }
            else if (GUI.Button(buttonPosition, s_EditButton, EditorStyles.miniButton))
            {
                s_UnlockedProperties.Add(propertyKey);
            }

            if (shouldRelockAfterDraw)
            {
                s_UnlockedProperties.Remove(propertyKey);
                GUI.FocusControl(null);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        static string GetPropertyKey(SerializedProperty property)
        {
            var targets = property.serializedObject.targetObjects;
            var key = property.propertyPath;

            for (var i = 0; i < targets.Length; i++)
            {
                key += $"|{targets[i].GetInstanceID()}";
            }

            return key;
        }

        static bool ShouldRelock(Rect position, bool isUnlocked, Event currentEvent)
        {
            if (!isUnlocked)
            {
                return false;
            }

            if (currentEvent.rawType == EventType.MouseDown && !position.Contains(currentEvent.mousePosition))
            {
                return true;
            }

            return currentEvent.rawType == EventType.KeyDown
                && (currentEvent.keyCode == KeyCode.Return
                    || currentEvent.keyCode == KeyCode.Escape
                    || currentEvent.keyCode == KeyCode.Tab);
        }

        static bool DrawLockButton(Rect position)
        {
            var previousBackgroundColor = GUI.backgroundColor;
            var previousColor = GUI.contentColor;

            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.82f, 0.82f, 0.78f)
                : new Color(0.62f, 0.62f, 0.58f);
            GUI.contentColor = EditorGUIUtility.isProSkin
                ? new Color(0.12f, 0.12f, 0.12f)
                : Color.white;

            var clicked = GUI.Button(position, s_LockButton, EditorStyles.miniButton);

            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousColor;

            return clicked;
        }
    }
}
#endif
