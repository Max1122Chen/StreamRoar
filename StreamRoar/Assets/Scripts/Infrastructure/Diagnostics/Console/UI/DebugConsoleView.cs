#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// OnGUI Debug Console 视图。Toggle 暂用 Keyboard（F03 IMC 前过渡）。
    /// 支持命令名阶段的实时候选提示。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugConsoleView : MonoBehaviour
    {
        const string InputControlName = "StreamRoar.DebugConsole.Input";
        const Key ToggleKey = Key.Backquote;
        const float HintLineHeight = 16f;
        const int MaxVisibleHints = 8;

        IDebugConsole m_Console;
        string m_Input = string.Empty;
        string m_LastHintInput;
        Vector2 m_Scroll;
        bool m_FocusRequested;
        bool m_ToggleWasDown;
        int m_SelectedHint;

        public void Bind(IDebugConsole console)
        {
            m_Console = console;
        }

        void Update()
        {
            if (m_Console == null)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            bool toggleDown = keyboard[ToggleKey].isPressed;
            if (toggleDown && !m_ToggleWasDown)
            {
                m_Console.Toggle();
                if (m_Console.IsOpen)
                {
                    m_FocusRequested = true;
                    m_Input = string.Empty;
                    m_SelectedHint = 0;
                    m_LastHintInput = null;
                }
            }

            m_ToggleWasDown = toggleDown;
        }

        void OnGUI()
        {
            if (m_Console == null || !m_Console.IsOpen)
                return;

            IReadOnlyList<string> candidates = m_Console.GetCompletionCandidates(m_Input);
            SyncHintSelection(candidates);
            HandleGuiKeys(candidates);

            float hintBlock = candidates.Count > 0
                ? 4f + candidates.Count * HintLineHeight
                : 0f;
            const float baseHeight = 280f;
            float height = baseHeight + hintBlock;
            Rect area = new Rect(8f, Screen.height - height - 8f, Screen.width - 16f, height);
            GUI.Box(area, "Debug Console  (` close · Tab accept · ↑↓ hints/history)");

            IReadOnlyList<ConsoleOutputEntry> entries = m_Console.GetOutputSnapshot();
            int visualLines = 0;
            for (int i = 0; i < entries.Count; i++)
                visualLines += CountLines(entries[i].Text);

            float outputBottomPad = 36f + hintBlock;
            Rect outputRect = new Rect(area.x + 8f, area.y + 24f, area.width - 16f, height - 24f - outputBottomPad);
            float contentHeight = Mathf.Max(outputRect.height, visualLines * 18f);
            m_Scroll = GUI.BeginScrollView(
                outputRect,
                m_Scroll,
                new Rect(0f, 0f, outputRect.width - 20f, contentHeight));

            float y = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                ConsoleOutputEntry entry = entries[i];
                Color prev = GUI.color;
                GUI.color = entry.Kind == ConsoleOutputKind.Error
                    ? new Color(1f, 0.45f, 0.45f)
                    : entry.Kind == ConsoleOutputKind.Input
                        ? new Color(0.7f, 0.9f, 1f)
                        : Color.white;

                string[] parts = entry.Text.Replace("\r\n", "\n").Split('\n');
                for (int p = 0; p < parts.Length; p++)
                {
                    GUI.Label(new Rect(0f, y, outputRect.width - 24f, 18f), parts[p]);
                    y += 18f;
                }

                GUI.color = prev;
            }

            GUI.EndScrollView();

            if (candidates.Count > 0)
            {
                float hintTop = area.yMax - 28f - hintBlock;
                for (int i = 0; i < candidates.Count && i < MaxVisibleHints; i++)
                {
                    Rect hintRect = new Rect(area.x + 10f, hintTop + i * HintLineHeight, area.width - 20f, HintLineHeight);
                    Color prev = GUI.color;
                    GUI.color = i == m_SelectedHint
                        ? new Color(1f, 0.92f, 0.45f)
                        : new Color(0.75f, 0.85f, 0.75f);
                    string mark = i == m_SelectedHint ? "> " : "  ";
                    string help = TryGetHelp(candidates[i]);
                    string label = string.IsNullOrEmpty(help)
                        ? mark + candidates[i]
                        : mark + candidates[i] + "  —  " + help;
                    GUI.Label(hintRect, label);
                    GUI.color = prev;
                }
            }

            Rect inputRect = new Rect(area.x + 8f, area.yMax - 28f, area.width - 16f, 22f);
            GUI.SetNextControlName(InputControlName);
            string nextInput = GUI.TextField(inputRect, m_Input ?? string.Empty);
            if (nextInput != m_Input)
            {
                m_Input = nextInput;
                m_SelectedHint = 0;
                m_LastHintInput = null;
            }

            if (m_FocusRequested)
            {
                GUI.FocusControl(InputControlName);
                m_FocusRequested = false;
                m_Scroll.y = float.MaxValue;
            }
        }

        void SyncHintSelection(IReadOnlyList<string> candidates)
        {
            if (candidates.Count == 0)
            {
                m_SelectedHint = 0;
                m_LastHintInput = m_Input;
                return;
            }

            if (m_LastHintInput != m_Input)
            {
                m_SelectedHint = 0;
                m_LastHintInput = m_Input;
            }

            if (m_SelectedHint >= candidates.Count)
                m_SelectedHint = candidates.Count - 1;
            if (m_SelectedHint < 0)
                m_SelectedHint = 0;
        }

        void HandleGuiKeys(IReadOnlyList<string> candidates)
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.KeyDown)
                return;

            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                Submit();
                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.UpArrow)
            {
                if (candidates.Count > 0)
                {
                    m_SelectedHint = (m_SelectedHint - 1 + candidates.Count) % candidates.Count;
                }
                else if (m_Console.TryHistoryPrevious(out string prev))
                {
                    m_Input = prev;
                    m_SelectedHint = 0;
                    m_LastHintInput = null;
                }

                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.DownArrow)
            {
                if (candidates.Count > 0)
                {
                    m_SelectedHint = (m_SelectedHint + 1) % candidates.Count;
                }
                else if (m_Console.TryHistoryNext(out string next))
                {
                    m_Input = next;
                    m_SelectedHint = 0;
                    m_LastHintInput = null;
                }

                e.Use();
                return;
            }

            if (e.keyCode == KeyCode.Tab)
            {
                string suggestion = m_Console.SuggestCompletion(m_Input, m_SelectedHint);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    m_Input = suggestion;
                    m_SelectedHint = 0;
                    m_LastHintInput = null;
                }

                e.Use();
            }
        }

        void Submit()
        {
            string line = m_Input;
            m_Input = string.Empty;
            m_SelectedHint = 0;
            m_LastHintInput = null;
            m_Console.TryExecute(line, out _);
            m_FocusRequested = true;
            m_Scroll.y = float.MaxValue;
        }

        string TryGetHelp(string commandName)
        {
            return m_Console.TryGetCommandHelp(commandName, out string help) ? help : null;
        }

        static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 1;
            int count = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    count++;
            }

            return count;
        }
    }
}
#endif
