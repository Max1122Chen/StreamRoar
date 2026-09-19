#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using StreamRoar.Infrastructure;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StreamRoar.Editor
{
    /// <summary>
    /// UI 系统配置编辑窗口。
    /// </summary>
    public sealed class UISystemConfigEditorWindow : EditorWindow
    {
        static UISystemConfig.UIPrefabBaseEntry s_LastClickedEntry;

        static readonly Vector2 k_WindowSize = new Vector2(720f, 640f);
        const string k_DefaultAssetDirectory = "Assets/ScriptableObjects/UI";
        const string k_StyleSheetPath = "Assets/Scripts/Infrastructure/UI/Editor/UISystemConfigEditor.uss";

        readonly List<string> m_Errors = new List<string>();
        readonly List<string> m_Warnings = new List<string>();

        [SerializeField]
        UISystemConfig m_Config;

        int m_CurrentTab;
        static readonly string[] k_TabNames = { "Root Config",    "Layers",   "Scopes",
                                                "Scene Bindings", "Elements", "Panel Scopes" };

        ObjectField m_ConfigField;
        VisualElement m_ContentRoot;

        [MenuItem("StreamRoar/UI/UI System Config")]
        static void Open()
        {
            var window = CreateWindow<UISystemConfigEditorWindow>("UI System");
            window.ApplyWindowBounds();
            window.maximized = false;
            window.ShowUtility();
        }

        void OnEnable()
        {
            ApplyWindowBounds();
            Undo.undoRedoPerformed -= RefreshWindow;
            Undo.undoRedoPerformed += RefreshWindow;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= RefreshWindow;
        }

        void OnSelectionChange()
        {
            if (Selection.activeObject is UISystemConfig config)
            {
                m_Config = config;
                RefreshWindow();
            }
        }

        void CreateGUI()
        {
            ApplyWindowBounds();
            rootVisualElement.Clear();
            ApplyStyleSheet();
            rootVisualElement.AddToClassList("ui-system-window");

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            m_ContentRoot = scrollView;
            m_ContentRoot.AddToClassList("main-content");
            rootVisualElement.Add(m_ContentRoot);

            RefreshWindow();
        }

        void RefreshWindow()
        {
            if (m_ContentRoot == null)
                return;
            m_ContentRoot.Clear();

            m_ContentRoot.Add(CreateHeaderSection());

            if (m_Config == null)
            {
                var msg = new Label("Please select a UISystemConfig asset in the Project window or click 'New Asset'.");
                msg.style.color = new Color(0.5f, 0.5f, 0.5f);
                msg.style.alignSelf = Align.Center;
                m_ContentRoot.Add(msg);
                return;
            }

            m_ContentRoot.Add(CreateTabBar());

            var tabContentArea = new VisualElement();
            tabContentArea.AddToClassList("ui-tab-content-area");

            if (m_CurrentTab == 0)
                tabContentArea.Add(CreateRootSection());
            else if (m_CurrentTab == 1)
                tabContentArea.Add(CreateLayerSection());
            else if (m_CurrentTab == 2)
                tabContentArea.Add(CreateScopeSection());
            else if (m_CurrentTab == 3)
                tabContentArea.Add(CreateSceneMappingSection());
            else if (m_CurrentTab == 4)
                tabContentArea.Add(CreatePrefabRegistrySection());
            else if (m_CurrentTab == 5)
                tabContentArea.Add(CreateElementScopesSection());

            m_ContentRoot.Add(tabContentArea);
        }

        VisualElement CreateTabBar()
        {
            var tabBar = new VisualElement();
            tabBar.AddToClassList("ui-tab-bar");

            for (int i = 0; i < k_TabNames.Length; i++)
            {
                int index = i;
                var tabBtn = new Button(() =>
                                        {
                                            m_CurrentTab = index;
                                            RefreshWindow();
                                        });
                tabBtn.text = k_TabNames[i];
                tabBtn.AddToClassList("ui-tab-button");
                if (m_CurrentTab == index)
                {
                    tabBtn.AddToClassList("ui-tab-button-active");
                }
                tabBar.Add(tabBtn);
            }

            return tabBar;
        }

        VisualElement CreateHeaderSection()
        {
            m_Errors.Clear();
            m_Warnings.Clear();
            if (m_Config != null)
            {
                UISystemConfigValidator.Validate(m_Config, m_Errors, m_Warnings);
            }

            var section = new VisualElement();
            section.AddToClassList("ui-top-toolbar");
            var headerRow = new VisualElement();
            headerRow.AddToClassList("ui-toolbar-row");

            var leftToolbar = new VisualElement();
            leftToolbar.AddToClassList("ui-toolbar-left");

            m_ConfigField = CreateObjectField("Target Config", typeof(UISystemConfig), m_Config,
                                              evt =>
                                              {
                                                  m_Config = evt as UISystemConfig;
                                                  RefreshWindow();
                                              });
            leftToolbar.Add(m_ConfigField);

            var rightToolbar = new VisualElement();
            rightToolbar.AddToClassList("ui-toolbar-right");
            rightToolbar.Add(CreateActionButton("New Asset",
                                                () =>
                                                {
                                                    m_Config = CreateConfigAsset();
                                                    RefreshWindow();
                                                },
                                                false));

            var syncButton = CreateActionButton("Sync to Scene", BuildPreview, true);
            syncButton.SetEnabled(m_Config != null);
            rightToolbar.Add(syncButton);

            headerRow.Add(leftToolbar);
            headerRow.Add(rightToolbar);
            section.Add(headerRow);

            if (m_Errors.Count > 0 || m_Warnings.Count > 0)
            {
                var messagesContainer = new VisualElement();
                foreach (var err in m_Errors)
                    messagesContainer.Add(CreateHelpBox(err, HelpBoxMessageType.Error));
                foreach (var warn in m_Warnings)
                    messagesContainer.Add(CreateHelpBox(warn, HelpBoxMessageType.Warning));
                section.Add(messagesContainer);
            }

            return section;
        }

        VisualElement CreateRootSection()
        {
            var wrapper = CreateSection("Root Configuration", out var content);
            bool hasScreenSpaceCamera = HasRenderMode(UISystemConfig.UIRenderMode.ScreenSpaceCamera);

            content.Add(CreateTextField("Root Name", m_Config.Root.RootName,
                                        value =>
                                        {
                                            RecordConfig("Edit UI Root Name");
                                            m_Config.Root.RootName = value;
                                            SaveConfig();
                                            RefreshWindow();
                                        }));
            content.Add(CreateResolutionField("Reference Resolution", m_Config.Root.ReferenceResolution,
                                              value =>
                                              {
                                                  RecordConfig("Edit UI Reference Resolution");
                                                  m_Config.Root.ReferenceResolution = value;
                                                  SaveConfig();
                                                  RefreshWindow();
                                              }));
            content.Add(CreateSliderField("Match", m_Config.Root.MatchWidthOrHeight,
                                          value =>
                                          {
                                              RecordConfig("Edit UI Match");
                                              m_Config.Root.MatchWidthOrHeight = value;
                                              SaveConfig();
                                              RefreshWindow();
                                          }));

            if (hasScreenSpaceCamera)
            {
                content.Add(CreateEnumField("Camera Source", m_Config.Root.CameraResolveMode,
                                            value =>
                                            {
                                                RecordConfig("Edit UI Camera Source");
                                                m_Config.Root.CameraResolveMode =
                                                    (UISystemConfig.UICameraResolveMode)value;
                                                SaveConfig();
                                                RefreshWindow();
                                            }));

                if (m_Config.Root.CameraResolveMode == UISystemConfig.UICameraResolveMode.Tag)
                {
                    content.Add(CreateTextField("Camera Tag", m_Config.Root.CameraTag,
                                                value =>
                                                {
                                                    RecordConfig("Edit UI Camera Tag");
                                                    m_Config.Root.CameraTag = value;
                                                    SaveConfig();
                                                    RefreshWindow();
                                                }));
                }

                if (hasScreenSpaceCamera)
                {
                    content.Add(CreateFloatField("Plane Distance", m_Config.Root.PlaneDistance,
                                                 value =>
                                                 {
                                                     RecordConfig("Edit UI Plane Distance");
                                                     m_Config.Root.PlaneDistance = value;
                                                     SaveConfig();
                                                     RefreshWindow();
                                                 }));
                }
            }

            return wrapper;
        }

        bool HasRenderMode(UISystemConfig.UIRenderMode renderMode)
        {
            for (int i = 0; i < m_Config.Layers.Count; i++)
            {
                UISystemConfig.LayerEntry layer = m_Config.Layers[i];
                if (layer != null && layer.RenderMode == renderMode)
                    return true;
            }

            return false;
        }

        VisualElement CreateLayerSection()
        {
            var wrapper = CreateSection("Layers Hierarchy", out var content);

            var tableRows = new VisualElement();
            tableRows.AddToClassList("ui-table-rows");
            content.Add(tableRows);

            var header = new VisualElement();
            header.AddToClassList("ui-layer-header-row");
            // Drag handle spacer
            // spacer removed
            header.Add(CreateLayerCell("ui-layer-name-compact-cell", CreateColumnHeader("Layer Name")));
            header.Add(CreateLayerCell("ui-layer-mode-cell", CreateColumnHeader("Render Mode")));
            header.Add(CreateLayerCell("ui-layer-order-cell", CreateColumnHeader("Sort Order")));
            header.Add(CreateLayerCell("ui-layer-ray-cell", CreateColumnHeader("Raycaster")));
            header.Add(CreateLayerCell("ui-layer-remove-cell", CreateColumnHeader(string.Empty, true)));
            tableRows.Add(header);

            var listView = new ListView();
            listView.itemsSource = m_Config.Layers;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.showBorder = false;
            listView.selectionType = SelectionType.None;
            listView.style.flexGrow = 1;
            listView.itemIndexChanged += (src, dst) =>
            {
                SaveConfig();
                rootVisualElement.schedule.Execute(() => RefreshWindow());
            };

            listView.makeItem = () => new VisualElement();
            listView.bindItem = (element, i) =>
            {
                element.Clear();
                element.Add(CreateLayerRow(m_Config.Layers[i], i));
            };
            tableRows.Add(listView);

            var addButton = CreateActionButton("+ Add Layer",
                                               () =>
                                               {
                                                   RecordConfig("Add UI Layer");
                                                   m_Config.AddLayer();
                                                   SaveConfig();
                                                   RefreshWindow();
                                               },
                                               false);
            addButton.AddToClassList("button-add-item");
            content.Add(addButton);

            return wrapper;
        }

        VisualElement CreateLayerRow(UISystemConfig.LayerEntry layer, int index)
        {
            var row = new VisualElement();
            row.AddToClassList("ui-sub-card");
            row.AddToClassList(index % 2 == 0 ? "row-even" : "row-odd");

            var line = new VisualElement();
            line.AddToClassList("ui-layer-line");

            var layerIdField = CreateTextField("", layer.LayerId,
                                               val =>
                                               {
                                                   RecordConfig("Edit UI Layer Id");
                                                   layer.LayerId = val;
                                                   SaveConfig();
                                                   RefreshWindow();
                                               });
            layerIdField.AddToClassList("ui-layer-input-field");
            line.Add(CreateLayerCell("ui-layer-name-compact-cell", layerIdField));

            var modes = new List<string>
            {
                UISystemConfig.UIRenderMode.ScreenSpaceCamera.ToString(),
                UISystemConfig.UIRenderMode.ScreenSpaceOverlay.ToString(),
                UISystemConfig.UIRenderMode.WorldSpace.ToString()
            };
            var modeField = new PopupField<string>(modes, layer.RenderMode.ToString());
            modeField.labelElement.style.display = DisplayStyle.None;
            modeField.AddToClassList("ui-table-dropdown");
            modeField.style.flexGrow = 1f;
            modeField.RegisterValueChangedCallback(evt =>
                                                   {
                                                       RecordConfig("Edit UI Layer Render Mode");
                                                       layer.RenderMode = (UISystemConfig.UIRenderMode)System.Enum.Parse(
                                                           typeof(UISystemConfig.UIRenderMode), evt.newValue);
                                                       SaveConfig();
                                                       RefreshWindow();
                                                   });
            line.Add(CreateLayerCell("ui-layer-mode-cell", modeField));

            var sortingField = new IntegerField { value = layer.SortingOrder };
            sortingField.labelElement.style.display = DisplayStyle.None;
            sortingField.AddToClassList("ui-layer-input-field");
            sortingField.AddToClassList("ui-flat-field");
            sortingField.RegisterValueChangedCallback(evt =>
                                                      {
                                                          RecordConfig("Edit UI Layer Order");
                                                          layer.SortingOrder = evt.newValue;
                                                          SaveConfig();
                                                          RefreshWindow();
                                                      });
            line.Add(CreateLayerCell("ui-layer-order-cell", sortingField));

            var raycasterField = new Toggle { value = layer.HasGraphicRaycaster };
            raycasterField.labelElement.style.display = DisplayStyle.None;
            raycasterField.AddToClassList("ui-layer-ray-toggle");
            raycasterField.RegisterValueChangedCallback(evt =>
                                                        {
                                                            RecordConfig("Edit UI Layer Raycaster");
                                                            layer.HasGraphicRaycaster = evt.newValue;
                                                            SaveConfig();
                                                        });
            line.Add(CreateLayerCell("ui-layer-ray-cell", raycasterField));

            var removeBtn = CreateIconButton("Remove Layer", GetTrashIcon(),
                                             () =>
                                             {
                                                 RecordConfig("Remove UI Layer");
                                                 m_Config.Layers.RemoveAt(index);
                                                 SaveConfig();
                                                 RefreshWindow();
                                             });
            line.Add(CreateLayerCell("ui-layer-remove-cell", removeBtn));

            row.Add(line);
            return row;
        }

        VisualElement CreateScopeSection()
        {
            var wrapper = CreateSection("Scope Definitions", out var content);

            var tableRows = new VisualElement();
            tableRows.AddToClassList("ui-table-rows");
            content.Add(tableRows);

            var header = new VisualElement();
            header.AddToClassList("ui-layer-header-row");
            // spacer for drag handle area
            // spacer removed
            header.Add(CreateLayerCell("ui-layer-name-cell", CreateColumnHeader("Scope Name")));
            header.Add(CreateLayerCell("ui-layer-remove-cell", CreateColumnHeader(string.Empty, true)));
            tableRows.Add(header);

            var listView = new ListView();
            listView.itemsSource = m_Config.Scopes;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.showBorder = false;
            listView.selectionType = SelectionType.None;
            listView.style.flexGrow = 1;
            listView.itemIndexChanged += (src, dst) =>
            {
                SaveConfig();
                rootVisualElement.schedule.Execute(() => RefreshWindow());
            };

            listView.makeItem = () => new VisualElement();
            listView.bindItem = (element, i) =>
            {
                element.Clear();
                int index = i;
                var row = new VisualElement();
                row.AddToClassList("ui-sub-card");
                row.AddToClassList(index % 2 == 0 ? "row-even" : "row-odd");

                var line = new VisualElement();
                line.AddToClassList("ui-layer-line");

                line.style.alignItems = Align.Center;

                var scopeField = CreateTextField("", m_Config.Scopes[index],
                                                 val =>
                                                 {
                                                     RecordConfig("Edit Scope Definition");
                                                     m_Config.Scopes[index] = val;
                                                     SaveConfig();
                                                 });
                scopeField.AddToClassList("ui-layer-input-field");

                scopeField.style.flexGrow = 1;
                scopeField.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                line.Add(CreateLayerCell("ui-layer-name-cell", scopeField));

                line.Add(CreateLayerCell("ui-layer-remove-cell", CreateIconButton("Remove Scope", GetTrashIcon(),
                                                                                  () =>
                                                                                  {
                                                                                      RecordConfig("Remove Scope");
                                                                                      m_Config.Scopes.RemoveAt(index);
                                                                                      SaveConfig();
                                                                                      RefreshWindow();
                                                                                  })));

                row.Add(line);
                element.Add(row);
            };

            tableRows.Add(listView);

            var addButton = CreateActionButton("+ Add Scope",
                                               () =>
                                               {
                                                   RecordConfig("Add UI Scope");
                                                   m_Config.Scopes.Add("NewScope");
                                                   SaveConfig();
                                                   RefreshWindow();
                                               },
                                               false);
            addButton.AddToClassList("button-add-item");
            content.Add(addButton);

            return wrapper;
        }

        VisualElement CreatePrefabRegistrySection()
        {
            var wrapper = CreateSection("UI Elements Registry", out var content);

            var pathContainer = new VisualElement();
            pathContainer.style.flexDirection = FlexDirection.Row;
            pathContainer.style.alignItems = Align.Center;

            var pathField = CreateTextField("Search Path", m_Config.PrefabSearchPath,
                                            val =>
                                            {
                                                RecordConfig("Edit Prefab Search Path");
                                                m_Config.PrefabSearchPath = val;
                                                SaveConfig();
                                            });
            pathField.labelElement.style.minWidth = 100f; // Fix label width
            pathField.labelElement.style.width = 100f;
            pathField.style.flexGrow = 1;
            pathField.style.flexShrink = 1;
            pathContainer.Add(pathField);

            var syncBtn = CreateActionButton(
                "Sync Elements",
                () =>
                {
                    RecordConfig("Sync Elements");

                    string searchPath = m_Config.PrefabSearchPath.Replace('\\', '/').TrimEnd('/');
                    string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { searchPath });

                    var panelsToKeep = new List<UISystemConfig.UIPanelEntry>();
                    var windowsToKeep = new List<UISystemConfig.UIWindowEntry>();
                    var popupsToKeep = new List<UISystemConfig.UIPopupEntry>();
                    var widgetsToKeep = new List<UISystemConfig.UIWidgetEntry>();

                    foreach (var guid in guids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (!assetPath.StartsWith(searchPath + "/"))
                            continue;

                        string id = Path.GetFileNameWithoutExtension(assetPath);
                        string assetKey = BuildAssetAddress(assetPath);
                        UISystemConfig.UIPrefabType type = ResolvePrefabType(assetPath);
                        UISystemConfig.UIPrefabBaseEntry existingEntry = FindEntry(id);

                        if (existingEntry != null)
                        {
                            if (existingEntry.PrefabType != type)
                                existingEntry = CreateEntry(type, id, assetKey);
                            else
                                existingEntry.AssetKey = assetKey;
                            AddEntry(existingEntry);
                            continue;
                        }

                        AddEntry(CreateEntry(type, id, assetKey));
                    }

                    m_Config.Panels = panelsToKeep;
                    m_Config.Windows = windowsToKeep;
                    m_Config.Popups = popupsToKeep;
                    m_Config.Widgets = widgetsToKeep;
                    RemoveInvalidElementSelections(CreateValidElementIdSet());

                    SaveConfig();
                    rootVisualElement.schedule.Execute(() => RefreshWindow());

                    void AddEntry(UISystemConfig.UIPrefabBaseEntry entry)
                    {
                        switch (entry.PrefabType)
                        {
                            case UISystemConfig.UIPrefabType.Window:
                                windowsToKeep.Add((UISystemConfig.UIWindowEntry)entry);
                                break;
                            case UISystemConfig.UIPrefabType.Popup:
                                popupsToKeep.Add((UISystemConfig.UIPopupEntry)entry);
                                break;
                            case UISystemConfig.UIPrefabType.Widget:
                                widgetsToKeep.Add((UISystemConfig.UIWidgetEntry)entry);
                                break;
                            default:
                                panelsToKeep.Add((UISystemConfig.UIPanelEntry)entry);
                                break;
                        }
                    }

                    UISystemConfig.UIPrefabBaseEntry FindEntry(string entryId)
                    {
                        if (m_Config.TryGetPanel(entryId, out UISystemConfig.UIPanelEntry panel))
                            return panel;
                        if (m_Config.TryGetWindow(entryId, out UISystemConfig.UIWindowEntry window))
                            return window;
                        if (m_Config.TryGetPopup(entryId, out UISystemConfig.UIPopupEntry popup))
                            return popup;
                        if (m_Config.TryGetWidget(entryId, out UISystemConfig.UIWidgetEntry widget))
                            return widget;
                        return null;
                    }

                    UISystemConfig.UIPrefabBaseEntry CreateEntry(
                        UISystemConfig.UIPrefabType entryType, string entryId, string entryAssetKey)
                    {
                        switch (entryType)
                        {
                            case UISystemConfig.UIPrefabType.Window:
                                return new UISystemConfig.UIWindowEntry
                                {
                                    Id = entryId,
                                    AssetKey = entryAssetKey,
                                    LayerId = UISystemConfig.DefaultLayerIds.Normal
                                };
                            case UISystemConfig.UIPrefabType.Popup:
                                return new UISystemConfig.UIPopupEntry
                                {
                                    Id = entryId,
                                    AssetKey = entryAssetKey,
                                    LayerId = UISystemConfig.DefaultLayerIds.Popup
                                };
                            case UISystemConfig.UIPrefabType.Widget:
                                return new UISystemConfig.UIWidgetEntry
                                {
                                    Id = entryId,
                                    AssetKey = entryAssetKey,
                                    LayerId = UISystemConfig.DefaultLayerIds.WorldOverlay
                                };
                            default:
                                return new UISystemConfig.UIPanelEntry
                                {
                                    Id = entryId,
                                    AssetKey = entryAssetKey,
                                    LayerId = UISystemConfig.DefaultLayerIds.Normal
                                };
                        }
                    }
                },
                true);
            syncBtn.style.flexShrink = 0; // Prevent from being squished
            syncBtn.style.width = 120f;
            syncBtn.style.height = 24f;
            pathContainer.Add(syncBtn);
            pathContainer.style.marginBottom = 8f;
            content.Add(pathContainer);

            var availableLayers = new List<string>();
            foreach (var l in m_Config.Layers)
                availableLayers.Add(l.LayerId);
            if (availableLayers.Count == 0)
                availableLayers.Add(UISystemConfig.DefaultScopeIds.None);

            VisualElement CreatePrefabTable(UISystemConfig.UIPrefabType type)
            {
                var container = new VisualElement();

                System.Collections.IList list = type == UISystemConfig.UIPrefabType.Panel
                                                    ? m_Config.Panels
                                                    : type == UISystemConfig.UIPrefabType.Window
                                                        ? m_Config.Windows
                                                        : type == UISystemConfig.UIPrefabType.Popup
                                                            ? m_Config.Popups
                                                            : m_Config.Widgets;

                var tableRows = new VisualElement();
                tableRows.AddToClassList("ui-table-rows");
                container.Add(tableRows);

                var header = new VisualElement();
                header.AddToClassList("ui-layer-header-row");

                var nameHeader = CreateLayerCell("ui-layer-name-cell", CreateColumnHeader(type.ToString()));
                nameHeader.style.width = 210f;
                nameHeader.style.flexGrow = 0f;
                nameHeader.style.flexShrink = 0f;
                header.Add(nameHeader);

                var layerHeader = CreateColumnHeader("Layer", false);
                layerHeader.style.width = type == UISystemConfig.UIPrefabType.Widget ? 160f : 180f;
                layerHeader.style.flexShrink = 0f;
                layerHeader.style.paddingLeft = 11f;
                header.Add(layerHeader);

                var configHeader = CreateColumnHeader(
                    type == UISystemConfig.UIPrefabType.Widget ? "Pooled" : "Esc", true);
                configHeader.style.width = type == UISystemConfig.UIPrefabType.Widget ? 170f : 200f;
                configHeader.style.flexShrink = 0f;
                header.Add(configHeader);

                if (type == UISystemConfig.UIPrefabType.Widget)
                {
                    var depthHeader = CreateColumnHeader("No Z", true);
                    depthHeader.style.width = 70f;
                    depthHeader.style.flexShrink = 0f;
                    header.Add(depthHeader);
                }

                tableRows.Add(header);

                if (list.Count == 0)
                {
                    var emptyLabel =
                        new Label($"No {type} elements found. Drag and drop from another table to change type.");
                    emptyLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                    emptyLabel.style.paddingLeft = 10f;
                    emptyLabel.style.paddingRight = 10f;
                    emptyLabel.style.paddingTop = 0f;
                    emptyLabel.style.paddingBottom = 0f;
                    emptyLabel.style.marginTop = 0f;
                    emptyLabel.style.marginBottom = 0f;
                    emptyLabel.style.height = 24f;
                    emptyLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                    tableRows.Add(emptyLabel);
                }
                else
                {
                    var listView = new ListView();
                    listView.itemsSource = list;
                    listView.reorderable = false;
                    listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
                    listView.showBorder = false;
                    listView.selectionType = SelectionType.None;
                    listView.style.height = list.Count * 46f; // exact height
                    listView.fixedItemHeight = 46f;

                    listView.makeItem = () => new VisualElement();
                    listView.bindItem = (element, i) =>
                    {
                        element.Clear();
                        var entryBase = (UISystemConfig.UIPrefabBaseEntry)list[i];

                        var row = new VisualElement();
                        row.AddToClassList("ui-sub-card");
                        row.AddToClassList(i % 2 == 0 ? "row-even" : "row-odd");

                        var line = new VisualElement();
                        line.style.height = 46f;

                        line.AddToClassList("ui-layer-line");
                        line.style.alignItems = Align.Center;

                        var infoContainer = new VisualElement();
                        infoContainer.style.flexDirection = FlexDirection.Column;
                        infoContainer.style.justifyContent = Justify.Center;
                        infoContainer.style.paddingTop = 4f;
                        infoContainer.style.paddingBottom = 4f;

                        row.RegisterCallback<PointerDownEvent>(evt =>
                                                               { s_LastClickedEntry = entryBase; });

                        row.RegisterCallback<PointerMoveEvent>(
                            evt =>
                            {
                                if (evt.pressedButtons == 1 && s_LastClickedEntry == entryBase)
                                {
                                    s_LastClickedEntry = null;
                                    DragAndDrop.PrepareStartDrag();
                                    DragAndDrop.SetGenericData("DraggedEntry", entryBase);
                                    DragAndDrop.StartDrag("MoveType");
                                    evt.StopPropagation();
                                }
                            });

                        row.RegisterCallback<DragUpdatedEvent>(
                            evt =>
                            {
                                var draggedEntry =
                                    DragAndDrop.GetGenericData("DraggedEntry") as UISystemConfig.UIPrefabBaseEntry;
                                if (draggedEntry != null && draggedEntry.PrefabType != type)
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                                    row.AddToClassList("ui-drop-target");
                                    evt.StopPropagation();
                                }
                            },
                            TrickleDown.TrickleDown);

                        row.RegisterCallback<DragLeaveEvent>(evt =>
                                                             { row.RemoveFromClassList("ui-drop-target"); },
                                                             TrickleDown.TrickleDown);
                        row.RegisterCallback<DragExitedEvent>(evt =>
                                                              { row.RemoveFromClassList("ui-drop-target"); },
                                                              TrickleDown.TrickleDown);

                        row.RegisterCallback<DragPerformEvent>(
                            evt =>
                            {
                                row.RemoveFromClassList("ui-drop-target");
                                var draggedEntry =
                                    DragAndDrop.GetGenericData("DraggedEntry") as UISystemConfig.UIPrefabBaseEntry;
                                if (draggedEntry != null && draggedEntry.PrefabType != type)
                                {
                                    DragAndDrop.AcceptDrag();
                                    RecordConfig("Change Prefab Type via Drag");

                                    RemoveEntry(draggedEntry.Id);
                                    AddEntry(CreateEntry(type, draggedEntry));

                                    DragAndDrop.SetGenericData("DraggedEntry", null);
                                    SaveConfig();
                                    rootVisualElement.schedule.Execute(() => RefreshWindow());
                                    evt.StopPropagation();
                                }
                            },
                            TrickleDown.TrickleDown);

                        var nameLabel = new Label(entryBase.Id);
                        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                        infoContainer.Add(nameLabel);

                        var pathLabel = new Label(entryBase.AssetKey);
                        pathLabel.style.color = new Color(0.55f, 0.55f, 0.55f);
                        pathLabel.style.fontSize = 10f;
                        infoContainer.Add(pathLabel);

                        var nameCell = CreateLayerCell("ui-layer-name-cell", infoContainer);
                        nameCell.style.width = 210f;
                        nameCell.style.flexGrow = 0f;
                        nameCell.style.flexShrink = 0f;
                        line.Add(nameCell);

                        var layerCell = new VisualElement();
                        layerCell.style.width = type == UISystemConfig.UIPrefabType.Widget ? 160f : 180f;
                        layerCell.style.flexShrink = 0f;
                        layerCell.style.justifyContent = Justify.FlexStart;
                        layerCell.style.alignItems = Align.Center;
                        layerCell.style.paddingLeft = 5f;

                        var layerPopup = new PopupField<string>(
                            availableLayers,
                            availableLayers.Contains(entryBase.LayerId)
                                ? entryBase.LayerId
                                : availableLayers[0]);
                        layerPopup.labelElement.style.display = DisplayStyle.None;
                        layerPopup.AddToClassList("ui-table-dropdown");
                        layerPopup.style.width = type == UISystemConfig.UIPrefabType.Widget ? 150f : 170f;
                        layerPopup.RegisterValueChangedCallback(evt =>
                        {
                            RecordConfig("Edit Layer");
                            entryBase.LayerId = evt.newValue;
                            SaveConfig();
                        });
                        layerCell.Add(layerPopup);

                        line.Add(layerCell);

                        var toggle = new Toggle();
                        var toggleCell = new VisualElement();
                        toggleCell.style.width = type == UISystemConfig.UIPrefabType.Widget ? 170f : 200f;
                        toggleCell.style.flexShrink = 0f;
                        toggleCell.style.flexDirection = FlexDirection.Row;
                        toggleCell.style.justifyContent = Justify.Center;
                        toggleCell.style.alignItems = Align.Center;

                        var toggleWrapper = new VisualElement();
                        toggleWrapper.style.width = 40f;
                        toggleWrapper.style.flexShrink = 0f;
                        toggleWrapper.style.justifyContent = Justify.Center;
                        toggleWrapper.style.alignItems = Align.Center;
                        toggleWrapper.Add(toggle);
                        toggleCell.Add(toggleWrapper);

                        if (type != UISystemConfig.UIPrefabType.Widget)
                        {
                            toggle.value = entryBase.IsEscapable;
                            toggleWrapper.style.width = 80f;
                            toggle.RegisterValueChangedCallback(
                                evt =>
                                {
                                    RecordConfig("Edit Stack");
                                    entryBase.IsEscapable = evt.newValue;
                                    SaveConfig();
                                });
                        }
                        else
                        {
                            var widget = (UISystemConfig.UIWidgetEntry)entryBase;
                            toggle.value = widget.IsPooled;
                            toggle.tooltip = "启用后会按 Initial 预热实例，并在运行时按需自动扩容。";

                            var rulesContainer = new VisualElement();
                            rulesContainer.style.flexDirection = FlexDirection.Column;
                            rulesContainer.style.justifyContent = Justify.Center;

                            toggle.RegisterValueChangedCallback(evt =>
                                                                {
                                                                    RecordConfig("Edit Pool");
                                                                    widget.IsPooled = evt.newValue;
                                                                    rulesContainer.style.display =
                                                                        evt.newValue
                                                                            ? DisplayStyle.Flex
                                                                            : DisplayStyle.None;
                                                                    toggleWrapper.style.width =
                                                                        evt.newValue ? 40f : 160f;
                                                                    SaveConfig();
                                                                });

                            var row1 = new VisualElement();
                            row1.style.flexDirection = FlexDirection.Row;
                            row1.style.alignItems = Align.Center;
                            row1.style.marginBottom = 1f;

                            var initLabel = new Label("Init");
                            initLabel.style.width = 33f;
                            initLabel.style.fontSize = 10f;
                            initLabel.style.color = new Color(0.65f, 0.65f, 0.65f);
                            var initField = new IntegerField { value = Mathf.Max(0, widget.InitialCapacity) };
                            initField.labelElement.style.display = DisplayStyle.None;
                            initField.tooltip = "预热容量";
                            initField.RegisterValueChangedCallback(evt =>
                                                                   {
                                                                       RecordConfig("Edit Initial Capacity");
                                                                       widget.InitialCapacity =
                                                                           Mathf.Max(0, evt.newValue);
                                                                       SaveConfig();
                                                                   });
                            initField.style.width = 40f;
                            initField.style.fontSize = 11f;
                            initField.style.marginTop = 0f;
                            initField.style.marginBottom = 0f;
                            initField.AddToClassList("ui-tiny-input");

                            row1.Add(initLabel);
                            row1.Add(initField);

                            var row2 = new VisualElement();
                            row2.style.flexDirection = FlexDirection.Row;
                            row2.style.alignItems = Align.Center;
                            row2.style.marginTop = 1f;

                            var minLabel = new Label("Cache");
                            minLabel.style.width = 33f;
                            minLabel.style.fontSize = 10f;
                            minLabel.style.color = new Color(0.65f, 0.65f, 0.65f);
                            var minField = new IntegerField { value = Mathf.Max(0, widget.MinCachedCount) };
                            minField.labelElement.style.display = DisplayStyle.None;
                            minField.tooltip = "常驻缓存数";
                            minField.RegisterValueChangedCallback(evt =>
                                                                  {
                                                                      RecordConfig("Edit Min Cached Count");
                                                                      widget.MinCachedCount =
                                                                          Mathf.Max(0, evt.newValue);
                                                                      SaveConfig();
                                                                  });
                            minField.style.width = 40f;
                            minField.style.fontSize = 11f;
                            minField.style.marginTop = 0f;
                            minField.style.marginBottom = 0f;
                            minField.AddToClassList("ui-tiny-input");
                            row2.Add(minLabel);
                            row2.Add(minField);

                            rulesContainer.Add(row1);
                            rulesContainer.Add(row2);
                            toggleCell.Add(rulesContainer);
                            rulesContainer.style.display =
                                widget.IsPooled ? DisplayStyle.Flex : DisplayStyle.None;
                            toggleWrapper.style.width = widget.IsPooled ? 40f : 160f;
                        }

                        line.Add(toggleCell);

                        if (type == UISystemConfig.UIPrefabType.Widget)
                        {
                            var widget = (UISystemConfig.UIWidgetEntry)entryBase;
                            var depthCell = new VisualElement();
                            depthCell.style.width = 70f;
                            depthCell.style.flexShrink = 0f;
                            depthCell.style.flexDirection = FlexDirection.Row;
                            depthCell.style.justifyContent = Justify.Center;
                            depthCell.style.alignItems = Align.Center;

                            var depthToggle = new Toggle { value = widget.IgnoreDepth };
                            depthToggle.labelElement.style.display = DisplayStyle.None;
                            depthToggle.tooltip = "忽略场景深度测试，始终显示在 Mesh 前；也会穿透墙体和地形。";
                            depthToggle.RegisterValueChangedCallback(evt =>
                            {
                                RecordConfig("Edit Widget Depth");
                                widget.IgnoreDepth = evt.newValue;
                                SaveConfig();
                            });
                            depthCell.Add(depthToggle);
                            line.Add(depthCell);
                        }

                        row.Add(line);
                        element.Add(row);
                    };

                    tableRows.Add(listView);
                }

                container.RegisterCallback<DragUpdatedEvent>(
                    evt =>
                    {
                        var draggedEntry =
                            DragAndDrop.GetGenericData("DraggedEntry") as UISystemConfig.UIPrefabBaseEntry;
                        if (draggedEntry != null && draggedEntry.PrefabType != type)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        }
                    });
                container.RegisterCallback<DragPerformEvent>(
                    evt =>
                    {
                        var draggedEntry =
                            DragAndDrop.GetGenericData("DraggedEntry") as UISystemConfig.UIPrefabBaseEntry;
                        if (draggedEntry != null && draggedEntry.PrefabType != type)
                        {
                            DragAndDrop.AcceptDrag();
                            RecordConfig("Change Prefab Type via Drag");

                            RemoveEntry(draggedEntry.Id);
                            AddEntry(CreateEntry(type, draggedEntry));

                            DragAndDrop.SetGenericData("DraggedEntry", null);
                            SaveConfig();
                            rootVisualElement.schedule.Execute(() => RefreshWindow());
                        }
                    });

                return container;
            }

            UISystemConfig.UIPrefabType[] types =
            {
                UISystemConfig.UIPrefabType.Window,
                UISystemConfig.UIPrefabType.Panel,
                UISystemConfig.UIPrefabType.Popup,
                UISystemConfig.UIPrefabType.Widget
            };
            for (int i = 0; i < types.Length; i++)
            {
                VisualElement table = CreatePrefabTable(types[i]);
                table.style.marginBottom = 15f;
                content.Add(table);
            }

            return wrapper;

            void RemoveEntry(string id)
            {
                m_Config.Panels.Remove(m_Config.Panels.Find(entry => entry.Id == id));
                m_Config.Windows.Remove(m_Config.Windows.Find(entry => entry.Id == id));
                m_Config.Popups.Remove(m_Config.Popups.Find(entry => entry.Id == id));
                m_Config.Widgets.Remove(m_Config.Widgets.Find(entry => entry.Id == id));
            }

            void AddEntry(UISystemConfig.UIPrefabBaseEntry entry)
            {
                switch (entry.PrefabType)
                {
                    case UISystemConfig.UIPrefabType.Window:
                        m_Config.Windows.Add((UISystemConfig.UIWindowEntry)entry);
                        break;
                    case UISystemConfig.UIPrefabType.Popup:
                        m_Config.Popups.Add((UISystemConfig.UIPopupEntry)entry);
                        break;
                    case UISystemConfig.UIPrefabType.Widget:
                        m_Config.Widgets.Add((UISystemConfig.UIWidgetEntry)entry);
                        break;
                    default:
                        m_Config.Panels.Add((UISystemConfig.UIPanelEntry)entry);
                        break;
                }
            }

            UISystemConfig.UIPrefabBaseEntry CreateEntry(
                UISystemConfig.UIPrefabType type, UISystemConfig.UIPrefabBaseEntry source)
            {
                if (type == UISystemConfig.UIPrefabType.Window)
                {
                    return new UISystemConfig.UIWindowEntry
                    {
                        Id = source.Id,
                        AssetKey = source.AssetKey,
                        LayerId = UISystemConfig.DefaultLayerIds.Normal,
                        IsEscapable = source.IsEscapable
                    };
                }

                if (type == UISystemConfig.UIPrefabType.Popup)
                {
                    return new UISystemConfig.UIPopupEntry
                    {
                        Id = source.Id,
                        AssetKey = source.AssetKey,
                        LayerId = UISystemConfig.DefaultLayerIds.Popup,
                        IsEscapable = source.IsEscapable
                    };
                }

                if (type == UISystemConfig.UIPrefabType.Widget)
                {
                    var widget = new UISystemConfig.UIWidgetEntry
                    {
                        Id = source.Id,
                        AssetKey = source.AssetKey,
                        LayerId = UISystemConfig.DefaultLayerIds.WorldOverlay
                    };
                    if (source is UISystemConfig.UIWidgetEntry sourceWidget)
                    {
                        widget.IsPooled = sourceWidget.IsPooled;
                        widget.IgnoreDepth = sourceWidget.IgnoreDepth;
                        widget.InitialCapacity = sourceWidget.InitialCapacity;
                        widget.MinCachedCount = sourceWidget.MinCachedCount;
                    }

                    return widget;
                }

                return new UISystemConfig.UIPanelEntry
                {
                    Id = source.Id,
                    AssetKey = source.AssetKey,
                    LayerId = UISystemConfig.DefaultLayerIds.Normal,
                    IsEscapable = source.IsEscapable
                };
            }
        }

        VisualElement CreateSceneMappingSection()
        {
            var wrapper = CreateSection("Scene -> Scope Mappings", out var content);

            var scanButton = CreateActionButton(
                "Sync Build Scenes",
                () =>
                {
                    RecordConfig("Sync Build Scenes");

                    var validScenes = new HashSet<string>();
                    foreach (var scene in EditorBuildSettings.scenes)
                    {
                        if (scene.enabled)
                        {
                            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                            if (!string.IsNullOrEmpty(sceneName))
                            {
                                validScenes.Add(sceneName);
                                if (m_Config.SceneMappings.Find(m => m.SceneName == sceneName) == null)
                                {
                                    m_Config.SceneMappings.Add(
                                        new UISystemConfig.SceneScopeMapping { SceneName = sceneName,
                                                                               ScopeId =
                                                                                   UISystemConfig.DefaultScopeIds.None });
                                }
                            }
                        }
                    }

                    // Remove mappings for scenes that are no longer enabled or in build settings
                    for (int i = m_Config.SceneMappings.Count - 1; i >= 0; i--)
                    {
                        if (!validScenes.Contains(m_Config.SceneMappings[i].SceneName))
                        {
                            m_Config.SceneMappings.RemoveAt(i);
                        }
                    }

                    SaveConfig();
                    rootVisualElement.schedule.Execute(() => RefreshWindow());
                },
                false);
            scanButton.style.marginBottom = 8f;
            scanButton.AddToClassList("button-list-add");
            content.Add(scanButton);

            var tableRows = new VisualElement();
            tableRows.AddToClassList("ui-table-rows");
            content.Add(tableRows);

            var header = new VisualElement();
            header.AddToClassList("ui-layer-header-row");
            // spacer removed
            var nameHeaderCell =
                CreateLayerCell("ui-layer-name-cell", CreateColumnHeader("Scene Name (From Build Settings)", false));
            nameHeaderCell.style.width = 300f;
            nameHeaderCell.style.flexShrink = 0;
            nameHeaderCell.style.flexGrow = 0;
            header.Add(nameHeaderCell);

            var scopeHeader = CreateColumnHeader("Bound Scope", false);
            var scopeCell = new VisualElement();
            scopeCell.style.width = 160f; // 限制 Scope 宽度
            scopeCell.style.flexShrink = 0;
            scopeCell.style.paddingLeft = 14f;
            scopeCell.Add(scopeHeader);
            header.Add(scopeCell);

            var headerSpacer = new VisualElement();
            headerSpacer.style.flexGrow = 1;
            header.Add(headerSpacer);

            var removeHeaderCell = CreateLayerCell("ui-layer-remove-cell", CreateColumnHeader(string.Empty, true));
            removeHeaderCell.style.marginRight = 10f;
            header.Add(removeHeaderCell);

            tableRows.Add(header);

            var availableScopes = new List<string>(m_Config.Scopes);
            if (availableScopes.Count == 0)
                availableScopes.Add(UISystemConfig.DefaultScopeIds.None);

            var listView = new ListView();
            listView.itemsSource = m_Config.SceneMappings;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.showBorder = false;
            listView.selectionType = SelectionType.None;
            listView.style.flexGrow = 1;
            listView.itemIndexChanged += (src, dst) =>
            {
                SaveConfig();
                rootVisualElement.schedule.Execute(() => RefreshWindow());
            };

            listView.makeItem = () => new VisualElement();
            listView.bindItem = (element, i) =>
            {
                element.Clear();
                int index = i;
                var mapping = m_Config.SceneMappings[index];

                var row = new VisualElement();
                row.AddToClassList("ui-sub-card");
                row.AddToClassList(index % 2 == 0 ? "row-even" : "row-odd");

                var line = new VisualElement();
                line.AddToClassList("ui-layer-line");

                line.style.alignItems = Align.Center;

                var nameLabel = new Label(mapping.SceneName);
                nameLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                var nameCell = CreateLayerCell("ui-layer-name-cell", nameLabel);
                nameCell.style.width = 300f;
                nameCell.style.flexShrink = 0;
                nameCell.style.flexGrow = 0;
                line.Add(nameCell);

                var scopePopup = new PopupField<string>(
                    availableScopes, availableScopes.Contains(mapping.ScopeId) ? mapping.ScopeId : availableScopes[0]);
                scopePopup.labelElement.style.display = DisplayStyle.None;
                scopePopup.AddToClassList("ui-table-dropdown");

                scopePopup.style.flexGrow = 1;
                scopePopup.RegisterValueChangedCallback(evt =>
                                                        {
                                                            RecordConfig("Edit Scene Scope Mapping");
                                                            mapping.ScopeId = evt.newValue;
                                                            SaveConfig();
                                                        });
                var scopeCellRow = new VisualElement();
                scopeCellRow.style.width = 160f; // 限制宽幅
                scopeCellRow.style.flexShrink = 0;
                scopeCellRow.style.paddingLeft = 10f;                  // 左侧一点缩进
                scopeCellRow.style.justifyContent = Justify.FlexStart; // 改为左对齐更符合阅读习惯
                scopeCellRow.Add(scopePopup);
                line.Add(scopeCellRow);

                var rowSpacer = new VisualElement();
                rowSpacer.style.flexGrow = 1;
                line.Add(rowSpacer);

                var removeCell =
                    CreateLayerCell("ui-layer-remove-cell", CreateIconButton("Remove Mapping", GetTrashIcon(),
                                                                             () =>
                                                                             {
                                                                                 RecordConfig("Remove Mapping");
                                                                                 m_Config.SceneMappings.RemoveAt(index);
                                                                                 SaveConfig();
                                                                                 RefreshWindow();
                                                                             }));
                removeCell.style.marginRight = 10f;
                line.Add(removeCell);

                row.Add(line);
                element.Add(row);
            };
            tableRows.Add(listView);

            if (m_Config.SceneMappings.Count == 0)
            {
                listView.style.display = DisplayStyle.None; // 隐藏自带的 "List is empty" 区域

                var emptyLabel = new Label("Click 'Sync Build Scenes' to load scenes.");
                emptyLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                emptyLabel.style.paddingLeft = 10f;
                emptyLabel.style.paddingTop = 0f;
                emptyLabel.style.paddingBottom = 0f;
                emptyLabel.style.marginTop = 0f;
                emptyLabel.style.marginBottom = 0f;
                emptyLabel.style.height = 24f;
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                tableRows.Add(emptyLabel);
            }

            return wrapper;
        }

        VisualElement CreateElementScopesSection()
        {
            var wrapper = new VisualElement();

            // Sync Scopes
            var syncAction = new System.Action(() =>
                                               {
                                                   RecordConfig("Sync Element Scopes");
                                                   var validScopes = new HashSet<string>(m_Config.Scopes);

                                                   SyncScopeGroups(validScopes);
                                                   RemoveInvalidElementSelections(CreateValidElementIdSet());

                                                   SaveConfig();
                                                   rootVisualElement.schedule.Execute(() => RefreshWindow());
                                               });

            var syncButton = CreateActionButton("Sync Scopes with Definition", syncAction, false);
            syncButton.AddToClassList("button-list-add");
            syncButton.style.marginBottom = 0f;
            wrapper.Add(syncButton);

            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            wrapper.Add(scrollView);

            var availableElements = new List<string>();
            foreach (var p in m_Config.Panels)
                availableElements.Add(p.Id);
            foreach (var window in m_Config.Windows)
                availableElements.Add(window.Id);
            foreach (var popup in m_Config.Popups)
                availableElements.Add(popup.Id);
            if (availableElements.Count == 0)
                availableElements.Add(UISystemConfig.DefaultScopeIds.None);

            VisualElement CreateElementGroupTable(
                string title, string columnTitle,
                IList<UISystemConfig.ScopeElementSelection> list)
            {
                var container = CreateSection(title, out var content);
                container.style.marginTop = 10f;

                var tableRows = new VisualElement();
                tableRows.AddToClassList("ui-table-rows");
                content.Add(tableRows);

                var header = new VisualElement();
                header.AddToClassList("ui-layer-header-row");
                var nameHeaderCell = CreateLayerCell("ui-layer-name-cell", CreateColumnHeader(columnTitle, false));
                nameHeaderCell.style.width = 240f;
                nameHeaderCell.style.flexShrink = 0;
                nameHeaderCell.style.flexGrow = 0;
                header.Add(nameHeaderCell);

                var preloadHeader = CreateColumnHeader("Preload", true);
                preloadHeader.style.width = 80f;
                preloadHeader.style.flexShrink = 0f;
                header.Add(preloadHeader);

                var openHeader = CreateColumnHeader("Open On Enter", true);
                openHeader.style.width = 120f;
                openHeader.style.flexShrink = 0f;
                header.Add(openHeader);

                var spacing = new VisualElement();
                spacing.style.flexGrow = 1;
                header.Add(spacing);

                var removeHeaderCell = CreateLayerCell("ui-layer-remove-cell", CreateColumnHeader(string.Empty, true));
                removeHeaderCell.style.marginRight = 10f;
                header.Add(removeHeaderCell);

                tableRows.Add(header);

                var listView = new ListView();
                listView.itemsSource = (System.Collections.IList)list;
                listView.reorderable = true;
                listView.reorderMode = ListViewReorderMode.Animated;
                listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                listView.showBorder = false;
                listView.selectionType = SelectionType.None;
                listView.style.flexGrow = 1;
                listView.itemIndexChanged += (src, dst) =>
                {
                    SaveConfig();
                    rootVisualElement.schedule.Execute(() => RefreshWindow());
                };

                listView.makeItem = () => new VisualElement();
                listView.bindItem = (element, i) =>
                {
                    element.Clear();
                    int index = i;
                    var selection = list[index];

                    var row = new VisualElement();
                    row.AddToClassList("ui-sub-card");
                    row.AddToClassList(index % 2 == 0 ? "row-even" : "row-odd");

                    var line = new VisualElement();
                    line.AddToClassList("ui-layer-line");
                    line.style.alignItems = Align.Center;

                    var elementPopup = new PopupField<string>(
                        availableElements, availableElements.Contains(selection.ElementId)
                                                ? selection.ElementId
                                                : availableElements[0]);
                    elementPopup.labelElement.style.display = DisplayStyle.None;
                    elementPopup.AddToClassList("ui-table-dropdown");
                    elementPopup.RegisterValueChangedCallback(evt =>
                                                            {
                                                                RecordConfig("Edit Element Assignment");
                                                                selection.ElementId = evt.newValue;
                                                                SaveConfig();
                                                            });
                    var nameCell = CreateLayerCell("ui-layer-name-cell", elementPopup);
                    nameCell.style.width = 240f;
                    nameCell.style.flexShrink = 0;
                    nameCell.style.flexGrow = 0;
                    line.Add(nameCell);

                    var togglePreload = new Toggle { value = selection.Preload };
                    togglePreload.RegisterValueChangedCallback(evt =>
                                                               {
                                                                   RecordConfig("Edit Preload");
                                                                   selection.Preload = evt.newValue;
                                                                   SaveConfig();
                                                               });
                    var preloadCell = new VisualElement();
                    preloadCell.style.width = 80f;
                    preloadCell.style.flexShrink = 0f;
                    preloadCell.style.justifyContent = Justify.Center;
                    preloadCell.style.alignItems = Align.Center;
                    preloadCell.Add(togglePreload);
                    line.Add(preloadCell);

                    var toggleOpen = new Toggle { value = selection.OpenOnEnter };
                    toggleOpen.RegisterValueChangedCallback(evt =>
                                                            {
                                                                RecordConfig("Edit Open On Enter");
                                                                selection.OpenOnEnter = evt.newValue;
                                                                SaveConfig();
                                                            });
                    var openCell = new VisualElement();
                    openCell.style.width = 120f;
                    openCell.style.flexShrink = 0f;
                    openCell.style.justifyContent = Justify.Center;
                    openCell.style.alignItems = Align.Center;
                    openCell.Add(toggleOpen);
                    line.Add(openCell);

                    var rowSpace = new VisualElement();
                    rowSpace.style.flexGrow = 1;
                    line.Add(rowSpace);

                    var removeCell =
                        CreateLayerCell("ui-layer-remove-cell", CreateIconButton("Remove Element", GetTrashIcon(),
                                                                                 () =>
                                                                                 {
                                                                                     RecordConfig("Remove Element");
                                                                                     list.RemoveAt(index);
                                                                                     SaveConfig();
                                                                                     RefreshWindow();
                                                                                 }));
                    removeCell.style.marginRight = 10f;
                    line.Add(removeCell);

                    row.Add(line);
                    element.Add(row);
                };
                tableRows.Add(listView);

                if (list.Count == 0)
                {
                    listView.style.display = DisplayStyle.None; // 隐藏自带的空提示
                    var emptyLabel = new Label("No elements assigned.");
                    emptyLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                    emptyLabel.style.paddingLeft = 10f;
                    emptyLabel.style.height = 24f;
                    emptyLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                    tableRows.Add(emptyLabel);
                }

                var addButton = CreateActionButton(
                    "+ Add Element",
                    () =>
                    {
                        RecordConfig("Add Scope Element");
                        list.Add(new UISystemConfig.ScopeElementSelection { ElementId = availableElements[0], Preload = true,
                                                                          OpenOnEnter = false });
                        SaveConfig();
                        RefreshWindow();
                    },
                    false);
                addButton.AddToClassList("button-add-item");
                content.Add(addButton);

                return container;
            }

            scrollView.Add(
                CreateElementGroupTable("Global Elements (Always Active)", "Global Element", m_Config.GlobalElements));

            foreach (var group in m_Config.ScopeElements)
            {
                scrollView.Add(
                    CreateElementGroupTable($"Scope: {group.ScopeId}", $"[{group.ScopeId}] Element", group.Elements));
            }

            return wrapper;
        }

        void BuildPreview()
        {
            if (m_Config == null)
            {
                var msg = new Label("Please select a UISystemConfig asset in the Project window or click 'New Asset'.");
                msg.style.color = new Color(0.5f, 0.5f, 0.5f);
                msg.style.alignSelf = Align.Center;
                m_ContentRoot.Add(msg);
                return;
            }
            if (!UISystemConfigValidator.Validate(m_Config, m_Errors, m_Warnings))
            {
                RefreshWindow();
                return;
            }

            RectTransform targetRoot = UISystemPreviewBuilder.RebuildPreview(m_Config, null);
            if (targetRoot != null)
            {
                Selection.activeGameObject = targetRoot.gameObject;
            }
        }

        void RecordConfig(string actionName) => Undo.RecordObject(m_Config, actionName);
        void SaveConfig()
        {
            EditorUtility.SetDirty(m_Config);
            AssetDatabase.SaveAssets();
        }

        void ApplyStyleSheet()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);
        }

        void ApplyWindowBounds()
        {
            minSize = k_WindowSize;
            maxSize = k_WindowSize;
        }

        static UISystemConfig CreateConfigAsset()
        {
            EnsureFolderExists(k_DefaultAssetDirectory);
            var config = CreateInstance<UISystemConfig>();
            config.ResetToDefault();
            string assetPath =
                AssetDatabase.GenerateUniqueAssetPath(k_DefaultAssetDirectory + "/UISystemConfig.asset");
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = config;
            return config;
        }

        HashSet<string> CreateValidElementIdSet()
        {
            var validElements = new HashSet<string>();
            for (int i = 0; i < m_Config.Panels.Count; i++)
                validElements.Add(m_Config.Panels[i].Id);
            for (int i = 0; i < m_Config.Windows.Count; i++)
                validElements.Add(m_Config.Windows[i].Id);
            for (int i = 0; i < m_Config.Popups.Count; i++)
                validElements.Add(m_Config.Popups[i].Id);

            return validElements;
        }

        void RemoveInvalidElementSelections(HashSet<string> validElements)
        {
            for (int i = m_Config.GlobalElements.Count - 1; i >= 0; i--)
            {
                string elementId = m_Config.GlobalElements[i].ElementId;
                if (!validElements.Contains(elementId) || elementId == UISystemConfig.DefaultScopeIds.None)
                {
                    m_Config.GlobalElements.RemoveAt(i);
                }
            }

            for (int groupIndex = 0; groupIndex < m_Config.ScopeElements.Count; groupIndex++)
            {
                List<UISystemConfig.ScopeElementSelection> elements = m_Config.ScopeElements[groupIndex].Elements;
                for (int elementIndex = elements.Count - 1; elementIndex >= 0; elementIndex--)
                {
                    string elementId = elements[elementIndex].ElementId;
                    if (!validElements.Contains(elementId) || elementId == UISystemConfig.DefaultScopeIds.None)
                    {
                        elements.RemoveAt(elementIndex);
                    }
                }
            }
        }

        void SyncScopeGroups(HashSet<string> validScopes)
        {
            for (int i = m_Config.ScopeElements.Count - 1; i >= 0; i--)
            {
                string scopeId = m_Config.ScopeElements[i].ScopeId;
                if (!validScopes.Contains(scopeId) || scopeId == UISystemConfig.DefaultScopeIds.None)
                {
                    m_Config.ScopeElements.RemoveAt(i);
                }
            }

            foreach (string scope in validScopes)
            {
                if (scope == UISystemConfig.DefaultScopeIds.None)
                {
                    continue;
                }

                if (m_Config.ScopeElements.Find(group => group.ScopeId == scope) == null)
                {
                    m_Config.ScopeElements.Add(new UISystemConfig.ScopeElementGroup { ScopeId = scope });
                }
            }
        }

        static void EnsureFolderExists(string assetFolderPath)
        {
            string[] segments = assetFolderPath.Split('/');
            string currentPath = segments[0];

            for (int i = 1; i < segments.Length; i++)
            {
                string nextPath = currentPath + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }
                currentPath = nextPath;
            }
        }

        static HelpBox CreateHelpBox(string text, HelpBoxMessageType messageType)
        {
            var helpBox = new HelpBox(text, messageType);
            return helpBox;
        }

        static TextField CreateTextField(string label, string value, System.Action<string> onChanged)
        {
            var field = new TextField(label) { value = value };
            if (string.IsNullOrEmpty(label))
                field.labelElement.style.display = DisplayStyle.None;
            field.AddToClassList("ui-flat-field");
            field.style.flexGrow = 1;
            field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return field;
        }

        static VisualElement CreateResolutionField(string label, Vector2 value, System.Action<Vector2> onChanged)
        {
            var root = new VisualElement();
            root.AddToClassList("ui-flat-field");
            root.style.flexDirection = FlexDirection.Row;
            root.style.flexGrow = 1;

            var title = new Label(label);
            title.AddToClassList("unity-base-field__label");
            root.Add(title);

            var xLabel = new Label("X");
            xLabel.AddToClassList("ui-resolution-axis-label");
            root.Add(xLabel);

            var xField = new IntegerField { value = Mathf.RoundToInt(value.x) };
            xField.AddToClassList("ui-resolution-field");
            root.Add(xField);

            var yLabel = new Label("Y");
            yLabel.AddToClassList("ui-resolution-axis-label");
            root.Add(yLabel);

            var yField = new IntegerField { value = Mathf.RoundToInt(value.y) };
            yField.AddToClassList("ui-resolution-field");
            root.Add(yField);

            void Apply() => onChanged(new Vector2(xField.value, yField.value));

            xField.RegisterValueChangedCallback(
                _ => Apply());
            yField.RegisterValueChangedCallback(
                _ => Apply());
            return root;
        }

        static VisualElement CreateFloatField(string label, float value, System.Action<float> onChanged)
        {
            var field = new FloatField(label) { value = value };
            field.AddToClassList("ui-flat-field");
            field.style.flexGrow = 1;
            field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return field;
        }

        static VisualElement CreateSliderField(string label, float value, System.Action<float> onChanged)
        {
            var root = new VisualElement();
            root.AddToClassList("ui-flat-field");
            root.style.flexDirection = FlexDirection.Row;
            root.style.flexGrow = 1;

            var title = new Label(label);
            title.AddToClassList("unity-base-field__label");
            root.Add(title);

            var field = new Slider(0f, 1f) { value = value };
            field.AddToClassList("ui-inline-slider");
            field.labelElement.style.display = DisplayStyle.None;

            // Disable child fields flex interactions that breaks custom layout
            field.style.flexDirection = FlexDirection.Row;

            var valueLabel = new Label(value.ToString("0.00"));
            valueLabel.AddToClassList("ui-inline-value");

            field.RegisterValueChangedCallback(evt =>
                                               {
                                                   valueLabel.text = evt.newValue.ToString("0.00");
                                                   onChanged(evt.newValue);
                                               });

            var sliderWrap = new VisualElement();
            sliderWrap.AddToClassList("ui-inline-slider-wrap");
            // Ensure slider itself occupies full height to accept drag inputs correctly
            field.style.flexGrow = 1f;
            sliderWrap.Add(field);
            root.Add(sliderWrap);
            root.Add(valueLabel);
            return root;
        }

        static VisualElement CreateEnumField<TEnum>(string label, TEnum value, System.Action<System.Enum> onChanged)
            where TEnum : System.Enum
        {
            var field = new EnumField(label, value);
            field.AddToClassList("ui-flat-field");
            field.style.flexGrow = 1;
            field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return field;
        }

        static Label CreateColumnHeader(string text, bool centerAligned = false)
        {
            var label = new Label(text);
            label.AddToClassList("ui-layer-column-header");
            label.style.unityTextAlign = centerAligned ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            label.style.paddingLeft = 0f;
            label.style.marginLeft = 0f;
            return label;
        }

        static VisualElement CreateLayerCell(string className, VisualElement content)
        {
            var cell = new VisualElement();
            cell.AddToClassList(className);
            cell.Add(content);
            return cell;
        }

        static Button CreateIconButton(string tooltip, Texture icon, System.Action onClick)
        {
            var button = new Button(onClick);
            button.tooltip = tooltip;
            button.AddToClassList("ui-icon-button");

            if (icon != null)
            {
                var image = new Image { image = icon, scaleMode = ScaleMode.ScaleToFit };
                image.AddToClassList("ui-icon-image");
                button.Add(image);
                return button;
            }

            var fallback = new Label("×");
            fallback.AddToClassList("ui-icon-fallback");
            button.Add(fallback);
            return button;
        }

        static Texture GetTrashIcon()
        {
            string[] iconNames = { EditorGUIUtility.isProSkin ? "d_TreeEditor.Trash" : "TreeEditor.Trash",
                                   "TreeEditor.Trash" };
            foreach (var name in iconNames)
            {
                var content = EditorGUIUtility.IconContent(name);
                if (content != null && content.image != null)
                    return content.image;
            }
            return null;
        }
        static Button CreateActionButton(string text, System.Action onClick, bool isPrimary)
        {
            var button = new Button(onClick) { text = text };
            button.AddToClassList("ui-action-button");
            button.AddToClassList(isPrimary ? "button-primary" : "button-secondary");
            return button;
        }

        static ObjectField CreateObjectField(string label, System.Type type, Object value,
                                             System.Action<Object> onChanged)
        {
            var field = new ObjectField(label) { objectType = type, allowSceneObjects = false, value = value };
            if (string.IsNullOrEmpty(label))
                field.labelElement.style.display = DisplayStyle.None;
            field.AddToClassList("ui-custom-object-field");
            field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return field;
        }

        static UISystemConfig.UIPrefabType ResolvePrefabType(string assetPath)
        {
            string normalizedPath = assetPath.Replace('\\', '/');
            if (normalizedPath.Contains("/Windows/"))
                return UISystemConfig.UIPrefabType.Window;
            if (normalizedPath.Contains("/Panels/"))
                return UISystemConfig.UIPrefabType.Panel;
            if (normalizedPath.Contains("/Popups/"))
                return UISystemConfig.UIPrefabType.Popup;
            if (normalizedPath.Contains("/Widgets/"))
                return UISystemConfig.UIPrefabType.Widget;

            throw new IOException("UI Prefab 必须位于 Windows、Panels、Popups 或 Widgets 目录：" +
                                  assetPath);
        }

        static string BuildAssetAddress(string assetPath)
        {
            return Path.GetFileNameWithoutExtension(assetPath);
        }

        static VisualElement CreateSection(string title, out VisualElement content, VisualElement trailing = null)
        {
            var container = new VisualElement();

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("ui-section-title-label");
            header.Add(titleLabel);

            if (trailing != null)
                header.Add(trailing);

            container.Add(header);

            content = new VisualElement();
            container.Add(content);

            return container;
        }
    }
}
#endif
