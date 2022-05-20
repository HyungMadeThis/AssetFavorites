using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Text;

namespace AssetFavorites
{
    public class FavsDataProvider : AssetModificationProcessor
    {
        private static string DATA_SAVE_KEY = "AssetFavoritesData";
        private static List<FavsState> m_activeStates = new List<FavsState>();
        private static FavsData m_latestData;
        private static EditorCoroutine m_waitFrameCoroutine;

        public static void AddActiveState(FavsState favsState)
        {
            if (!m_activeStates.Contains(favsState))
            {
                m_activeStates.Add(favsState);
            }
        }

        public static void RemoveActiveState(FavsState favsState)
        {
            m_activeStates.Remove(favsState);
            if (m_activeStates.Count == 0)
            {
                m_latestData = null;
            }
        }

        public static FavsData LoadData()
        {
            if (!EditorPrefs.HasKey(DATA_SAVE_KEY))
            {
                Debug.LogWarning("No FavsData found! Created new one.");
                EditorPrefs.SetString(DATA_SAVE_KEY, JsonUtility.ToJson(new FavsData()));
            }

            if (m_latestData == null)
            {
                m_latestData = JsonUtility.FromJson<FavsData>(EditorPrefs.GetString(DATA_SAVE_KEY));
                ValidateFavsData();
            }

            if (m_latestData == null || m_latestData.GetRootFolderData() == null)
            {
                Debug.LogError("FavsData was corrupt! Resetting state.");
                m_latestData = new FavsData();
                EditorPrefs.SetString(DATA_SAVE_KEY, JsonUtility.ToJson(m_latestData));
            }
            return m_latestData;
        }

        public static void SaveData(FavsData favsData, FavsState invoker = null)
        {
            //Debug.Log($"Saving data: \n{JsonUtility.ToJson(favsData, true)}");
            EditorPrefs.SetString(DATA_SAVE_KEY, JsonUtility.ToJson(favsData));
            m_latestData = favsData;
            CallStateChanged(invoker);
        }

        private static bool ValidateFavsData()
        {
            int numAssetToUpdate = 0;
            StringBuilder strBuilder = new StringBuilder();
            foreach (AssetData assetData in m_latestData.GetAssetDatas())
            {
                string currPath = AssetDatabase.GUIDToAssetPath(assetData.Guid);
                if (!string.IsNullOrEmpty(currPath) && currPath != assetData.Path)
                {
                    strBuilder.Append($"==========\nOLD: {assetData.Path}\nNEW: {currPath}\n==========");
                    assetData.Path = currPath;
                    numAssetToUpdate++;
                }
            }
            if (numAssetToUpdate > 0)
            {
                strBuilder.Insert(0, $"Favs: Found assets to update! (Count: {numAssetToUpdate})\n");
                Debug.LogWarning(strBuilder.ToString());
            }
            return numAssetToUpdate > 0;
        }

        private static void CallStateChanged(FavsState invoker)
        {
            foreach (FavsState favsState in m_activeStates)
            {
                if (favsState != invoker)
                {
                    favsState.HandleStateChanged();
                }
            }
        }

        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            if (m_activeStates.Count == 0 || m_latestData == null)
            {
                return AssetDeleteResult.DidNotDelete;
            }

            string targetGuid = AssetDatabase.AssetPathToGUID(path);
            List<AssetData> assetDatas = m_latestData.GetAssetDatas();
            for (int i = assetDatas.Count - 1; i >= 0; i--)
            {
                AssetData assetData = assetDatas[i];
                if (assetData.Guid == targetGuid)
                {
                    m_latestData.UnsetSubAssetData(assetData.Id);
                    m_latestData.DeleteAssetData(assetData.Id);
                }
            }

            SaveData(m_latestData);

            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (m_activeStates.Count == 0 || m_latestData == null)
            {
                return AssetMoveResult.DidNotMove;
            }

            // We need to validate and update the FavData AFTER the file move has been made.
            if (m_waitFrameCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(m_waitFrameCoroutine);
            }
            m_waitFrameCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(WaitFrameBeforeUpdateData());
            return AssetMoveResult.DidNotMove;
        }

        /// <summary>
        /// We only have access to OnWillMoveAsset and nothing for AFTER the asset is moved.
        /// So we want to wait a frame before actually updating all the favs windows.
        /// Otherwise the moved asset will incorrectly display as MISSING in the favs window.
        /// </summary>
        private static IEnumerator WaitFrameBeforeUpdateData()
        {
            yield return new WaitForFixedUpdate();
            ValidateFavsData();
            SaveData(m_latestData);
            EditorCoroutineUtility.StopCoroutine(m_waitFrameCoroutine); // Do I need to stop myself?
        }
    }
}