﻿using UnityEngine;

namespace Assets.Scripts.Classes.Helpers
{
    public class Utility
    {
        private RaycastHit _hit;
        private Ray _ray;
        public static Utility _instance;

        //  Instance 	
        public static Utility Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Utility();
                return _instance;
            }
        }

        public bool CheckIfClicked(Transform transform, int layerMask = -1)
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // if the layermask is not defined
            if (layerMask == -1)
            {
                if (Physics.Raycast(_ray, out _hit, 100) && _hit.transform == transform)
                {
                    return true;
                }
            }
            else
            {
                if (Physics.Raycast(_ray, out _hit, 100, layerMask) && _hit.transform == transform)
                {
                    return true;
                }
            }

            return false;
        }


        //By turnipski at Reddit
        public static Bounds GetChildRendererBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1, ni = renderers.Length; i < ni; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }
            return new Bounds();
        }

        public static void PlaceNewGameObject(Transform transform, Vector3 startPosition, float placementRadius)
        {
            var prefabBounds = GetChildRendererBounds(transform.gameObject);
            var clearPosition = false;
            var position = Vector3.one;
            //to avoid infinite loops
            var safetyCounter = 0;

            while (!clearPosition)
            {
                position =
                    new Vector3(Random.Range(startPosition.x - placementRadius, startPosition.x + placementRadius),
                        prefabBounds.extents.y,
                        Random.Range(startPosition.z - placementRadius, startPosition.z + placementRadius));

                var hitColliders = Physics.OverlapSphere(position,
                    transform.GetComponent<Renderer>().bounds.extents.magnitude);

                //Debug.DrawLine(position, position + (transform.localScale / 2), Color.cyan, 30.0f);

                if (hitColliders.Length <= 1) //You haven't hit someone with a collider here, excluding ours
                {
                    //Debug.Log("clear");
                    clearPosition = true;
                }

                //safety clause
                safetyCounter++;
                if (safetyCounter > 300)
                {
                    break;
                }
            }
            transform.localPosition = position;
        }

        public static GameObject GetChild(GameObject parent, string name)
        {
            var transforms = parent.GetComponentsInChildren(typeof(Transform), true);

            foreach (Transform transform in transforms)
            {
                if (transform.gameObject.name == name)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }
    }
}