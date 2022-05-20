using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetFavorites
{
    public class FavsTreeView : TreeView
    {
        private static string DRAG_NAME = "FavsDragData";
        private FavsState m_state;

        public FavsTreeView(FavsState state) : base(state.TreeState)
        {
            m_state = state;
            Reload();
        }

        #region Building TreeView
        protected override TreeViewItem BuildRoot()
        {
            FavElement root;
            if (!m_state.IsSearching) // Not searching. Show Regular tree.
            {
                root = GenerateTree(m_state.FavsData);
            }
            else // Searching. Show search.
            {
                root = GenerateSearchResults(m_state.FavsData, m_state.ParsedSearchString);
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            Rect toggleRect = args.rowRect;
            float indent = GetContentIndent(args.item);
            toggleRect.x += indent;
            toggleRect.width -= indent;
            (args.item as FavElement).OnDraw(toggleRect, m_state.ParsedSearchString);
        }
        private FavElement GenerateTree(FavsData favData)
        {
            // Build all Folder/Asset elements and cache them by Id.
            Dictionary<int, FolderElement> folderLookup = new Dictionary<int, FolderElement>();
            Dictionary<int, AssetElement> assetLookup = new Dictionary<int, AssetElement>();

            foreach (FolderData folderData in favData.GetFolderDatas())
            {
                FolderElement newFolderElement = new FolderElement(folderData);
                folderLookup.Add(folderData.Id, newFolderElement);
            }
            foreach (AssetData assetData in favData.GetAssetDatas())
            {
                AssetElement newAssetElement = new AssetElement(assetData);
                assetLookup.Add(assetData.Id, newAssetElement);
            }

            // Build out the tree
            foreach (FolderElement folderElement in folderLookup.Values)
            {
                foreach (int subFolderId in folderElement.FolderData.GetSubFolderIds())
                {
                    folderElement.AddChild(folderLookup[subFolderId]);
                }

                foreach (int subAssetId in folderElement.FolderData.GetSubAssetIds())
                {
                    folderElement.AddChild(assetLookup[subAssetId]);
                }
            }

            // Find the root element
            FolderElement rootElement = folderLookup[0];

            // If there are no elements, drop in a placeholder element.
            if (!rootElement.hasChildren)
            {
                rootElement.AddChild(new EmptyElement());
            }

            return rootElement;
        }
        private FavElement GenerateSearchResults(FavsData favData, List<string> searchArgs)
        {
            SearchRootElement rootElement = new SearchRootElement();
            List<ISearchable> results = new List<ISearchable>();

            foreach (FolderData folderData in favData.GetFolderDatas())
            {
                if(folderData.Id == 0)
                {
                    continue;
                }

                if (ElementIsSearchMatch(folderData, searchArgs))
                {
                    results.Add(folderData);
                }
            }
            foreach (AssetData assetData in favData.GetAssetDatas())
            {
                if (ElementIsSearchMatch(assetData, searchArgs))
                {
                    results.Add(assetData);
                }
            }
            foreach (ISearchable searchable in results.OrderBy(x => x.SearchableString))
            {
                if (searchable is FolderData folderData)
                {
                    rootElement.AddChild(new FolderElement(folderData));
                }
                else if (searchable is AssetData assetData)
                {
                    rootElement.AddChild(new AssetElement(assetData));
                }
            }

            if (results.Count == 0) // if no results
            {
                NoSearchResultsElement noSearchResultsElement = new NoSearchResultsElement();
                rootElement.AddChild(noSearchResultsElement);
            }
            return rootElement;
        }
        #endregion

        #region Elements Management
        private FavElement GetFavElementById(int id)
        {
            return FindItem(id, rootItem) as FavElement;
        }
        private void CreateSubFolderElement(int parentFolderId, string folderName = "New Folder")
        {
            FolderElement parentFolderElement = GetFavElementById(parentFolderId) as FolderElement;

            FolderData newFolderData = CreateFolderData(folderName);
            SetSubFolder(parentFolderElement.id, newFolderData.Id);
            SaveFavData(); //NOTE: A tiny bit inefficient because it makes all Fav windows refresh but not really a big deal.

            Reload();
            SetExpanded(parentFolderId, true);
            BeginRename(new FolderElement(newFolderData));
        }
        private AssetElement CreateAssetElement(string guid)
        {
            return new AssetElement(CreateAssetData(guid));
        }
        private void AddElements(List<string> pathsToAdd, FavElement parentElement, int insertAtIndex)
        {
            FolderElement parentElementAsFolder = parentElement as FolderElement;

            // If parentElement is an Asset, move everything to UNDER this asset element.
            if (parentElement is AssetElement ogParentAssetElement)
            {
                parentElementAsFolder = ogParentAssetElement.GetParentFolder();
                insertAtIndex = parentElementAsFolder.GetSubFolderCount() + GetIndexOfSubAsset(ogParentAssetElement, parentElementAsFolder) + 1;
            }

            List<AssetElement> newlyCreatedAssetElements = new List<AssetElement>();
            foreach (string path in pathsToAdd)
            {
                string assetGuid = AssetDatabase.AssetPathToGUID(path);
                AssetElement newAssetElement = CreateAssetElement(assetGuid);
                newlyCreatedAssetElements.Add(newAssetElement); // Do I actually need to create an AssetElement here? We're just gonna reload and recreate all of them anyway...
            }

            if (insertAtIndex == -1) // This is outside of any element. Put them at the bottom!
            {
                foreach (AssetElement assetElement in newlyCreatedAssetElements)
                {
                    SetSubAsset(parentElementAsFolder, assetElement);
                }
            }
            else if (insertAtIndex <= parentElementAsFolder.GetSubFolderCount() - 1) // This is in a folder. Put them at the top of the assets list.
            {
                insertAtIndex = 0;
                foreach (AssetElement assetElement in newlyCreatedAssetElements)
                {
                    SetSubAsset(parentElementAsFolder, assetElement, insertAtIndex++);
                }
            }
            else // This is inbetween asset elements. So we add things inbetween.
            {
                insertAtIndex -= parentElementAsFolder.GetSubFolderCount();

                foreach (AssetElement assetElement in newlyCreatedAssetElements)
                {
                    SetSubAsset(parentElementAsFolder, assetElement, insertAtIndex++);
                }
            }
            SaveFavData();

            SetExpanded(parentElementAsFolder.id, true);
            Reload();
        }
        private void MoveElements(List<FavElement> elementsToMove, FavElement parentElement, int insertAtIndex)
        {
            FolderElement parentElementAsFolder = parentElement as FolderElement;

            // If parentElement is an Asset, move everything to UNDER this asset element.
            if (parentElement is AssetElement ogParentAssetElement)
            {
                parentElementAsFolder = ogParentAssetElement.GetParentFolder();
                insertAtIndex = parentElementAsFolder.GetSubFolderCount() + GetIndexOfSubAsset(ogParentAssetElement, parentElementAsFolder) + 1;
            }

            // Cache all folder and asset elements
            List<FolderElement> folderElementsToMove = new List<FolderElement>();
            List<AssetElement> assetElementsToMove = new List<AssetElement>();
            foreach (FavElement elementToMove in elementsToMove)
            {
                if (elementToMove is FolderElement elementAsFolder)
                {
                    folderElementsToMove.Add(elementAsFolder);
                }
                else if (elementToMove is AssetElement elementAsAsset)
                {
                    assetElementsToMove.Add(elementAsAsset);
                }
            }

            // Validation step...
            // If you're trying to drag a folder into a subfolder...STOP!!!
            foreach (FolderElement folderElementToMove in folderElementsToMove)
            {
                FolderElement parentTraverser = parentElementAsFolder;
                while (parentTraverser != rootItem)
                {
                    if (folderElementToMove == parentTraverser)
                    {
                        EditorUtility.DisplayDialog("Favorites Error",
                            $"You're trying to move folder {folderElementToMove.FolderName} into subfolder {parentElementAsFolder.FolderName}! This isn't allowed!!",
                            "Oops sorry won't do that again.");
                        return;
                    }
                    else
                    {
                        parentTraverser = parentTraverser.GetParentFolder();
                    }
                }
            }

            // if insertAtIndex is -1
            // we put everything at the end.
            if (insertAtIndex == -1)
            {
                // Remove all elements from their original indices.
                RemoveElementsFromTheirParents(folderElementsToMove, assetElementsToMove);

                // Add folders to the bottom of the folders list.
                foreach (FolderElement folderElementToMove in folderElementsToMove)
                {
                    SetSubFolder(parentElementAsFolder.id, folderElementToMove.id);
                }
                // Add assets to the bottom of the assets list.
                foreach (AssetElement assetElementToMove in assetElementsToMove)
                {
                    SetSubAsset(parentElementAsFolder, assetElementToMove);
                }
            }

            else if (insertAtIndex <= parentElementAsFolder.GetSubFolderCount() - 1)
            {
                // if inbetween folders...
                //  - if it's a folder, we gotta do the usual.
                //  - if its an element, put it at index zero.

                // Adjust the insertAtIndex value by how many folder elements are BEFORE the desired insertAtIndex.
                int indexAdjustAmount = 0;
                foreach (FolderElement folderElementToMove in folderElementsToMove)
                {
                    int indexOfFolder = GetIndexOfSubFolder(folderElementToMove, parentElementAsFolder);
                    if (indexOfFolder != -1 && indexOfFolder < insertAtIndex)
                    {
                        indexAdjustAmount++;
                    }
                }
                insertAtIndex -= indexAdjustAmount;

                // Remove all elements from their original indices.
                RemoveElementsFromTheirParents(folderElementsToMove, assetElementsToMove);

                // Add folders to folders list while incrementing index
                foreach (FolderElement folderElementToMove in folderElementsToMove)
                {
                    SetSubFolder(parentElementAsFolder.id, folderElementToMove.id, insertAtIndex++);
                }

                // Add assets to the bottom of the assets list.
                int assetIndex = 0;
                foreach (AssetElement assetElementToMove in assetElementsToMove)
                {
                    SetSubAsset(parentElementAsFolder, assetElementToMove, assetIndex++);
                }
            }

            else
            {
                // if inbetween elements...
                // - if it's a folder, we put it at the end.
                // - if it's an element, we do the usual.

                // Adjust the insertAtIndex by how many folders exist.
                insertAtIndex -= parentElementAsFolder.GetSubFolderCount();

                // Adjust the insertAtIndex value by how many asset elements are BEFORE the desired insertAtIndex.
                int indexAdjustAmount = 0;
                foreach (AssetElement assetElementToMove in assetElementsToMove)
                {
                    int indexOfAsset = GetIndexOfSubAsset(assetElementToMove, parentElementAsFolder);
                    if (indexOfAsset != -1 && indexOfAsset < insertAtIndex)
                    {
                        indexAdjustAmount++;
                    }
                }
                insertAtIndex -= indexAdjustAmount;

                // Remove all elements from their original indices.
                RemoveElementsFromTheirParents(folderElementsToMove, assetElementsToMove);

                // Add folders to the bottom of the folders list.
                foreach (FolderElement folderElementToMove in folderElementsToMove)
                {
                    SetSubFolder(parentElementAsFolder.id, folderElementToMove.id);
                }
                foreach (AssetElement assetElementToMove in assetElementsToMove)
                {
                    SetSubAsset(parentElementAsFolder, assetElementToMove, insertAtIndex++);
                }
            }
            SaveFavData();

            SetExpanded(parentElementAsFolder.id, true);
            Reload();
        }
        private void DeleteFolderElement(FolderElement folderToDelete)
        {
            if (folderToDelete.GetSubFolderCount() == 0 && folderToDelete.GetSubAssetCount() == 0)
            {
                DeleteFolder(folderToDelete);
                SaveFavData();
                Reload();
            }
            else
            {
                int choice = EditorUtility.DisplayDialogComplex("Delete Folder",
                    $"The folder \"{folderToDelete.FolderName}\" has things inside it! Are you sure you'd like to delete this folder?",
                    "Yes", "No", "");

                if (choice == 0)
                {
                    DeleteFolder(folderToDelete);
                    SaveFavData();
                    Reload();
                }
            }
        }
        private void DeleteAssetElement(AssetElement assetToDelete)
        {
            DeleteAsset(assetToDelete);
            SaveFavData();
            Reload();
        }
        private void RemoveElementsFromTheirParents(List<FolderElement> foldersToRemove, List<AssetElement> assetsToRemove)
        {
            foreach (FolderElement folderToRemove in foldersToRemove)
            {
                UnsetSubFolder(folderToRemove);
            }
            foreach (AssetElement assetToRemove in assetsToRemove)
            {
                UnsetSubAsset(assetToRemove);
            }
        }
        #endregion

        #region FavData Management
        private FolderData CreateFolderData(string folderName)
        {
            return m_state.FavsData.GetNewFolderData(folderName);
        }
        private void SetSubFolder(int parentFolderId, int subFolderId, int insertIndex = -1)
        {
            m_state.FavsData.SetSubFolderData(parentFolderId, subFolderId, insertIndex);
        }
        private void DeleteFolder(FolderElement folderToRemove)
        {
            m_state.FavsData.DeleteFolderData(folderToRemove.id);
            UnsetSubFolder(folderToRemove);
        }
        private void UnsetSubFolder(FolderElement folderToRemove)
        {
            FolderElement parentFolder = folderToRemove.GetParentFolder();
            m_state.FavsData.UnsetSubFolderData(parentFolder.id, folderToRemove.id);
        }
        private int GetIndexOfSubFolder(FolderElement folderForIndex, FolderElement parentFolderElement = null)
        {
            int parentFolderIndex = 0;
            if (parentFolderElement != null)
            {
                parentFolderIndex = parentFolderElement.id;
            }
            return m_state.FavsData.GetIndexOfSubFolderData(parentFolderIndex, folderForIndex.id);
        }

        private AssetData CreateAssetData(string guid)
        {
            return m_state.FavsData.GetNewAssetData(guid);
        }
        private void SetSubAsset(FolderElement parentFolder, AssetElement asset, int insertIndex = -1)
        {
            m_state.FavsData.SetSubAssetData(parentFolder.id, asset.id, insertIndex);
        }
        private void DeleteAsset(AssetElement assetToRemove)
        {
            m_state.FavsData.DeleteAssetData(assetToRemove.id);
            UnsetSubAsset(assetToRemove);
        }
        private void UnsetSubAsset(AssetElement assetToRemove)
        {
            FolderElement parentFolder = assetToRemove.GetParentFolder();
            m_state.FavsData.UnsetSubAssetData(parentFolder.id, assetToRemove.id);
        }
        private int GetIndexOfSubAsset(AssetElement assetForIndex, FolderElement parentFolderElement = null)
        {
            int parentFolderIndex = 0;
            if (parentFolderElement != null)
            {
                parentFolderIndex = parentFolderElement.id;
            }
            return m_state.FavsData.GetIndexOfSubAssetData(parentFolderIndex, assetForIndex.id);
        }
        private void SaveFavData()
        {
            m_state.SaveData();
        }
        #endregion

        #region Interactions
        protected override void SingleClickedItem(int id)
        {
            // Id is invalid. Ignore it.
            if (id == -1)
            {
                return;
            }

            // Find the element with id...
            FavElement clickedElement = GetFavElementById(id);
            if (clickedElement is AssetElement clickedAsset) // If it's an AssetData...
            {
                // Ping the asset.
                if (clickedAsset.AssetIsValid)
                {
                    EditorGUIUtility.PingObject(clickedAsset.LoadedObject);
                }
            }
        }
        protected override void DoubleClickedItem(int id)
        {
            // Id is invalid. Ignore it.
            if (id == -1)
            {
                return;
            }

            if (!m_state.IsSearching)
            {
                FavElement clickedElement = GetFavElementById(id);
                if (clickedElement is AssetElement clickedAsset)
                {
                    // Ping the asset.
                    if (clickedAsset.AssetIsValid)
                    {
                        AssetDatabase.OpenAsset(clickedAsset.LoadedObject);
                    }
                }
            }
            else // Double click while searching.
            {
                // Clear the search, make the element visible in the treeview, select the element.
                m_state.SearchString = null;
                Reload();
                FavElement clickedElement = GetFavElementById(id);
                FolderElement recurseTillRoot = clickedElement.GetParentFolder();
                while (recurseTillRoot != rootItem)
                {
                    SetExpanded(recurseTillRoot.id, true);
                    recurseTillRoot = recurseTillRoot.GetParentFolder();
                }
                SetSelection(new List<int>() { id });
            }
        }
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return !m_state.IsSearching;
        }
        public void HandleSearchFieldChanged()
        {
            SetSelection(new List<int>());
            Reload();
        }
        private bool ElementIsSearchMatch(ISearchable searchable, List<string> searchArgs)
        {
            if (string.IsNullOrEmpty(searchable.SearchableString))
            {
                return false;
            }
            string sortString = searchable.SearchableString.ToLower();
            foreach (string arg in searchArgs)
            {
                if (string.IsNullOrEmpty(arg))
                {
                    continue;
                }
                if (!sortString.Contains(arg))
                {
                    return false;
                }
                int index = sortString.IndexOf(arg);
                sortString = sortString.Remove(index, arg.Length);
            }
            return true;
        }
        #endregion

        #region DragAndDrop
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return !m_state.IsSearching;
        }
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(DRAG_NAME, SortItemIDsInRowOrder(args.draggedItemIDs));
            DragAndDrop.StartDrag("Favs DragNDrop");
        }
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (m_state.IsSearching)
            {
                return DragAndDropVisualMode.Rejected;
            }

            if (args.performDrop)
            {
                List<int> draggedIds = DragAndDrop.GetGenericData(DRAG_NAME) as List<int>;
                if (draggedIds != null) //From within Favs Window
                {
                    List<FavElement> draggedElements = new List<FavElement>();
                    foreach (int draggedId in draggedIds)
                    {
                        draggedElements.Add(GetFavElementById(draggedId));
                    }
                    FavElement parentElement = args.parentItem != null ? args.parentItem as FavElement : rootItem as FavElement;
                    MoveElements(draggedElements, parentElement, args.insertAtIndex);
                }
                else if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length != 0) // Dragged from project window probably.
                {
                    List<string> pathsToAdd = new List<string>();
                    foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(obj);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            pathsToAdd.Add(assetPath);
                        }
                        else
                        {
                            Debug.Log("Invalid asset path for " + obj.name);
                            return DragAndDropVisualMode.Rejected;
                        }
                    }
                    FavElement parentElement = args.parentItem != null ? args.parentItem as FavElement : rootItem as FavElement;
                    AddElements(pathsToAdd, parentElement, args.insertAtIndex);
                }
                DragAndDrop.AcceptDrag();
            }
            return DragAndDropVisualMode.Move;
        }
        #endregion

        #region Rename
        protected override bool CanRename(TreeViewItem item)
        {
            return item is FolderElement && !m_state.IsSearching;
        }
        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (GetFavElementById(args.itemID) is FolderElement targetFolder)
            {
                if (args.acceptedRename)
                {
                    targetFolder.FolderName = args.newName;
                    SaveFavData();
                }
            }
        }
        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            float indent = GetContentIndent(item) + FavElement.IconWidth + 2;
            rowRect.x += indent;
            rowRect.width -= indent;
            return rowRect;
        }
        #endregion

        #region ContextMenus
        protected override void ContextClicked()
        {
            if (m_state.IsSearching)
            {
                return;
            }

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("New Folder"), false, () => { CreateSubFolderElement(0); });
            menu.ShowAsContext();
            Event.current.Use();
        }
        protected override void ContextClickedItem(int id)
        {
            if (m_state.IsSearching)
            {
                return;
            }

            Vector2 mousePosition = Event.current.mousePosition;

            FavElement clickedElement = GetFavElementById(id);
            if (clickedElement == rootItem) // No context clicking the root!
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            if (clickedElement is FolderElement folderClicked)
            {
                menu.AddItem(new GUIContent("Rename"),
                    false,
                    (element) => { BeginRename(folderClicked); },
                    folderClicked);
                menu.AddItem(new GUIContent("Change Icon"),
                    false,
                    (element) =>
                    {
                        Rect popupRect = new Rect(mousePosition, new Vector2(40f, 40f)); // Some made up rect I guess
                    PopupWindow.Show(popupRect, new FolderIconPicker((FolderElement)element, () => { m_state.SaveData(); Repaint(); }));
                    },
                    folderClicked);
                menu.AddItem(new GUIContent("New Folder"),
                    false,
                    (elementId) => { CreateSubFolderElement(id); },
                    id);
                menu.AddItem(new GUIContent("Delete Folder"),
                    false,
                    (element) => { DeleteFolderElement((FolderElement)element); },
                    folderClicked);
            }
            else if (clickedElement is AssetElement assetClicked)
            {
                menu.AddItem(new GUIContent("Delete Element"),
                    false,
                    (element) => { DeleteAssetElement((AssetElement)element); },
                    assetClicked);
            }
            menu.ShowAsContext();

            Event.current.Use();
        }
        #endregion
    }
}