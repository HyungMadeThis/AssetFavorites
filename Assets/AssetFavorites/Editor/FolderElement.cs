using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetFavorites
{
    public class FolderElement : FavElement
    {
        public FolderData FolderData { get; private set; }
        public string FolderName
        {
            get { return FolderData.Name; }
            set
            {
                FolderData.Name = value;
                displayName = FolderData.Name;
            }
        }

        public FolderElement(FolderData folderData)
        {
            FolderData = folderData;
            id = FolderData.Id;

            if (FolderData.Id == 0)
            {
                depth = -1;
                displayName = "root";
            }
            else
            {
                displayName = FolderData.Name;
            }
        }

        public override void OnDraw(Rect rect, List<string> searchArgs = null)
        {
            EditorGUI.LabelField(rect, new GUIContent()
            {
                image = FavsWindowResources.GetFolderIconTexture(FolderData.FolderIcon),
                text = ApplySearchBoldingToString(FolderName, searchArgs)
            }, FavsWindowResources.RichTextStyle);
        }

        public int GetSubFolderCount()
        {
            return FolderData.GetSubFolderCount();
        }
        public int GetSubAssetCount()
        {
            return FolderData.GetSubAssetCount();
        }
    }
}