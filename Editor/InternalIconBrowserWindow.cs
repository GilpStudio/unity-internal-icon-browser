using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class InternalIconBrowserWindow : EditorWindow
{
    private SearchField _searchField;
    private BrowserTreeView _treeView;

    [SerializeField]
    private TreeViewState _treeState;

    [SerializeField]
    private MultiColumnHeaderState _headerState;

    [NonSerialized]
    private bool _isInitialized;

    private Dictionary<int, Texture2D> _iconCache;
    private MultiColumnHeader _header;

    [MenuItem("Tools/Internal Icon Browser")]
    private static void OpenWindow()
    {
        var window = GetWindow<InternalIconBrowserWindow>();
        
        window.minSize = new Vector2(300f, 200f);
        window.titleContent = new GUIContent(EditorGUIUtility.IconContent("FilterByType"));
        window.titleContent.text = "Internal Icon Browser";
        
        window.Show();
    }

    private void InitIfNeeded()
    {
        if (_isInitialized)
        {
            return;
        }

        _iconCache = new Dictionary<int, Texture2D>();

        _searchField = new SearchField();
        
        RebuildTreeViewHeader();
        
        _treeState = _treeState ?? new TreeViewState();

        _treeView = new BrowserTreeView(_treeState, new MultiColumnHeader(_headerState));
        _treeView.Reload();

        RefreshIcons();

        _isInitialized = true;
    }


    private void RebuildTreeViewHeader()
    {
        var headerState = new MultiColumnHeaderState(new[]
        {
            new MultiColumnHeaderState.Column
            {
                headerContent = EditorGUIUtility.IconContent("FilterByType"),
                autoResize = false,
                minWidth = 30f,
                width =  30f,
                headerTextAlignment = TextAlignment.Center,
                allowToggleVisibility = false,
                canSort = false,
            },

            new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Icon Content"),
                minWidth = 300,
                width = 300,
                autoResize = true,
                allowToggleVisibility = false,
                canSort = false,
            }
        });

        if (_header != null)
        {
            _header.state = headerState;
        }
        else
        {
            _header = new MultiColumnHeader(headerState);
        }

        if (MultiColumnHeaderState.CanOverwriteSerializedFields(_headerState, headerState))
        {
            MultiColumnHeaderState.OverwriteSerializedFields(_headerState, headerState);
        }

        _headerState = headerState;
    }

    private void OnGUI()
    {
        InitIfNeeded();

        var width = _header.state.widthOfAllVisibleColumns;
        var delta = width - position.width;
        _header.state.columns[1].width = _header.state.columns[1].width - delta - 20f;
        
        GUILayout.BeginArea(new Rect(Vector2.zero, position.size));

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        _treeView.searchString = _searchField.OnGUI(_treeView.searchString);

        GUILayout.EndHorizontal();
        
        _treeView.OnGUI(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));

        GUILayout.EndArea();
    }

    void RefreshIcons()
    {
        _iconCache.Clear();

        Texture2D[] t = Resources.FindObjectsOfTypeAll<Texture2D>();
        foreach (Texture2D x in t)
        {
            if (x.name.Length == 0)
                continue;

            if (x.hideFlags != HideFlags.HideAndDontSave &&
                x.hideFlags != (HideFlags.HideInInspector | HideFlags.HideAndDontSave))
                continue;

            if (!EditorUtility.IsPersistent(x))
                continue;

            /* This is the *only* way I have found to confirm the icons are indeed unity builtin. Unfortunately
             * it uses LogError instead of LogWarning or throwing an Exception I can catch. So make it shut up. */
            Debug.unityLogger.logEnabled = false;
            GUIContent gc = EditorGUIUtility.IconContent(x.name);
            Debug.unityLogger.logEnabled = true;

            if (gc == null)
                continue;
            if (gc.image == null)
                continue;

            var hash = x.name.GetHashCode();
            if (!_iconCache.ContainsKey(hash))
            {
                _iconCache.Add(hash, x);
            }
        }

        _treeView.items.Clear();
        foreach (var pair in _iconCache.OrderBy(i => i.Value.name))
        {
            _treeView.items.Add(new TreeViewItem
            {
                id = pair.Key,
                depth = 0,
                icon = pair.Value,
                displayName = pair.Value.name,
            });
        }

        _treeView.Reload();

        Resources.UnloadUnusedAssets();
        GC.Collect();
        Repaint();
    }

    class BrowserTreeView : TreeView
    {
        private int _iconColumnWidth = 0;
        public List<TreeViewItem> items;

        public BrowserTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
        {
            items = new List<TreeViewItem>();
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem {depth = -1, children = items};
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            MultiColumnHeaderState.Column column = multiColumnHeader.state.columns[0];

            var width = Mathf.Min(column.width - 2 * cellMargin, item.icon.width);
            var height = width * item.icon.height / item.icon.width;

            return Mathf.Max(height, rowHeight);
        }

        public override void OnGUI(Rect rect)
        {
            MultiColumnHeaderState.Column column = multiColumnHeader.state.columns[0];
            int currentWidth = (int) column.width;

            if (_iconColumnWidth != currentWidth)
            {
                RefreshCustomRowHeights();
            }

            base.OnGUI(rect);
        }

        Rect CenterRect(Rect rect, float width, float height)
        {
            if (width < rect.width)
            {
                var diff = rect.width - width;
                rect.xMin += diff * .5f;
                rect.xMax -= diff * .5f;
            }
                
            if (height < rect.height)
            {
                var diff = rect.height - height;
                rect.yMin += diff * .5f;
                rect.yMax -= diff * .5f;
            }

            return rect;
        }
        
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;

            Rect iconRect = CenterRect(args.GetCellRect(0), item.icon.width, item.icon.height);
            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
            
            Rect nameRect = args.GetCellRect(1);
            EditorGUI.LabelField(nameRect, item.displayName);
            
            if (nameRect.Contains(Event.current.mousePosition) 
                && Event.current.type == EventType.MouseDown && Event.current.clickCount > 1) {
                
                EditorGUIUtility.systemCopyBuffer = item.displayName;
                Debug.Log($"'{item.displayName}' copied to clipboard.");
            }
        }
    }
}