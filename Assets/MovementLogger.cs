using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovementLogger : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    
    public float logHz = 60.0f;   // log 60x per second
    private float nextLogTime = 0.0f;   // the next timeframe data should be logged
                                        
    private string sceneName;
    private string filePath;
    private string folderName = "OBE_Movement_Logs";

    /// <summary>
    /// adds the x,y,z of the position and rotation of each tracked body part to dataRows
    /// </summary>
    /// <param name="dataRows">The list with all positional and rotational arguements added to.</param>
    private void AddPositionRotation(List<string> dataRows) {
        List<Transform> bodyParts = new List<Transform> {head, leftHand, rightHand};
        foreach(Transform bodyPart in bodyParts) {
            dataRows.Add(bodyPart.position.x.ToString());
            dataRows.Add(bodyPart.position.y.ToString());
            dataRows.Add(bodyPart.position.z.ToString());

            dataRows.Add(bodyPart.rotation.eulerAngles.x.ToString());
            dataRows.Add(bodyPart.rotation.eulerAngles.y.ToString());
            dataRows.Add(bodyPart.rotation.eulerAngles.z.ToString());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // folder creation
        string folderPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }

        // add timestamp to file name
        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);
        string timestamp = easternTime.ToString("yyyyMMdd_HHmmssfff");

        sceneName = SceneManager.GetActiveScene().name;

        string fileName = $"{sceneName}Movement_Log_{timestamp}.csv";
        filePath = Path.Combine(folderPath, fileName);
        Debug.Log($"File path set to: {filePath}"); 

        // add CSV headers
        List<string> bodyParts = new List<string> {"Head", "LHand", "RHand"};
        List<string> transforms = new List<string> {"X", "Y", "Z", "Yaw", "Pitch", "Roll"};

        List<string> headers = new List<string>();
        headers.Add("Frame");
        headers.Add("Time");
        foreach(string bodyPart in bodyParts) {
            foreach(string transform in transforms) {
                headers.Add(bodyPart + transform);
            }
        }

        string headersFlat = string.Join(",", headers) + "\n";

        File.WriteAllText(filePath, headersFlat);
    }

    // Update is called once per fixed timeframe
    void FixedUpdate()
    {
        if (head != null && leftHand != null && rightHand != null) {
            if (Time.time >= nextLogTime) {
                // populate csv in order of head, lhand, rhand, for pos: x,y,z then rot: x,y,z
                List<string> dataRows = new List<string> {Time.frameCount.ToString(), Time.time.ToString("F2")};
                AddPositionRotation(dataRows);
                string flatDataRows = string.Join(",", dataRows) + "\n";

                File.AppendAllText(filePath, flatDataRows);

                nextLogTime += (1.0f/logHz);   // update for consistent intervals between logs
            }
            
        } else {
            Debug.LogError("Error: Missing reference in XROrigin");
        }
    }
}
