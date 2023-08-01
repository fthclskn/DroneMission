using UnityEngine;
using DroneController.Physics;
using System.Collections;
namespace DroneController
{
    namespace CameraMovement
    {
        public class CameraScript : MonoBehaviour
        {

			#region PUBLIC VARIABLES - EDITED THROUGH CUSTOM INSPECTOR EDITOR

			[HideInInspector] public int inputEditorFPS;

            [HideInInspector] public bool FPS;
            [HideInInspector] public LayerMask fpsViewMask;
            [HideInInspector] public Vector3 positionInsideDrone;
            [HideInInspector] public Vector3 rotationInsideDrone;
            [HideInInspector] public float fpsFieldOfView = 90;

            [HideInInspector] public GameObject[] dronesToControl; 
            [HideInInspector] public bool pickedMyDrone = false; 
            public GameObject ourDrone; 

            [Header("Position of the camera behind the drone.")]
            [HideInInspector] public Vector3 positionBehindDrone = new Vector3(0, 2, -4);

            
            [Range(0.0f, 0.1f)]
            [HideInInspector] public float cameraFollowPositionTime = 0.1f;

           
            [HideInInspector] public float extraTilt = 10;
            [HideInInspector] public LayerMask tpsLayerMask;
            [HideInInspector] public float tpsFieldOfView = 60;

            [HideInInspector] public bool freeMouseMovement = false;
			[HideInInspector] public bool useJoystickFreeMovementOnly = false;
			[HideInInspector] public string mouse_X_axisName = "Mouse X", mouse_Y_axisName = "Mouse Y";
			[HideInInspector] public string dPad_X_axisName = "dPad_X", dPad_Y_axisName = "dPad_Y";
            [HideInInspector] public float mouseSensitvity = 100;
            [HideInInspector] public float mouseFollowTime = 0.2f;

            #endregion

            #region PRIVATE VARIABLES

            private int counterToControl = 0; //counter which determins what drone we are following
            private Vector3 velocitiCameraFollow;

            private float cameraYVelocity;
            private float previousFramePos;

            private float currentXPos, currentYPos;
            private float xVelocity, yVelocity;

            private float mouseXwanted, mouseYwanted;

            private float zScrollAmountSensitivity = 1, yScrollAmountSensitivity = -0.5f;
            private float zScrollValue, yScrollValue;

            #endregion

            #region PUBLIC VARIABLES

          
            public GameObject[] canvasSelectButtons;
            public GameObject[] canvasExitButtons;
            public GameObject canvasTimeTrack;

            #endregion

            #region MONO BEHAVIOUR METHODS

            public virtual void Awake()
            {
                Drone_Pick_Initialization();
            }

			/*
            void FixedUpdate()
            {       
                FPVTPSCamera();
                ScrollMath();
            }

            void Update()
            {
                PickDroneToControl();
            }
            */

			#endregion

			#region PRIVATE METHODS

		
			void Drone_Pick_Initialization()
            {
				if (ourDrone)
				{
					pickedMyDrone = true;
					return;
				}

                //resseting sheit
                pickedMyDrone = false;
                ourDrone = null;
				
                dronesToControl = GameObject.FindGameObjectsWithTag("Player");

                //added pick the drone before fly
                if (dronesToControl.Length > 1)
                {
                    pickedMyDrone = false;
                    ourDrone = dronesToControl[counterToControl].gameObject;
                }
                else
                {
                    StartCoroutine(KeepTryingToFindOurDrone());
                }
            }
            
            //only when FPS is toggled ON
            void FPSCameraPositioning()
            {
                if (transform.parent == null)
                    transform.SetParent(ourDrone.GetComponent<DroneMovementScript>().animatedGameObject.transform);

                if (GetComponent<Camera>().cullingMask != fpsViewMask)
                    GetComponent<Camera>().cullingMask = fpsViewMask;

                if (GetComponent<Camera>().fieldOfView != fpsFieldOfView)
                    GetComponent<Camera>().fieldOfView = fpsFieldOfView;

                transform.localPosition = positionInsideDrone;
                transform.localEulerAngles = rotationInsideDrone;
            }

            //only when FPS is toggled OFF (Third person view)
            void TPSCameraPositioning()
            {
                if (transform.parent != null)
                    transform.SetParent(null);

                if (GetComponent<Camera>().cullingMask != tpsLayerMask)
                    GetComponent<Camera>().cullingMask = tpsLayerMask;

                if (GetComponent<Camera>().fieldOfView != tpsFieldOfView)
                    GetComponent<Camera>().fieldOfView = tpsFieldOfView;

                FollowDroneMethod();
                TiltCameraUpDown();
                FreeMouseMovementView();
            }

            void FollowDroneMethod()
            {
                if (pickedMyDrone && ourDrone)
                    transform.position = Vector3.SmoothDamp(transform.position, ourDrone.transform.TransformPoint(positionBehindDrone + new Vector3(0, yScrollValue, zScrollValue)), ref velocitiCameraFollow, cameraFollowPositionTime);
                else if (pickedMyDrone == false && dronesToControl.Length > 0)
                    transform.position = Vector3.SmoothDamp(transform.position, dronesToControl[counterToControl].transform.TransformPoint(positionBehindDrone + new Vector3(0, yScrollValue, zScrollValue)), ref velocitiCameraFollow, cameraFollowPositionTime);
            }
            
            void TiltCameraUpDown()
            {
                cameraYVelocity = Mathf.Lerp(cameraYVelocity, (transform.position.y - previousFramePos) * -extraTilt, Time.deltaTime * 10);
                previousFramePos = transform.position.y;
            }

            void FreeMouseMovementView()
            {
                if (freeMouseMovement == true)
                {
					float wantedXMouseFreeMovement = ((useJoystickFreeMovementOnly == false) ? Input.GetAxis(mouse_Y_axisName) : 0) * Time.deltaTime * mouseSensitvity;
					float wantedYMouseFreeMovement = ((useJoystickFreeMovementOnly == false) ? Input.GetAxis(mouse_X_axisName) : 0)* Time.deltaTime * mouseSensitvity;

					if(wantedXMouseFreeMovement == 0)
					{
						wantedXMouseFreeMovement = Input.GetAxis(dPad_Y_axisName) * Time.deltaTime * mouseSensitvity;
					}
					if(wantedYMouseFreeMovement == 0)
					{
						wantedYMouseFreeMovement = Input.GetAxis(dPad_X_axisName) * Time.deltaTime * mouseSensitvity;
					}

					mouseXwanted -= wantedXMouseFreeMovement;
                    mouseYwanted += wantedYMouseFreeMovement;

                    currentXPos = Mathf.SmoothDamp(currentXPos, mouseXwanted, ref xVelocity, mouseFollowTime);
                    currentYPos = Mathf.SmoothDamp(currentYPos, mouseYwanted, ref yVelocity, mouseFollowTime);

                    transform.rotation = Quaternion.Euler(new Vector3(14, ourDrone.GetComponent<DroneMovementScript>().currentYRotation, 0)) *
                        Quaternion.Euler(currentXPos, currentYPos, 0);
                }
                else
                {
					if (pickedMyDrone && ourDrone)
						transform.rotation = Quaternion.Euler(new Vector3(14 + cameraYVelocity, ourDrone.transform.rotation.eulerAngles.y, 0));
                    else if (pickedMyDrone == false && dronesToControl.Length > 0)
                        transform.rotation = Quaternion.Euler(new Vector3(14, dronesToControl[counterToControl].transform.rotation.eulerAngles.y, 0));
                }
            }

            #endregion

            #region PUBLIC METHODS

            public void FPVTPSCamera()
            {
                if (FPS == true && pickedMyDrone == true)
                {
                    FPSCameraPositioning();
                }
                else
                {
                    TPSCameraPositioning();
                }
            }

            public void PickDroneToControl()
            {
                if (dronesToControl.Length > 1)
                {
                    if (pickedMyDrone == false)
                    {

                        //freeMouseMovement = true;
                        if (canvasSelectButtons.Length != 0)
                        {
                            foreach (GameObject go in canvasSelectButtons)
                            {
                                go.SetActive(true);
                            }
                            foreach (GameObject go in canvasExitButtons)
                            {
                                go.SetActive(false);
                            }
                        }

                        if (Input.GetKeyDown(KeyCode.Return))
                        {
                            Select();
                        }
                        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                        {
                            PressedLeft();
                        }
                        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                        {
                            PressedRight();
                        }
                    }
                    else
                    {
                        //freeMouseMovement = false;
                        //mouseXwanted = 0;
                        //	mouseYwanted = 0;
                        if (canvasSelectButtons.Length != 0)
                        {
                            foreach (GameObject go in canvasSelectButtons)
                            {
                                if (go)
                                    go.SetActive(false);
                            }
                            foreach (GameObject go in canvasExitButtons)
                            {
                                if (go)
                                    go.SetActive(true);
                            }
                        }

                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            ReturnToPick();
                        }
                    }
                }
                else
                {
                    if (canvasSelectButtons.Length != 0)
                    {
                        foreach (GameObject go in canvasSelectButtons)
                        {
                            if (go)
                                go.SetActive(false);
                        }
                        foreach (GameObject go in canvasExitButtons)
                        {
                            if (go)
                                go.SetActive(false);
                        }
                    }
                }
            }

       
            public void ReturnToPick()
            {
                pickedMyDrone = false;

            }

            public void Select()
            {
                ourDrone = dronesToControl[counterToControl].gameObject;
                pickedMyDrone = true;
            }

            public void PressedLeft()
            {
                if (counterToControl >= 1)
                {
                    counterToControl--;
                }
                else
                {
                    counterToControl = dronesToControl.Length - 1;
                }
            }

          
            public void PressedRight()
            {
                if (counterToControl < dronesToControl.Length - 1)
                {
                    counterToControl++;
                }
                else
                {
                    counterToControl = 0;
                }
            }

            public void ScrollMath()
            {
                if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                {
                    zScrollValue += Input.GetAxis("Mouse ScrollWheel") * zScrollAmountSensitivity;
                    yScrollValue += Input.GetAxis("Mouse ScrollWheel") * yScrollAmountSensitivity;
                }
            }

            #endregion

            #region PRIVATE Coroutine METHODS

         
            IEnumerator KeepTryingToFindOurDrone()
            {
                while (ourDrone == null)
                {
                    yield return null;
                    try
                    {
                        ourDrone = GameObject.FindGameObjectWithTag("Player").gameObject;

                        if (ourDrone)
                            pickedMyDrone = true;
                    }
                    catch (System.Exception e)
                    {
                        print(" <color=red>I can't find it!</color> -> " + e);
                    }
                }
            }

            #endregion

        }
    }
}
