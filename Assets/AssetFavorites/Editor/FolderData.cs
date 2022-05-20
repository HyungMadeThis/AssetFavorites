using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetFavorites
{
    public enum FolderIcon
    {
        Red = 0,
        Orange = 1,
        Yellow = 2,
        Green = 3,
        Cyan = 4,
        Blue = 5,
        Purple = 6,
        Pink = 7,
        Grey = 8,
        Black = 9
    }
    
    [Serializable]
    public class FolderData : ISearchable
    {
        [SerializeField]
        private int m_id;
        [SerializeField]
        private string m_name;
        [SerializeField]
        private FolderIcon m_folderIcon = FolderIcon.Blue;
        [SerializeField]
        private List<int> m_subFolderIds = new List<int>();
        [SerializeField]
        private List<int> m_subAssetIds = new List<int>();

        public int Id { get { return m_id; } }
        public string Name { get { return m_name; } set { m_name = value; } }
        public FolderIcon FolderIcon { get { return m_folderIcon; } set { m_folderIcon = value; } }
        public string SearchableString { get { return Name; } }

        public FolderData(int id, string name)
        {
            m_id = id;
            m_name = name;
        }

        public List<int> GetSubFolderIds()
        {
            return new List<int>(m_subFolderIds);
        }
        public int GetSubFolderCount()
        {
            return m_subFolderIds.Count;
        }
        public void AddSubFolder(int id, int insertIndex = -1)
        {
            if (insertIndex == -1)
            {
                m_subFolderIds.Add(id);
            }
            else
            {
                m_subFolderIds.Insert(insertIndex, id);
            }
        }
        public void RemoveSubFolder(int id)
        {
            m_subFolderIds.Remove(id);
        }
        public int IndexOfSubFolder(int id)
        {
            return m_subFolderIds.IndexOf(id);
        }
        public bool ContainsSubFolder(int id)
        {
            return m_subFolderIds.Contains(id);
        }

        public List<int> GetSubAssetIds()
        {
            return new List<int>(m_subAssetIds);
        }
        public int GetSubAssetCount()
        {
            return m_subAssetIds.Count;
        }
        public void AddSubAsset(int id, int insertIndex = -1)
        {
            if (insertIndex == -1)
            {
                m_subAssetIds.Add(id);
            }
            else
            {
                m_subAssetIds.Insert(insertIndex, id);
            }
        }
        public void RemoveSubAsset(int id)
        {
            m_subAssetIds.Remove(id);
        }
        public int IndexOfSubAsset(int id)
        {
            return m_subAssetIds.IndexOf(id);
        }
        public bool ContainsSubAsset(int id)
        {
            return m_subAssetIds.Contains(id);
        }
    }

    public class FolderDataComparer : IComparer<FolderData>
    {
        public int Compare(FolderData x, FolderData y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }
}