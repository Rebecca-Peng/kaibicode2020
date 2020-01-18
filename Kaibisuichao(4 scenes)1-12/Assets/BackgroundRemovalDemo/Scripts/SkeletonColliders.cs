using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonColliders : MonoBehaviour
{

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Camera used to estimate the overlay positions of 3D-objects over the background. By default it is the main camera.")]
    public Camera foregroundCamera;

    [Tooltip("Depth image renderer.")]
    public SpriteRenderer depthImage;

    // width of the created box colliders
    [Range(0.01f, 0.3f)]//?
    public float colliderWidth = 0.2f;

    // the KinectManager instance
    private KinectManager manager;

    // screen rectangle taken by the foreground image (in pixels)
    private Rect foregroundImgRect;

    // game objects to contain the joint colliders
    private GameObject[,] jointColliders;
    private List<GameObject> jointColliderManager;
    private int numColliders;
    private List<int> numCollierManager;

    // depth image resolution
    private int depthImageWidth;
    private int depthImageHeight;

    // depth sensor platform
    private KinectInterop.DepthSensorPlatform sensorPlatform = KinectInterop.DepthSensorPlatform.None;

    // Use this for initialization
    void Start()
    {
	  Vector3 offsetPos = new Vector3(0.0f, 30.0f, 0.0f);

        if (foregroundCamera == null)
        {
            // by default use the main camera
            foregroundCamera = Camera.main;
        }

        manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            KinectInterop.SensorData sensorData = manager.GetSensorData();
            if (sensorData != null && sensorData.sensorInterface != null && foregroundCamera != null)
            {
                // sensor platform
                sensorPlatform = sensorData.sensorIntPlatform;

                // get depth image size
                depthImageWidth = sensorData.depthImageWidth;
                depthImageHeight = sensorData.depthImageHeight;

                // calculate the foreground rectangles
                Rect cameraRect = foregroundCamera.pixelRect;
                float rectHeight = cameraRect.height;
                float rectWidth = cameraRect.width;

                if (rectWidth > rectHeight)
                    rectWidth = rectHeight * depthImageWidth / depthImageHeight;
                else
                    rectHeight = rectWidth * depthImageHeight / depthImageWidth;

                float foregroundOfsX = (cameraRect.width - rectWidth) / 2;
                float foregroundOfsY = (cameraRect.height - rectHeight) / 2;
                foregroundImgRect = new Rect(foregroundOfsX, foregroundOfsY, rectWidth, rectHeight);

                // create joint colliders
                numColliders = sensorData.jointCount;
                int player_tot = 6;
                jointColliders = new GameObject[player_tot, numColliders];
                for (int playercnt_i = 0; playercnt_i < player_tot; playercnt_i++)
                {
                    for (int jointnum_j = 0; jointnum_j < numColliders; jointnum_j++)
                    {

                        string sColObjectName = playercnt_i.ToString() + "_" + ((KinectInterop.JointType)jointnum_j).ToString() + "Collider";
                        jointColliders[playercnt_i, jointnum_j] = new GameObject(sColObjectName);
                        jointColliders[playercnt_i, jointnum_j].transform.parent = transform;
				        jointColliders[playercnt_i, jointnum_j].transform.position = offsetPos;

                        if (jointnum_j == 0)
                        {
                            CircleCollider2D collider = jointColliders[playercnt_i, jointnum_j].AddComponent<CircleCollider2D>();
                            collider.radius = colliderWidth;
                        }
                        else
                        {
                            BoxCollider2D collider = jointColliders[playercnt_i, jointnum_j].AddComponent<BoxCollider2D>();
                            collider.size = new Vector2(colliderWidth, colliderWidth);
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        jointColliderManager = new List<GameObject>();
        numCollierManager = new List<int>();
        //numColliders = 0;
        //List<long> userId = new List<long>();
        // get the users texture
        if (manager && manager.IsInitialized() && depthImage &&
            (depthImage.sprite == null || sensorPlatform == KinectInterop.DepthSensorPlatform.KinectSDKv1))
        {
            Texture2D texDepth = manager.GetUsersLblTex2D();
            Rect rectDepth = new Rect(0, 0, texDepth.width, texDepth.height);
            Vector2 pivotSprite = new Vector2(0.5f, 0.5f);

            depthImage.sprite = Sprite.Create(texDepth, rectDepth, pivotSprite);
            depthImage.flipY = true;

            float worldScreenHeight = foregroundCamera.orthographicSize * 2f;
            float spriteHeight = depthImage.sprite.bounds.size.y;

            float scale = worldScreenHeight / spriteHeight;
            depthImage.transform.localScale = new Vector3(scale, scale, 1f);

        }
        int playerCount = manager.GetUsersCount();
        if (manager && playerCount > 0 && foregroundCamera)
        {
            KinectInterop.SensorData sensorData = manager.GetSensorData();
            numColliders = sensorData.jointCount;
            for (int player_i = 0; player_i < 6; player_i++)
            {
                if (player_i >= playerCount)
                {                                                                                                                                                                       
                    for (int collider_i = 0; collider_i < numColliders; collider_i++)
                    {
                        jointColliders[player_i, collider_i].SetActive(false);
                        jointColliders[player_i, collider_i].transform.position = Vector3.zero;
                    }
                    continue;
                }

                long userId = manager.GetUserIdByIndex(player_i);
                for (int collider_i = 0; collider_i < numColliders; collider_i++)
                {
                    bool bActive = false;
                    if (manager.IsJointTracked(userId, collider_i))
                    {
                        Vector3 posJoint = manager.GetJointPosDepthOverlay(userId, collider_i, foregroundCamera, foregroundImgRect);
                        posJoint.z = depthImage ? depthImage.transform.position.z : 0f;
                        if (collider_i == 0)
                        {
                            jointColliders[player_i, collider_i].transform.position = posJoint;

                            Quaternion rotCollider = manager.GetJointOrientation(userId, collider_i, true);
                            jointColliders[player_i, collider_i].transform.rotation = rotCollider;
                            bActive = true;
                        }
                        else
                        {
                            int p = (int)manager.GetParentJoint((KinectInterop.JointType)collider_i);

                            if (manager.IsJointTracked(userId, p))
                            {
                                // box colliders for bones
                                Vector3 posParent = manager.GetJointPosDepthOverlay(userId, p, foregroundCamera, foregroundImgRect);
                                posParent.z = depthImage ? depthImage.transform.position.z : 0f;

                                Vector3 posCollider = (posJoint + posParent) / 2f;
                                jointColliders[player_i, collider_i].transform.position = posCollider;

                                Quaternion rotCollider = Quaternion.FromToRotation(Vector3.up, (posJoint - posParent).normalized);
                                jointColliders[player_i, collider_i].transform.rotation = rotCollider;

                                BoxCollider2D collider = jointColliders[player_i, collider_i].GetComponent<BoxCollider2D>();
                                collider.size = new Vector2(collider.size.x, (posJoint - posParent).magnitude);

                                bActive = true;
                            }
                        }
                    }
                    if (jointColliders[player_i, collider_i].activeSelf != bActive)
                    {
                        // change collider activity
                        jointColliders[player_i, collider_i].SetActive(bActive);
                    }
                }
            }
        }
    }
}
