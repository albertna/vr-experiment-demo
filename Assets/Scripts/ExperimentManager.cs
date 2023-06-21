using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ExperimentManager : MonoBehaviour
{
    [SerializeField] string googleDriveFolderID;
    [SerializeField] string googleAPIKey;

    // Dictionary that maps session file names to download links
    Dictionary<string, string> fileLinks = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetFilesList());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GetFilesList()
    {
        // form url that retrieves file IDs & names in the Google Drive folder and make the GET request
        string fileListUrl = $"https://www.googleapis.com/drive/v3/files?q='{googleDriveFolderID}'+in+parents&fields=files(id,name)&key={googleAPIKey}";
        UnityWebRequest www = UnityWebRequest.Get(fileListUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // get JSON text from response
            string jsonString = www.downloadHandler.text;
            Debug.Log(jsonString);

            // deserialize JSON text and populate fileLinks dictionary
            dynamic fileListObject = JsonConvert.DeserializeObject(www.downloadHandler.text);
            foreach (var file in fileListObject.files)
            {
                fileLinks.Add(file.name.ToString(), $"https://docs.google.com/spreadsheets/export?id={file.id}&exportFormat=csv");
            }
        }
    }
}
