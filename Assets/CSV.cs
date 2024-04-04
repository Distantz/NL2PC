using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using System.Threading;
using System.Globalization;

public class CSV : MonoBehaviour
{

    public Object displayer;

    public Button nxtTrackPieceButton;
    public Button prevTrackPieceButton;
    public Button loadCSVButton;
    public Button toggleExactButton;
    public Button pcButton;
    public TMP_Text trackCntrl;
    public TMP_Text posXText;
    public TMP_Text posYText;
    public TMP_Text posZText;
    public TMP_InputField nodeNumInput;
    public TMP_Text totalNodeCount;
    public Image timerImage;

    public LineRenderer line;

    public List<GameObject> displayObj;

    public Material unsel;
    public Material sel;

    public float timerDuration;
    public bool doTimer;

    public AudioSource audioPlayer;
    public AudioClip startTimer;
    public AudioClip stopTimer;
    public AudioClip timerProgress;

    private List<Vector3[]> data;

    private float nextYawAddition;

    [SerializeField]
    public float currentTimerVal;

    private int trackPieceCount;
    private int lastSelPiece;

    private bool showExact;

    // Start is called before the first frame update
    void Start()
    {

        CultureInfo.CurrentCulture = new CultureInfo("en-us");

        trackPieceCount = 0;

        nxtTrackPieceButton.onClick.AddListener(doNextTrackPiece);
        prevTrackPieceButton.onClick.AddListener(doPrevTrackPiece);
        loadCSVButton.onClick.AddListener(startCSV);
        toggleExactButton.onClick.AddListener(toggleExact);
        pcButton.onClick.AddListener(timerPress);


    }

    public void onNodeNumberEnter()
    {
        int number = int.Parse(nodeNumInput.text);

        trackPieceCount = number;
        doTrackPiece(number);

        displayObj[lastSelPiece].GetComponent<MeshRenderer>().material = unsel;
        displayObj[trackPieceCount].GetComponent<MeshRenderer>().material = sel;

        lastSelPiece = number;

        Debug.Log(number);
    }

    void playAudio(AudioClip clip)
    {
        audioPlayer.clip = clip;
        audioPlayer.Play();
    }

    void toggleExact()
    {

        showExact = !showExact;

        doTrackPiece(trackPieceCount);
        doPosInfo(data[trackPieceCount][0], trackPieceCount);

        if (showExact)
        {
            toggleExactButton.GetComponentInChildren<TMP_Text>().text = "show integers";
        }
        else
        {
            toggleExactButton.GetComponentInChildren<TMP_Text>().text = "show decimals";
        }

    }

    void timerPress()
    {
        doTimer = !doTimer;

        if (doTimer)
        {
            playAudio(startTimer);
        }
        else
        {
            playAudio(stopTimer);
        }
    }

    void startCSV()
    {

        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
            trackPieceCount = 0;
        }

        string path = OpenCSV();

        if (path != null)
        {
            data = LoadCSV(path);
        }

        totalNodeCount.text = string.Format("/ {0}", (data.Count - 1).ToString());
    }


    void doTrackPiece(int count)
    {

        float yaw = data[count][1].y -data[count - 1][1].y + nextYawAddition;

        if (yaw < -180)
        {
            yaw += 360;
        }

        else if (yaw > 180)
        {
            yaw -= 360;
        }

        if (yaw > 90)
        {
            nextYawAddition = yaw - 90;
            yaw = 90;
        }
        else if (yaw < -90)
        {
            nextYawAddition = (yaw * -1 - 90) * -1;
            yaw = -90;
        }
        else
        {
            nextYawAddition = 0;
        }


        float pitch = data[count][1].x;

        if (pitch < -180)
        {
            pitch += 360;
        }
        else if (pitch > 180)
        {
            pitch -= 360;
        }


        float roll = data[count][2].z * -1;

        if (roll < -180)
        {
            roll += 360;
        }
        else if (roll > 180)
        {
            roll -= 360;
        }

        float lastRoll = data[count - 1][2].z * -1;

        if (lastRoll < -180)
        {
            lastRoll += 360;
        }
        else if (lastRoll > 180)
        {
            lastRoll -= 360;
        }

        print(Mathf.Sign(roll) + ", " + Mathf.Sign(lastRoll));

        if (Mathf.Sign(roll) != Mathf.Sign(lastRoll) && (roll > 90 || roll < -90))
        {

            float rollDifference = Mathf.Sign(roll) - Mathf.Sign(lastRoll);

            if (rollDifference < -180)
            {
                rollDifference += 360;
            }
            else if (rollDifference > 180)
            {
                rollDifference -= 360;
            }

            float largeAngle = 360 - Mathf.Abs(rollDifference);


            if (Mathf.Sign(roll) == 1)
            {
                roll -= largeAngle;
            }
            else
            {
                roll += largeAngle;
            }
        }

        if (showExact)
        {
            trackCntrl.text = string.Format("yaw: {0}°\npitch: {1}°\nroll: {2}°", yaw.ToString("n2"), pitch.ToString("n2"), roll.ToString("n2"));
        }
        else
        {
            trackCntrl.text = string.Format("yaw: {0}°\npitch: {1}°\nroll: {2}°", Mathf.Round(yaw), Mathf.Round(pitch), Mathf.Round(roll));
        }



        try
        {
            line.SetPosition(0, data[count - 1][0]);
            line.SetPosition(1, data[count][0]);
            line.enabled = true;
        }
        catch
        {
            line.enabled = false;
        }


    }

    string OpenCSV()
    {
        ExtensionFilter[] extension = new[] {
            new ExtensionFilter("CSV file", "csv" ),
        };


        string[] path = StandaloneFileBrowser.OpenFilePanel("Open CSV", "", extension, false);

        if (path.Length != 0)
        {
            return path[0];
        }
        else
        {
            return null;
        }
    }

    List<Vector3[]> LoadCSV(string path)
    {

        List<Vector3[]> csvFile = new List<Vector3[]>();

        string csv = System.IO.File.ReadAllText(path);

        string[] csvLines = csv.Split("\n"[0]);

        displayObj = new List<GameObject>();

        int count = 0;
        foreach(string line in csvLines)
        {
            string[] lineData = line.Split("\t"[0]);

            if (count != 0)
            {
                Vector3 position = new Vector3(float.Parse(lineData[1]) * -1, float.Parse(lineData[2]), float.Parse(lineData[3]));
                Vector3 forward = new Vector3(float.Parse(lineData[4]) * -1, float.Parse(lineData[5]), float.Parse(lineData[6]));
                Vector3 left = new Vector3(float.Parse(lineData[7]), float.Parse(lineData[8]), float.Parse(lineData[9]));
                Vector3 up = new Vector3(float.Parse(lineData[10]), float.Parse(lineData[11]), float.Parse(lineData[12]));

                GameObject instant = (GameObject)Instantiate(displayer, transform);
               
                instant.transform.position = position;
                instant.transform.up = up;
                instant.transform.forward = forward * -1;

                float roll = Mathf.Atan2(left.y, up.y) * (180 / Mathf.PI) * -1;

                Debug.DrawRay(instant.transform.position, instant.transform.forward * 3, Color.blue, 100);
                Debug.DrawRay(instant.transform.position, instant.transform.up * 3, Color.red, 100);
                Debug.DrawRay(instant.transform.position, left * 3, Color.green, 100);

                Vector3[] vectors = new Vector3[3];

                vectors[0] = position;
                vectors[1] = instant.transform.localEulerAngles;
                vectors[2] = new Vector3(0, 0, roll);

                csvFile.Add(vectors);

                instant.transform.localEulerAngles = new Vector3(instant.transform.localEulerAngles.x, instant.transform.localEulerAngles.y, instant.transform.localEulerAngles.z - roll);

                displayObj.Add(instant);
            }

            count += 1;

        }

        return csvFile;
    }

    void doNextTrackPiece()
    {

        trackPieceCount += 1;

        currentTimerVal = 0;

        try
        {
            if (trackPieceCount == 0)
            {
                trackCntrl.text = "yaw: 0°\npitch: 0°\nroll: 0°";
            }
            else if (trackPieceCount > data.Count - 1)
            {
                trackCntrl.text = "yaw: done!\npitch: done!\nroll: done!";
                trackPieceCount = -1;
            }
            else
            {
                doTrackPiece(trackPieceCount);
            }



            doPosInfo(data[trackPieceCount][0], trackPieceCount);

            displayObj[lastSelPiece].GetComponent<MeshRenderer>().material = unsel;
            displayObj[trackPieceCount].GetComponent<MeshRenderer>().material = sel;

            lastSelPiece = trackPieceCount;
        }

        catch
        {
            try
            {
                displayObj[0].GetComponent<MeshRenderer>().material = unsel;
            }
            catch
            {
            }
        }

        doPosInfo(data[trackPieceCount][0], trackPieceCount);

    }
    
    void doPosInfo(Vector3 position, int count)

    {

        if (!showExact)
        {
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);
            position.z = Mathf.Round(position.z);

            posXText.text = "pos x: \n" + position.x + "m";
            posYText.text = "pos Y: \n" + position.y + "m";
            posZText.text = "pos Z: \n" + position.z + "m";

        }
        else
        {
            posXText.text = "pos x: \n" + position.x.ToString("n2") + "m";
            posYText.text = "pos y: \n" + position.y.ToString("n2") + "m";
            posZText.text = "pos z: \n" + position.z.ToString("n2") + "m";
        }


        nodeNumInput.text = count.ToString();
    }

    void doPrevTrackPiece()
    {

        trackPieceCount += 1;

        currentTimerVal = 0;

        try
        {
            trackPieceCount -= 2;

            if (trackPieceCount < 0)
            {
                trackCntrl.text = "yaw: done!\npitch: done!\nRoll: done!";
                trackPieceCount = -1;
            }
            else
            {
                doTrackPiece(trackPieceCount);
            }

            doPosInfo(data[trackPieceCount][0], trackPieceCount);

            displayObj[lastSelPiece].GetComponent<MeshRenderer>().material = unsel;
            displayObj[trackPieceCount].GetComponent<MeshRenderer>().material = sel;

            lastSelPiece = trackPieceCount;

        }
        catch
        {
            try
            {
                displayObj[0].GetComponent<MeshRenderer>().material = unsel;
            }
            catch
            {
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (doTimer)
        {
            if (currentTimerVal < 1)
            {
                currentTimerVal += Time.deltaTime / timerDuration;
                timerImage.fillAmount = (currentTimerVal / 2) + 0.5f;
            }
            else
            {

                if (trackPieceCount >= data.Count - 1)
                {
                    doTimer = false;
                }
                else
                {
                    doNextTrackPiece();
                }

                timerImage.fillAmount = 0.5f;

                playAudio(timerProgress);
            }
        }
        else
        {

            timerImage.fillAmount = 0;
            currentTimerVal = 0;

        }


        if (Input.GetKeyDown(KeyCode.Comma))
        {
            doPrevTrackPiece();
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            doNextTrackPiece();
        }

    }

}
