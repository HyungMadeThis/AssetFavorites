using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

namespace AssetFavorites
{
    public abstract class FavElement : TreeViewItem
    {
        public static readonly float IconWidth = 16f;

        public virtual void OnDraw(Rect rect, List<string> searchArgs = null)
        {
            EditorGUI.LabelField(rect, $"{displayName} ({id})");
        }

        public FolderElement GetParentFolder()
        {
            return parent as FolderElement;
        }

        protected string ApplySearchBoldingToString(string text, List<string> searchArgs)
        {
            if(searchArgs == null || searchArgs.Count == 0)
            {
                return text;
            }
            // NOTE: Maybe one day I'll get around to decorating the display text with rich text to bold the search arg hits.
            // I'll keep the wiring in place just in case ;) <b>text<\b>
            return text;
        }
    }

    public class SearchRootElement : FavElement
    {
        public SearchRootElement()
        {
            id = 0;
            depth = -1;
        }
    }

    public class NoSearchResultsElement : FavElement
    {
        public NoSearchResultsElement()
        {
            id = -1;
        }
        public override void OnDraw(Rect rect, List<string> searchArgs = null)
        {
            EditorGUI.LabelField(rect, "No Search Results.");
        }
    }

    public class EmptyElement : FavElement
    {
        public EmptyElement()
        {
            id = -1;
        }
        public override void OnDraw(Rect rect, List<string> searchArgs = null) { }
    }
}