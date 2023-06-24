using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

using yutokun;

public class SessionManager : MonoBehaviour
{
    [SerializeField] Transform experimentObjectsParent;

    public void StartSession(string sessionFileUrl)
    {
        StartCoroutine(StartSessionCoroutine(sessionFileUrl));
    }

    IEnumerator StartSessionCoroutine(string sessionFileUrl)
    {
        // make GET request for session file
        UnityWebRequest www = UnityWebRequest.Get(sessionFileUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // get CSV text from response
            string sessionFileCSVString = www.downloadHandler.text;
            Debug.Log(sessionFileCSVString);

            StartCoroutine(StartSessionFromStringCoroutine(sessionFileCSVString));
        }
    }

    IEnumerator StartSessionFromStringCoroutine(string sessionFileCSVString)
    {
        // clear object spawning area
        foreach (Transform child in experimentObjectsParent)
        {
            Destroy(child.gameObject);
        }

        // parse CSV string and do steps
        var session = CSVParser.LoadFromString(sessionFileCSVString);
        foreach (var row in session)
        {
            // if not on header row, parse relevant fields
            if (row[0] != "#")
            {
                string objName = row[1];
                float distance = float.Parse(row[2]);
                float scale = float.Parse(row[3]);
                string color = row[4];
                float duration = float.Parse(row[5]);

                DoSessionStep(objName, distance, scale, color);

                // wait for duration
                yield return new WaitForSeconds(duration);
            }
        }

        // enable all session select buttons afterwards
        GetComponent<MenuManager>().EnableSessionSelectButtons(true);
    }

    // Spawn object with given parameters at random position with given distance from separation line
    void DoSessionStep(string objName, float distance, float scale, string color)
    {
        // instantiate object
        GameObject obj = Instantiate(Resources.Load<GameObject>($"Objects/{objName}"), experimentObjectsParent);

        // set position with random offset on x axis
        obj.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0, -distance);

        //set random rotation
        obj.transform.localRotation = Quaternion.Euler(Random.Range(-30, 30), Random.Range(-30, 30), Random.Range(0, 20));

        // set scale
        obj.transform.localScale = scale * obj.transform.localScale;

        // set color material
        obj.GetComponent<Renderer>().material = Resources.Load<Material>($"Materials/{color}");
    }
}
