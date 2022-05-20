using System.Collections.Generic;
using UnityEngine;

namespace AssetFavorites
{
    public static class FavsWindowResources
    {
        private static Dictionary<FolderIcon, Texture> m_cachedFolderIcons;
        public static GUIStyle RichTextStyle;

        static FavsWindowResources()
        {
            m_cachedFolderIcons = new Dictionary<FolderIcon, Texture>()
            {
                [FolderIcon.Red] = Resources.Load<Texture>("AssetFavorites/Folder_Red"),
                [FolderIcon.Orange] = Resources.Load<Texture>("AssetFavorites/Folder_Orange"),
                [FolderIcon.Yellow] = Resources.Load<Texture>("AssetFavorites/Folder_Yellow"),
                [FolderIcon.Green] = Resources.Load<Texture>("AssetFavorites/Folder_Green"),
                [FolderIcon.Cyan] = Resources.Load<Texture>("AssetFavorites/Folder_Cyan"),
                [FolderIcon.Blue] = Resources.Load<Texture>("AssetFavorites/Folder_Blue"),
                [FolderIcon.Purple] = Resources.Load<Texture>("AssetFavorites/Folder_Purple"),
                [FolderIcon.Pink] = Resources.Load<Texture>("AssetFavorites/Folder_Pink"),
                [FolderIcon.Grey] = Resources.Load<Texture>("AssetFavorites/Folder_Grey"),
                [FolderIcon.Black] = Resources.Load<Texture>("AssetFavorites/Folder_Black")
            };

            RichTextStyle = new GUIStyle() { richText = true };
            RichTextStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f); //#C0C0C0
        }

        public static Texture GetFolderIconTexture(FolderIcon icon)
        {
            return m_cachedFolderIcons[icon];
        }
    }
}