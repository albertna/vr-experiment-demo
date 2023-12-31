using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Newtonsoft.Json;

public class MenuManager : MonoBehaviour
{
    [Tooltip("A comma-separated list of sort keys. Valid keys are 'createdTime', 'folder', 'modifiedByMeTime', 'modifiedTime', 'name', 'name_natural', 'quotaBytesUsed', 'recency', 'sharedWithMeTime', 'starred', and 'viewedByMeTime'.")]
    // See more at https://developers.google.com/drive/api/reference/rest/v3/files/list
    [SerializeField] string orderSessionsBy;
    [SerializeField] string googleDriveFolderID;
    [SerializeField] string googleAPIKey;
    [SerializeField] Transform menuModal;
    [SerializeField] GameObject sessionSelectButtonPrefab;

    // Dictionary that maps session file names to download links
    Dictionary<string, string> fileLinks = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        Refresh();
    }

    // Fetch session files list and populate menu with session select buttons
    //
    // Note: No need to refresh after modifying session files since download links are static,
    // but do refresh after renaming/adding files
    public void Refresh()
    {
        StartCoroutine(RefreshCoroutine());
    }

    IEnumerator RefreshCoroutine()
    {
        // update fileLinks dictionary
        yield return StartCoroutine(GetFilesList());

        // clear menu
        foreach (Transform child in menuModal)
        {
            if (child.tag == "SessionSelectButton")
            {
                Destroy(child.gameObject);
            }
        }

        // populate menu with session select buttons
        foreach (var fileLink in fileLinks)
        {
            // instantiate button
            // Note: sessionSelectButtonPrefab should already have tag "SessionSelectButton"
            GameObject button = Instantiate(sessionSelectButtonPrefab, menuModal);

            // display session name as button text
            button.GetComponentInChildren<Text>().text = fileLink.Key;

            // add button click listener
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                // disable all session select buttons to prevent multiclicks
                EnableSessionSelectButtons(false);
                // start clicked session
                GetComponent<SessionManager>().StartSession(fileLink.Value);
            });
        }
    }


    IEnumerator GetFilesList()
    {
        // form url that retrieves file names & IDs in the Google Drive folder
        string fileListUrl = $"https://www.googleapis.com/drive/v3/files?q='{googleDriveFolderID}'+in+parents&fields=files(id,name)&orderBy={orderSessionsBy}&key={googleAPIKey}";

        // make GET request
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
            fileLinks.Clear();
            dynamic fileListObject = JsonConvert.DeserializeObject(www.downloadHandler.text);
            foreach (var file in fileListObject.files)
            {
                fileLinks.Add(file.name.ToString(), $"https://docs.google.com/spreadsheets/export?id={file.id}&exportFormat=csv");
            }
        }
    }

    public void EnableSessionSelectButtons(bool flag)
    {
        foreach (Transform child in menuModal)
        {
            if (child.tag == "SessionSelectButton")
            {
                child.GetComponent<Button>().interactable = flag;
            }
        }
    }
}
