﻿using System;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using ColossalFramework.IO;

namespace CSLMusicMod
{
    /// <summary>
    /// Class that initializes the custom music tracks.
    /// Those are separate from the radio channels.
    /// </summary>
    public class ContentInitializer : MonoBehaviour
    {
        private bool _isInitialized;
        private Dictionary<string, RadioContentInfo> _customPrefabs;

        public ContentInitializer()
        {
        }

        /// <summary>
        /// Initializes the custom songs
        /// </summary>
        protected void InitializeImpl()
        {
            UserRadioCollection collection = LoadingExtension.UserRadioContainer;

            var collectionnames = collection.m_Songs.Values.Select(song => song.m_Collection).Distinct().ToArray();
            CSLMusicMod.Log("Available collections: " + String.Join("\n", collectionnames));

            foreach(UserRadioContent content in collection.m_Songs.Values)
            {
                try
                {                    
                
                    // Bases all music on vanilla "Aukio" song. 
	                CreatePrefab(content.m_Name, "aukio", new Action<RadioContentInfo>((RadioContentInfo obj) =>
	                    {
	                        obj.m_fileName = content.m_FileName;
	                        obj.m_displayName = content.m_DisplayName;
	                        obj.m_contentType = content.m_ContentType;

                            // Add the channels this song is playing in into the song.
	                        List<RadioChannelInfo> channels = new List<RadioChannelInfo>();

	                        foreach(UserRadioChannel uchannel in content.m_Channels)
	                        {
	                            var channel = FindChannelPrefab(uchannel.m_Name);
	                            channels.Add(channel);
	                        }

	                        obj.m_radioChannels = channels.ToArray();
	                    }));
                }
                catch(Exception e)
                {
                    Debug.LogError("[CSLMusic] Error while initializing prefab in " + content.m_Name + "! Exception: " + e.ToString());
                }
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
            _customPrefabs = new Dictionary<string, RadioContentInfo>();
        }

        public void OnLevelWasLoaded(int level)
        {
            if (level == 6)
            {
                _customPrefabs.Clear();
                _isInitialized = false;
            }
        }

        public void Update()
        {
            if (!_isInitialized)
            {
                RadioContentCollection collection = Resources.FindObjectsOfTypeAll<RadioContentCollection>().FirstOrDefault();

                if (collection != null && collection.isActiveAndEnabled)
                {
                    Loading.QueueLoadingAction(() =>
                        {
                            InitializeImpl();
                            PrefabCollection<RadioContentInfo>.InitializePrefabs("CSLMusicContent ", _customPrefabs.Values.ToArray(), null);
                        });
                    _isInitialized = true;
                }
            }
        }

        protected void CreatePrefab(string newPrefabName, string originalPrefabName, Action<RadioContentInfo> setupAction)
        {
            var originalPrefab = FindOriginalPrefab(originalPrefabName);

            if (originalPrefab == null)
            {
                Debug.LogErrorFormat("AbstractInitializer#CreatePrefab - Prefab '{0}' not found (required for '{1}')", originalPrefabName, newPrefabName);
                return;
            }
            if (_customPrefabs.ContainsKey(newPrefabName))
            {
                return;
            }
            var newPrefab = ClonePrefab(originalPrefab, newPrefabName, transform);
            if (newPrefab == null)
            {
                Debug.LogErrorFormat("AbstractInitializer#CreatePrefab - Couldn't make prefab '{0}'", newPrefabName);
                return;
            }
            setupAction.Invoke(newPrefab);
            _customPrefabs.Add(newPrefabName, newPrefab);
        }

        protected static RadioContentInfo ClonePrefab(RadioContentInfo originalPrefab, string newName, Transform parentTransform)
        {
            var instance = UnityEngine.Object.Instantiate(originalPrefab.gameObject);
            instance.name = newName;

            var newPrefab = instance.GetComponent<RadioContentInfo>();   


            instance.SetActive(false);
            newPrefab.m_prefabInitialized = false;
            return newPrefab;
        }

        protected static RadioContentInfo FindOriginalPrefab(string originalPrefabName)
        {
            RadioContentInfo foundPrefab;           
            foundPrefab = Resources.FindObjectsOfTypeAll<RadioContentInfo>().FirstOrDefault(netInfo => netInfo.name == originalPrefabName);
            if (foundPrefab == null)
            {
                return null;
            }
            return foundPrefab;
        }

        protected static RadioChannelInfo FindChannelPrefab(string originalPrefabName)
        {
            RadioChannelInfo foundPrefab;           
            foundPrefab = Resources.FindObjectsOfTypeAll<RadioChannelInfo>().FirstOrDefault(netInfo => netInfo.name == originalPrefabName);
            if (foundPrefab == null)
            {
                return null;
            }
            return foundPrefab;
        }
    }
}

