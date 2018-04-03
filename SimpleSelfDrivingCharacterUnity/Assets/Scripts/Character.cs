using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Character : MonoBehaviour
{
    public GameObject Menu;
    public GameObject Mode;
    public GameObject MenuCamera;
    public GameObject MainCamera;
    public GameObject RawImage;
    public GameObject CollectingData;
    public GameObject Restart;
    public GameObject ZoneName;
    public GameObject ip_address;
    public GameObject Port;
    public GameObject Connect;
    private TCPConnection myTCP;                                                // Object responsible for TCP connection   
    private string action = "";                                                 // Command got by TCP connection
    private string zoneName = "";                                               // Name of current zone, also used for collecting data to specified folder
    private bool starting = true;                                               // Actual at the begining of program execution for 'Esc' button
    private bool train = false;                                                 // Identify the data collecting mode
    private bool test = false;                                                  // Identify the testing mode for algorithm by TCP connection
    private bool ConnectBtnPressed = false;

    public float RotateFrequency = 10;                                          // Rotations per second
    public float SampleFrequency = 1800;                                        // Generating samples per second
    public int Channels = 64;                                                   // Rays by vertical per degree
    public float MaximalVerticalFOV = +12.7f;                                   // Start degree by vertical
    public float MinimalVerticalFOV = -37.3f;                                   // End degree by vertical
    public float MeasurementRange = 120f;                                       // Max distance of ray
    public float MeasurementAccuracy = 0.02f;                                   // Generate some noise

    private const float viewAngle = 180;                                        // View range
    private const float shiftViewAngle = -90;
    private bool forwardPressed = false;                                         // Movement commands
    private bool backPressed = true;
    private bool leftPressed = false;
    private bool rightPressed = false;
    private short frameCounter = 0;                                             
    private const short framePeriod = 5;                                        // The time duration of the last command

    private bool TrainingData = false;

    private static string path = Directory.GetCurrentDirectory();

    public List<LaserSliceData> data = new List<LaserSliceData>();
    int CloudWidth;

    public Texture2D lastImage = null;
    bool imageRendered;

    public RawImage rawImage;

    void Start()
    {
        CloudWidth = Mathf.RoundToInt(SampleFrequency / RotateFrequency);
        lastImage = new Texture2D(CloudWidth, Channels, TextureFormat.RGB24, false);

        imageRendered = false;

        rawImage.texture = lastImage;
    }

    public void RestartOnClick()
    {
        if (zoneName == "Quadro Clockwise")
        {
            QuadroClock();
        }
        else if (zoneName == "Quadro Conter-Clockwise")
        {
            QuadroConterClock();
        }
        else if (zoneName == "Octus Clockwise")
        {
            OctusClock();
        }
        else if (zoneName == "Octus Conter-Clockwise")
        {
            OctusConterClock();
        }
        else if (zoneName == "Random")
        {
            RandomZone();
        }
        else if (zoneName == "Snake №1")
        {
            Snake_1();
        }
        else if (zoneName == "Snake №2")
        {
            Snake_2();
        }
    }

    public void TrainOnClick()
    {
        TrainingData = !TrainingData;

        if (TrainingData)
        {
            CollectingData.GetComponentInChildren<Text>().text = "Stop collecting!";
            Debug.Log("STARTED collecting training data!");
        }
        else
        {
            CollectingData.GetComponentInChildren<Text>().text = "Start collecting!";
            Debug.Log("STOPED collecting training data!");
        }
    }

    public void ConnectOnClick()
    {
        ConnectBtnPressed = true;
        myTCP = gameObject.AddComponent<TCPConnection>();
        myTCP.setupSocket(GameObject.Find("IPaddress").GetComponentInChildren<Text>().text,
                          System.Convert.ToInt32(GameObject.Find("Port").GetComponentInChildren<Text>().text));         // Make TCP Connection

        GameObject.Find("IPaddress").SetActive(false);
        GameObject.Find("Port").SetActive(false);
        GameObject.Find("Connect").SetActive(false);
    }

    public bool TryRenderPointCloud(out byte[] image)
    {
        if (imageRendered != false)
        {
            image = lastImage.EncodeToJPG();
            imageRendered = false;
            return true;
        }

        image = null;
        return false;
    }

    void TryComposeTexture()
    {
        while (data.Count >= CloudWidth * 2)
        {
            data.RemoveRange(0, CloudWidth);
        }

        if (data.Count >= CloudWidth)
        {
            for (int i = 0; i < CloudWidth; i++)
            {
                for (int j = 0; j < Channels; j++)
                {
                    float val = (data[i].Lasers[j].distance / MeasurementRange);

                    val = 2f / (1f + Mathf.Exp(-10f * val)) - 1f;

                    lastImage.SetPixel(i, j, new Color(0, val, 0));
                }
            }

            lastImage.Apply();


            if (TrainingData)
            {
                string imageName = "Lidar_" + (System.DateTime.Now.ToString() + '_' + System.DateTime.Now.Millisecond.ToString()).Replace('/', '_').Replace(':', '_') + ".jpg";
                string imagePath = path + '/' + zoneName + " Dataset/images/";
                File.WriteAllBytes((imagePath + imageName), lastImage.EncodeToJPG());
                File.AppendAllText(path + '/' + zoneName + " Dataset/" + "labels.csv", "\n" + imageName + ',' + (bool_to_int(forwardPressed) * 1 + bool_to_int(rightPressed) * 2 + bool_to_int(backPressed) * 3 + bool_to_int(leftPressed) * 4));
            }

            if (RawImage.active)
            {
                RawImage.GetComponentInChildren<RawImage>().texture = lastImage;
            }


            imageRendered = true;
        }
    }

    int bool_to_int(bool q)
    {
        if (q)
        {
            return 1;
        }
        return 0;
    }

    void SocketResponse()
    {
        action = myTCP.readSocket();
        Debug.Log(action);
    }

    void Update()
    {
        if (!starting && Input.GetKeyDown(KeyCode.Escape))
        {
            // Stop collecting the data
            TrainingData = false;

            // Cancel all movements
            forwardPressed = false;
            backPressed = true;
            leftPressed = false;
            rightPressed = false;

            // Activate\Deactivate other canvas elements
            RawImage.SetActive(!RawImage.active);

            if (train)
            {
                CollectingData.SetActive(!CollectingData.active);
                Restart.SetActive(!Restart.active);
            }
            ZoneName.SetActive(!ZoneName.active);

            MenuCamera.SetActive(!MenuCamera.active);
            MainCamera.SetActive(!MainCamera.active);

            if (test && !ConnectBtnPressed)
            {
                ip_address.SetActive(!ip_address.active);
                Port.SetActive(!Port.active);
                Connect.SetActive(!Connect.active);
            }

            // Deactivate\Activate Menu
            Menu.SetActive(!Menu.active);
        }

        if ((frameCounter == framePeriod) && ConnectBtnPressed)
        {
            action = "";
            while (action == "")
            {
                SocketResponse();
            }
        }

        if ((train && Input.GetKeyDown(KeyCode.UpArrow)) || action.StartsWith("forward"))
        {
            forwardPressed = true;
            backPressed = false;
            leftPressed = false;
            rightPressed = false;
        }
        else if ((train && Input.GetKeyDown(KeyCode.DownArrow)) || action.StartsWith("backward"))
        {
            forwardPressed = false;
            backPressed = true;
            leftPressed = false;
            rightPressed = false;
        }
        else if ((train && Input.GetKeyDown(KeyCode.RightArrow)) || action.StartsWith("rightward"))
        {
            forwardPressed = false;
            backPressed = false;
            leftPressed = false;
            rightPressed = true;
        }
        else if ((train && Input.GetKeyDown(KeyCode.LeftArrow)) || action.StartsWith("leftward"))
        {
            forwardPressed = false;
            backPressed = false;
            leftPressed = true;
            rightPressed = false;
        }

        if (forwardPressed)
        {
            gameObject.transform.Translate(Vector3.forward * 5 * Time.deltaTime, Space.Self);
        }
        else if (backPressed)
        {

        }
        else if (leftPressed)
        {
            gameObject.transform.Rotate(Vector2.down, Space.World);
        }
        else if (rightPressed)
        {
            gameObject.transform.Rotate(Vector2.up, Space.World);
        }


        if (data.Count >= 0)
        {
            for (float i = 0; i < CloudWidth; i++)
            {
                LaserSliceData temp;
                float ha = i * (viewAngle / (float)CloudWidth) + shiftViewAngle;
                RenderSlice(ha, out temp);
                data.Add(temp);
            }

        }
        else
        {
            LaserSliceData temp;
            RenderSlice(0, out temp);
            data.Add(temp);
        }
        TryComposeTexture();
        if (frameCounter == framePeriod)
        {
            frameCounter = 0;
            System.DateTime startTime = System.DateTime.Now;
            if (ConnectBtnPressed)
            {
                myTCP.writeSocket(lastImage.GetRawTextureData());
            }
        }

        frameCounter++;
    }

    void RenderSlice(float horizontalAngle, out LaserSliceData outSlice)
    {
        LaserData[] lasers = new LaserData[Channels];

        for (int i = 0; i < Channels; i++)
        {
            float verticalAngel = -Mathf.Lerp(MinimalVerticalFOV, MaximalVerticalFOV, (i / (float)(Channels - 1)));

            RaycastHit hit;

            float dist;

            Vector3 fwd = transform.TransformDirection(Quaternion.Euler(verticalAngel, horizontalAngle, 0) * Vector3.forward);
            if (Physics.Raycast(transform.position, fwd, out hit, MeasurementRange))
            {
                dist = hit.distance + Random.Range(-MeasurementAccuracy, MeasurementAccuracy);
                dist = Mathf.Clamp(dist, 0, MeasurementRange);

                Debug.DrawLine(transform.position, hit.point, Color.green);
                Debug.DrawLine(hit.point - Vector3.up * 0.3f, hit.point + Vector3.up * 0.3f, Color.red, 0, false);
                Debug.DrawLine(hit.point - Vector3.left * 0.3f, hit.point + Vector3.left * 0.3f, Color.red, 0, false);
                Debug.DrawLine(hit.point - Vector3.forward * 0.3f, hit.point + Vector3.forward * 0.3f, Color.red, 0, false);
            }
            else
            {
                dist = MeasurementRange;
            }

            lasers[i] = new LaserData()
            {
                distance = dist,
            };
        }

        LaserSliceData laserSliceData = new LaserSliceData()
        {
            RotationalPosition = horizontalAngle,
            Timestamp = Time.time,
            Lasers = lasers,
        };

        outSlice = laserSliceData;
    }

    public struct LaserData
    {
        public float distance;
        public float intensity;
    }

    public struct LaserSliceData
    {

        public float RotationalPosition;
        public LaserData[] Lasers;
        public float Timestamp;
    }

    public void QuadroClock()
    {
        zoneName = "Quadro Clockwise";

        gameObject.transform.position = new Vector3(129.7f, 7.5f, 368.3f);
        gameObject.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);

        Init();
    }

    public void QuadroConterClock()
    {
        zoneName = "Quadro Conter-Clockwise";

        gameObject.transform.position = new Vector3(129.7f, 7.5f, 368.3f);
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        Init();
    }

    public void OctusClock()
    {
        zoneName = "Octus Clockwise";

        gameObject.transform.position = new Vector3(189f, 7.5f, 149f);
        gameObject.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);

        Init();
    }

    public void OctusConterClock()
    {
        zoneName = "Octus Conter-Clockwise";

        gameObject.transform.position = new Vector3(189f, 7.5f, 149f);
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        Init();
    }

    public void RandomZone()
    {
        zoneName = "Random";

        gameObject.transform.position = new Vector3(304f, 7.5f, 70f);
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        Init();
    }

    public void Snake_1()
    {
        zoneName = "Snake №1";

        gameObject.transform.position = new Vector3(451f, 7.5f, 27f);
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        Init();
    }

    public void Snake_2()
    {
        zoneName = "Snake №2";

        gameObject.transform.position = new Vector3(556f, 7.5f, 27f);
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        Init();
    }

    private void Init()
    {
        starting = false;

        // Activate other canvas elements
        RawImage.SetActive(true);
        if (train)
        {
            CollectingData.SetActive(true);
            Restart.SetActive(true);
        }
        ZoneName.SetActive(true);
        ZoneName.GetComponentInChildren<Text>().text = zoneName;
        MainCamera.SetActive(true);

        if (test)
        {
            ip_address.SetActive(true);
            Port.SetActive(true);
            Connect.SetActive(true);
        }

        // Deactivate Menu
        Menu.SetActive(false);
        MenuCamera.SetActive(false);
    }

    public void Train()
    {
        train = true;
        Menu.SetActive(true);
        Mode.SetActive(false);
    }

    public void Test()
    {
        test = true;
        Menu.SetActive(true);
        Mode.SetActive(false);
    }
}