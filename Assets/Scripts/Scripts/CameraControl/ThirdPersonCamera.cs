﻿using Assets.Scripts.Classes.Agent;
using UnityEngine;

namespace Assets.Scripts.Scripts.CameraControl
{
    public class ThirdPersonCamera : MonoBehaviour
    {

        private const float MIN_Y_ANGLE = 10.0f;
        private const float MAX_Y_ANGLE = 70.0f;
        private const float MIN_ZOOM = 5.0f;
        private const float MAX_ZOOM = 75.0f;

        private Camera _camera;
        public Transform BirdViewLookAt;
        public Transform CloseUpLookAt;
        private Transform _lookAtInUse;
        private bool _lookAtChanged = false;

        private bool _birdEyeViewActive = true;
        private const float BIRD_EYE_VIEW_DIST = 50.0f;

        private float currentDistance = 50.0f;
        private float zoomSensitivity = 50.0f;
        private float currentX = 0.0f;
        private float currentY = 40.0f;
        private float sensitivityX = 4.0f;
        private float sensitivityY = 1.0f;

        private RaycastHit _hit;
        private Ray _ray;

        private void Start()
        {
            _camera = Camera.main;

            //start looking at the center of the set
            BirdViewLookAt = GameObject.FindGameObjectWithTag("Scenario").transform;
            _lookAtInUse = BirdViewLookAt;

            //register listener from camera mode switch
            UI.ChangeCameraMode.Instance.OnSelect += ChangeCameraMode;
        }

        private void Update()
        {
            if (BirdViewLookAt == null)
            {
                BirdViewLookAt = GameObject.FindGameObjectWithTag("Scenario").transform;
            }

            //negate X in order to move in same direction as mouse
            if (Input.GetButton("Fire2"))
            {
                currentX -= Input.GetAxis("Mouse X");
                currentY += Input.GetAxis("Mouse Y");
                currentY = Mathf.Clamp(currentY, MIN_Y_ANGLE, MAX_Y_ANGLE);
            }

            //only when allowing changing lookat and zooming
            if (!_birdEyeViewActive)
            {
                currentDistance += Input.GetAxis("Mouse ScrollWheel")*zoomSensitivity;
                currentDistance = Mathf.Clamp(currentDistance, MIN_ZOOM, MAX_ZOOM);

                if (Input.GetButtonUp("Fire1"))
                {
                    _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    int layer = 8;
                    int layerMask = 1 << layer;

                    //if we selected a cube change the look at (layer only considers cubes)
                    if (Physics.Raycast(_ray, out _hit, 100, layerMask))
                    {
                        Debug.Log("New look at: " + _hit.transform.name);
                        CloseUpLookAt = _hit.transform;
                        _lookAtChanged = true;
                    }
                }
            }
        }

        private void LateUpdate()
        {

            if (_lookAtChanged)
            {
                if (_birdEyeViewActive)
                {
                    _lookAtInUse = BirdViewLookAt;
                    _lookAtChanged = false;
                }
                else
                {
                    _lookAtInUse = CloseUpLookAt ?? BirdViewLookAt;
                    _lookAtChanged = false;
                }

            }

            //if the lookat isn't static check if it's moving and don't move if it is
            if (!_birdEyeViewActive && _lookAtInUse.gameObject.GetComponent<Body>() != null)
            {
                if (_lookAtInUse.gameObject.GetComponent<Body>().Dragging)
                {
                    return;
                }
            }

            //only move camera when look at object is stationary
            if (_lookAtInUse != null)
            {
                float translationSpeed = 3.0f;  //This will determine translation speed
                float rotationSpeed = 50.0f;  //This will determine rotation speed
                float lookAtSpeed = 6.0f;  //This will determine lookAt speed

                Vector3 dir = new Vector3(0, 0, -currentDistance);
                Quaternion rotation = Quaternion.Slerp(_camera.transform.rotation, Quaternion.Euler(currentY, currentX, 0), rotationSpeed * Time.deltaTime);
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, _lookAtInUse.position + rotation * dir, translationSpeed * Time.deltaTime);

                Vector3 direction = _lookAtInUse.position - _camera.transform.position;
                _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, Quaternion.LookRotation(direction, Vector3.up), lookAtSpeed * Time.deltaTime);
            }
        }

        public void ChangeCameraMode()
        {
            _birdEyeViewActive = !_birdEyeViewActive;

            if (_birdEyeViewActive)
            {
                currentDistance = BIRD_EYE_VIEW_DIST;
            }

            _lookAtChanged = true;
        }

    }
}