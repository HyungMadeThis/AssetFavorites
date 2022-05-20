using UnityEngine;
using UnityEditor;
using System;

namespace AssetFavorites
{
    public class FolderIconPicker : PopupWindowContent
    {
        private static readonly int ICONS_PER_ROW = 5;
        public static readonly Vector2 POPUP_DIMENSIONS = new Vector2(240f, 110f);

        private FolderElement m_folderElement;
        private Action m_onFolderIconChanged;

        public FolderIconPicker(FolderElement folderElement, Action onFolderIconChanged)
        {
            m_folderElement = folderElement;
            m_onFolderIconChanged = onFolderIconChanged;
        }

        public override Vector2 GetWindowSize()
        {
            return POPUP_DIMENSIONS;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            DrawFolderElement();
            FolderIcon[] icons = (FolderIcon[])Enum.GetValues(typeof(FolderIcon));
            for (int i = 0; i < icons.Length; i++)
            {
                if (i % ICONS_PER_ROW == 0)
                {
                    GUILayout.BeginHorizontal();
                }

                if (GUILayout.Button(FavsWindowResources.GetFolderIconTexture(icons[i])))
                {
                    m_folderElement.FolderData.FolderIcon = icons[i];
                    m_onFolderIconChanged?.Invoke();
                }

                if (i % ICONS_PER_ROW == ICONS_PER_ROW - 1 || i == icons.Length - 1)
                {
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawFolderElement()
        {
            GUILayout.BeginHorizontal(new GUIStyle("box"));
            GUILayout.Label(new GUIContent()
            {
                image = FavsWindowResources.GetFolderIconTexture(m_folderElement.FolderData.FolderIcon),
                text = m_folderElement.FolderName
            }, GUILayout.Height(18f));
            GUILayout.EndHorizontal();
        }
    }
}