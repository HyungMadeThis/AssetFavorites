using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetFavorites
{
    public class FavoritesWindow : EditorWindow, IHasCustomMenu
    {
        private SearchField m_SearchField;
        private FavsTreeView m_treeView;
        [SerializeField]
        private FavsState m_state;

        [MenuItem("Window/Asset Favorites")]
        public static void OpenWindow()
        {
            FavoritesWindow window = CreateInstance<FavoritesWindow>();
            Texture folderTex = FavsWindowResources.GetFolderIconTexture(FolderIcon.Grey);
            window.titleContent = new GUIContent("Favorites", folderTex);
            window.Show();
        }

        private void OnEnable()
        {
            if (m_state == null)
            {
                m_state = new FavsState();
            }
            m_state.SubscribeToStateChangedExternally(OnStateChangedExternally);

            m_SearchField = new SearchField();
            m_treeView = new FavsTreeView(m_state);
        }

        private void OnDisable()
        {
            if (m_state != null)
            {
                m_state.UnsubscribeToStateChangedExternally(OnStateChangedExternally);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawToolbar();
            DrawContent();
            GUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            m_state.SearchString = m_SearchField.OnToolbarGUI(m_state.SearchString);
            if (EditorGUI.EndChangeCheck())
            {
                m_treeView.HandleSearchFieldChanged();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(4);

            if (m_state.IsSearching)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("searching...", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            DrawTreeView();
            GUILayout.Space(4);
            GUILayout.EndVertical();
        }

        private void DrawTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_treeView.OnGUI(rect);
        }

        private void OnStateChangedExternally()
        {
            m_state.ReloadData();
            m_treeView.Reload();
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Reset FavData"), false, () =>
            {
                m_state.ResetData();
                m_state.ReloadData();
                m_treeView.Reload();
            });
            menu.AddItem(new GUIContent("Debug FavData"), false, () =>
            {
                Debug.Log($"FavData:\n{JsonUtility.ToJson(m_state.FavsData, true)}");
            });
        }
    }
}