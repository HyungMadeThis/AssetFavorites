using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetFavorites
{
    public interface ISearchable
    {
        string SearchableString { get; }
    }

    [Serializable]
    public class FavsData
    {
        [SerializeField]
        private int m_idCounter;
        [SerializeField]
        private List<FolderData> m_folderDatas = new List<FolderData>();
        [SerializeField]
        private List<AssetData> m_assetDatas = new List<AssetData>();

        public FavsData()
        {
            m_folderDatas.Add(new FolderData(m_idCounter++, "root"));
        }

        public List<FolderData> GetFolderDatas()
        {
            return m_folderDatas;
        }
        public FolderData GetNewFolderData(string folderName)
        {
            FolderData newFolder = new FolderData(m_idCounter++, folderName);
            int folderInsertAt = m_folderDatas.BinarySearch(newFolder, new FolderDataComparer());
            if (folderInsertAt < 0)
            {
                m_folderDatas.Insert(~folderInsertAt, newFolder);
            }
            else
            {
                m_folderDatas.Add(newFolder);
            }
            return newFolder;
        }
        private FolderData GetFolderDataById(int id)
        {
            return m_folderDatas.Find(x => x.Id == id);
        }
        public FolderData GetRootFolderData()
        {
            return GetFolderDataById(0);
        }
        public void SetSubFolderData(int parentFolderId, int subFolderId, int insertIndex = -1)
        {
            FolderData parentFolderData = GetFolderDataById(parentFolderId);
            if (parentFolderData != null)
            {
                parentFolderData.AddSubFolder(subFolderId, insertIndex);
            }
        }
        public void UnsetSubFolderData(int parentFolderId, int subFolderIdToRemove)
        {
            FolderData parentFolderData = GetFolderDataById(parentFolderId);
            if (parentFolderData != null)
            {
                parentFolderData.RemoveSubFolder(subFolderIdToRemove);
            }
        }
        public int GetIndexOfSubFolderData(int parentFolderId, int subFolderId)
        {
            FolderData parentFolderData = GetFolderDataById(parentFolderId);
            if (parentFolderData != null)
            {
                return parentFolderData.IndexOfSubFolder(subFolderId);
            }
            return -1;
        }
        public void DeleteFolderData(int folderIdToDelete)
        {
            FolderData folderToDelete = GetFolderDataById(folderIdToDelete);
            if (folderToDelete != null)
            {
                m_folderDatas.Remove(folderToDelete);
            }
        }

        public List<AssetData> GetAssetDatas()
        {
            return m_assetDatas;
        }
        public AssetData GetNewAssetData(string guid)
        {
            AssetData newAsset = new AssetData(m_idCounter++, guid);
            int assetInsertAt = m_assetDatas.BinarySearch(newAsset, new AssetDataComparer());
            if (assetInsertAt < 0)
            {
                m_assetDatas.Insert(~assetInsertAt, newAsset);
            }
            else
            {
                m_assetDatas.Add(newAsset);
            }
            return newAsset;
        }
        private AssetData GetAssetDataById(int id)
        {
            return m_assetDatas.Find(x => x.Id == id);
        }
        public void SetSubAssetData(int parentFolderId, int subAssetId, int insertIndex = -1)
        {
            FolderData parentFolderData = GetFolderDataById(parentFolderId);
            if (parentFolderData != null)
            {
                parentFolderData.AddSubAsset(subAssetId, insertIndex);
            }
        }
        public void UnsetSubAssetData(int parentFolderId, int subAssetIdToRemove)
        {
            FolderData parentFolderData = GetFolderDataById(parentFolderId);
            if (parentFolderData != null)
            {
                parentFolderData.RemoveSubAsset(subAssetIdToRemove);
            }
        }
        public void UnsetSubAssetData(int subAssetIdToRemove)
        {
            FolderData folderData = m_folderDatas.Find(x => x.ContainsSubAsset(subAssetIdToRemove));
            if (folderData != null)
            {
                folderData.RemoveSubAsset(subAssetIdToRemove);
            }
        }
        public int GetIndexOfSubAssetData(int parentFolderId, int subAssetId)
        {
            FolderData parentFolderData = GetFolderDataById(parentFolderId);
            if (parentFolderData != null)
            {
                return parentFolderData.IndexOfSubAsset(subAssetId);
            }
            return -1;
        }
        public void DeleteAssetData(int assetIdToDelete)
        {
            AssetData assetToDelete = GetAssetDataById(assetIdToDelete);
            if (assetToDelete != null)
            {
                m_assetDatas.Remove(assetToDelete);
            }
        }
    }
}