using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DepthSpriteViewer : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Camera used to estimate the overlay positions of 3D-objects over the background. By default it is the main camera.")]
	public Camera foregroundCamera;
	
	[Tooltip("Depth image renderer.")]
	public SpriteRenderer depthImage;

	// width of the created box colliders
	private const float colliderWidth = 0.2f;

	// the KinectManager instance
	private KinectManager manager;

	// screen rectangle taken by the foreground image (in pixels)
	private Rect foregroundImgRect;

	// game objects to contain the joint colliders
	private GameObject[] jointColliders = null;
    private List<GameObject> jointColliderManager;
	private int numColliders = 0;
    private List<int> numCollierManager;

	// depth image resolution
	private int depthImageWidth;
	private int depthImageHeight;

	// depth sensor platform
	private KinectInterop.DepthSensorPlatform sensorPlatform = KinectInterop.DepthSensorPlatform.None;


	void Start () 
	{
		if (foregroundCamera == null) 
		{
			// by default use the main camera
			foregroundCamera = Camera.main;
		}

		manager = KinectManager.Instance;
		if(manager && manager.IsInitialized())
		{
			KinectInterop.SensorData sensorData = manager.GetSensorData();

			if(sensorData != null && sensorData.sensorInterface != null && foregroundCamera != null)
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
				
				if(rectWidth > rectHeight)
					rectWidth = rectHeight * depthImageWidth / depthImageHeight;
				else
					rectHeight = rectWidth * depthImageHeight / depthImageWidth;
				
				float foregroundOfsX = (cameraRect.width - rectWidth) / 2;
				float foregroundOfsY = (cameraRect.height - rectHeight) / 2;
				foregroundImgRect = new Rect(foregroundOfsX, foregroundOfsY, rectWidth, rectHeight);

				// create joint colliders
				numColliders = sensorData.jointCount;
                Debug.Log("-------------"+sensorData.jointCount);
				jointColliders = new GameObject[numColliders];
				
				for(int i = 0; i < numColliders; i++)
				{
					string sColObjectName = ((KinectInterop.JointType)i).ToString() + "Collider";
					jointColliders[i] = new GameObject(sColObjectName);
					jointColliders[i].transform.parent = transform;

					if (i == 0) 
					{
						// circle collider for body center
						CircleCollider2D collider = jointColliders[i].AddComponent<CircleCollider2D>();
						collider.radius = colliderWidth;
					} 
					else 
					{
						// box colliders for bones
						BoxCollider2D collider = jointColliders[i].AddComponent<BoxCollider2D>();
						collider.size = new Vector2(colliderWidth, colliderWidth);
					}
				}
			}
		}

	}
	
	void Update () 
	{
        jointColliderManager = new List<GameObject>();
        numCollierManager = new List<int>();
		// get the users texture
		if(manager && manager.IsInitialized() && depthImage && 
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
        if (playerCount == 0)
        {
            playerIndex = 0;
        }
        else
        {
            playerIndex = playerCount - 1;
        }

   //     Debug.Log("PlayerIndex: " + playerCount);
        if (manager && manager.IsUserDetected(playerIndex) && foregroundCamera)
		{
            KinectInterop.SensorData sensorData = manager.GetSensorData();
            numColliders = sensorData.jointCount;
            Debug.Log("Colliders Num： " + numColliders);
            List<long> userId = new List<long>();
            //long userId = manager.GetUserIdByIndex(playerIndex);  // manager.GetPrimaryUserID();
            for (int j = 0; j < playerCount; j++)
            {
                userId.Add(manager.GetUserIdByIndex(j));
                // update colliders
                for (int i = 0; i < numColliders; i++)
                {
                    bool bActive = false;

                    if (manager.IsJointTracked(userId[j], i))
                    {
                        Vector3 posJoint = manager.GetJointPosDepthOverlay(userId[j], i, foregroundCamera, foregroundImgRect);
                        posJoint.z = depthImage ? depthImage.transform.position.z : 0f;

                        if (i == 0)
                        {
                            // circle collider for body center
                            jointColliders[i].transform.position = posJoint;

                            Quaternion rotCollider = manager.GetJointOrientation(userId[j], i, true);
                            jointColliders[i].transform.rotation = rotCollider;

                            bActive = true;
                        }
                        else
                        {
                            int p = (int)manager.GetParentJoint((KinectInterop.JointType)i);

                            if (manager.IsJointTracked(userId[j], p))
                            {
                                // box colliders for bones
                                Vector3 posParent = manager.GetJointPosDepthOverlay(userId[j], p, foregroundCamera, foregroundImgRect);
                                posParent.z = depthImage ? depthImage.transform.position.z : 0f;

                                Vector3 posCollider = (posJoint + posParent) / 2f;
                                jointColliders[i].transform.position = posCollider;

                                Quaternion rotCollider = Quaternion.FromToRotation(Vector3.up, (posJoint - posParent).normalized);
                                jointColliders[i].transform.rotation = rotCollider;

                                BoxCollider2D collider = jointColliders[i].GetComponent<BoxCollider2D>();
                                collider.size = new Vector2(collider.size.x, (posJoint - posParent).magnitude);

                                bActive = true;
                            }
                        }
                    }

                    if (jointColliders[i].activeSelf != bActive)
                    {
                        // change collider activity
                        jointColliders[i].SetActive(bActive);
                    }
                }

            }
		}

	}

}
