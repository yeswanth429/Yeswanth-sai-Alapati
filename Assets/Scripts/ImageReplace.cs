using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenAi;
using TMPro;
using UnityEditor;

public class ImageReplace : MonoBehaviour
{
    public OpenAiImageReplace openAiImageReplace;
    private bool isPrefab = false;


    public void UpdateImage()
    {
        EditorGUI.BeginDisabledGroup(openAiImageReplace.requestPending);
        if (!checkPromt())
        {
            string assetPath = AssetDatabase.GetAssetPath(openAiImageReplace.gameObject);
            isPrefab = assetPath != "";

            if (isPrefab)
            {
                EditorUtility.SetDirty(openAiImageReplace);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                OpenAiImageReplace prefabTarget = prefabRoot.GetComponent<OpenAiImageReplace>();
                prefabTarget.ReplaceImage(() =>
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath, out bool success);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                });
            }
            else
            {
                openAiImageReplace.ReplaceImage();
            }
        }
    }

    public bool checkPromt()
    {
        bool promptShown = false;

        if (Configuration.GlobalConfig.ApiKey == "")
        {
            Configuration.GlobalConfig = OpenAiApi.ReadConfigFromUserDirectory();
            promptShown = true;

        }

        return promptShown;
    }
}
