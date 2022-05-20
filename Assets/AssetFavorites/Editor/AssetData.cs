using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetFavorites
{
    [Serializable]
    public class AssetData : ISearchable
    {
        [SerializeField]
        private int m_id;
        [SerializeField]
        private string m_guid;
        [SerializeField]
        private string m_path;

        public int Id { get { return m_id; } }
        public string Guid { get { return m_guid; } }
        public string Path { get { return m_path; } set { m_path = value; } }
        public string SearchableString { get { return System.IO.Path.GetFileNameWithoutExtension(Path); } }

        public AssetData(int id, string guid)
        {
            m_id = id;
            m_guid = guid;
            m_path = AssetDatabase.GUIDToAssetPath(guid);
        }
    }

    public class AssetDataComparer : IComparer<AssetData>
    {
        public int Compare(AssetData x, AssetData y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }
}