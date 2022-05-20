using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetFavorites
{
    public class AssetElement : FavElement
    {
        public AssetData AssetData { get; private set; }
        public Object LoadedObject { get; private set; }
        public Texture AssetIcon { get; private set; }
        public string AssetName { get; private set; } = "";
        public bool AssetIsValid { get { return LoadedObject != null; } }

        public AssetElement(AssetData assetData)
        {
            AssetData = assetData;
            id = AssetData.Id;
            LoadedObject = AssetDatabase.LoadAssetAtPath(AssetData.Path, typeof(Object));
            AssetIcon = AssetDatabase.GetCachedIcon(AssetData.Path);
            AssetName = Path.GetFileNameWithoutExtension(AssetData.Path);
        }

        public override void OnDraw(Rect rect, List<string> searchArgs = null)
        {
            if (AssetIsValid)
            {
                EditorGUI.LabelField(rect, new GUIContent()
                {
                    image = AssetIcon,
                    text = ApplySearchBoldingToString(AssetName, searchArgs),
                    tooltip = AssetData.Path
                }, FavsWindowResources.RichTextStyle);
            }
            else
            {
                EditorGUI.LabelField(rect, new GUIContent()
                {
                    text = $"<color=red>[MISSING]</color> {ApplySearchBoldingToString(AssetData.Path, searchArgs)}",
                    tooltip = AssetData.Path
                }, FavsWindowResources.RichTextStyle);
            }
        }
    }
}