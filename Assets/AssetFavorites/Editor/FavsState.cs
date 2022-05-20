using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetFavorites
{
    [Serializable]
    public class FavsState
    {
        [SerializeField]
        private TreeViewState m_treeState = new TreeViewState();
        [SerializeField]
        private string m_searchString;
        [SerializeField]
        private List<string> m_parsedSearchString = new List<string>();
        [NonSerialized]
        private FavsData m_favsData;

        public TreeViewState TreeState => m_treeState;
        public string SearchString
        {
            get
            {
                return m_searchString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_searchString = "";
                    m_parsedSearchString.Clear();
                }
                else
                {
                    m_searchString = value;
                    m_parsedSearchString = new List<string>(m_searchString.ToLower().Split(" "));
                }
            }
        }
        public bool IsSearching { get { return !string.IsNullOrEmpty(SearchString); } }
        public List<string> ParsedSearchString => m_parsedSearchString;
        public FavsData FavsData
        {
            get
            {
                if (m_favsData == null)
                {
                    ReloadData();
                }
                return m_favsData;
            }
        }
        private Action OnStateChangedExternally;

        public void ReloadData()
        {
            m_favsData = FavsDataProvider.LoadData();
        }
        public void ResetData()
        {
            FavsDataProvider.SaveData(new FavsData(), this);
        }
        public void SaveData()
        {
            FavsDataProvider.SaveData(FavsData, this);
        }

        public void SubscribeToStateChangedExternally(Action callback)
        {
            FavsDataProvider.AddActiveState(this);
            OnStateChangedExternally -= callback;
            OnStateChangedExternally += callback;
        }

        public void UnsubscribeToStateChangedExternally(Action callback)
        {
            FavsDataProvider.RemoveActiveState(this);
            OnStateChangedExternally -= callback;
        }

        public void HandleStateChanged()
        {
            OnStateChangedExternally?.Invoke();
        }
    }
}