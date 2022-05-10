using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using System; 
namespace BuildLesson
{
    public class TouchHandler : MonoBehaviour
    {
        private static TouchHandler instance; 
        public static TouchHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TouchHandler>();
                }
                return instance; 
            }
        }
        public static event Action onResetStatusFeature; 
        public static event Action<GameObject> onSelectChilObject; 
        const float ROTATION_RATE = 0.08f;
        const float LONG_TOUCH_THRESHOLD = 1f; 
        const float ROTATION_SPEED = 0.5f; 
        float touchDuration = 0.0f; 
        Touch touch; 
        Touch touchZero; 
        Touch touchOne; 
        float originDelta; 
        Vector3 originScale;
        Vector3 originLabelScale = new Vector3(1f, 1f, 1f);
        Vector3 originLabelTagScale = new Vector3(7f, 1f, 1f);
        Vector3 originLineScale = new Vector3(1f, 1f, 1f); 

        Vector3 originScaleSelected;

        bool isMovingByLongTouch = false; 
        bool isLongTouch = false;

        GameObject currentSelectedObject; 
        private Vector3 mOffset; 
        private float mZCoord; 

        private Vector3 hitPoint;
        public void HandleTouchInteraction()    
        {
            if (ObjectManager.Instance.CurrentObject == null)
            {
                return;
            }
            if (Input.touchCount == 1)
            {
                touch = Input.GetTouch(0); 
                if (touch.tapCount == 1)
                {
                    HandleSingleTouch(touch);
                }
                else if (touch.tapCount == 2)
                {
                    touch = Input.touches[0];
                    if (touch.phase == TouchPhase.Ended)
                    {
                        HandleDoupleTouch(touch);
                    }
                }
            }
        }
        private void HandleSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began: 
                {
                    Debug.Log("Begin Touch ....");
                    var rs = GetChildOrganOnTouchByTag(touch.position);
                    
                    currentSelectedObject = rs.Item1;
                    // mZCoord = rs.Item2; 
                    hitPoint = rs.Item2;

                    isMovingByLongTouch = currentSelectedObject != null; 
                    if (currentSelectedObject != null)
                    {
                        Debug.Log("Current Selected Object: " + currentSelectedObject.name);
                        
                        // mZCoord = Camera.main.WorldToScreenPoint(currentSelectedObject.transform.position).z; 
                        // Vector3 point = GetTouchPositionAsWorldPoint(touch);
                        GameObject s;
                        s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        var spereRenderer = s.GetComponent<Renderer>();
                        
                        spereRenderer.material.SetColor("_Color", Color.red);
                        s.transform.parent = currentSelectedObject.transform;
                        s.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                        s.transform.position = hitPoint; // Global
                        
                        // Debug.Log("");
                        Debug.Log("Hit point 3D in world pos: " + hitPoint.x + ", " + hitPoint.y + ", " + hitPoint.z);
                        // Create label 
                        LabelManager.Instance.CreateLabel(currentSelectedObject, hitPoint);
                    }
                    break; 
                }
                case TouchPhase.Stationary: 
                {
                    if (isMovingByLongTouch && !isLongTouch)
                    {
                        touchDuration += Time.deltaTime;
                        if (touchDuration > LONG_TOUCH_THRESHOLD)
                        {
                            OnLongTouchInvoke();
                            Debug.Log("OnLongTouchInvoke...");
                        }
                    }
                    break;
                }
                case TouchPhase.Moved:
                {
                    if (isLongTouch)
                    {
                        // Drag(touch, currentSelectedObject);
                    }
                    else
                    {
                        Rotate(touch);
                    }
                    break;
                }
                case TouchPhase.Ended: 
                {
                    // ResetLongTouch(); 
                    break;
                }
                case TouchPhase.Canceled: 
                {
                    // ResetLongTouch(); 
                    break;
                }
            }
        }
        private Vector3 GetTouchPositionAsWorldPoint(Touch touch)
        {
            Vector3 touchPoint = touch.position;
            touchPoint.z = mZCoord;
            return Camera.main.ScreenToWorldPoint(touchPoint);
        }

        private void Rotate(Touch touch)
        {
            if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_AR)
            {
                ObjectManager.Instance.OriginObject.transform.rotation *= Quaternion.Euler(new Vector3(0, -touch.deltaPosition.x * ROTATION_SPEED, 0));
            }
            else if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_3D)
            {
                ObjectManager.Instance.OriginObject.transform.Rotate(touch.deltaPosition.y * ROTATION_RATE, -touch.deltaPosition.x * ROTATION_RATE, 0, Space.World);
            }
        }

        void OnLongTouchInvoke()
        {
            StartCoroutine(HightLightObject());
            isLongTouch = true;
        }

        IEnumerator HightLightObject()
        {
            originScaleSelected = currentSelectedObject.transform.localScale;
            currentSelectedObject.transform.localScale = originScaleSelected * 1.5f;
            yield return new WaitForSeconds(0.12f);
            currentSelectedObject.transform.localScale = originScaleSelected;
        }

        private (GameObject, Vector3) GetChildOrganOnTouchByTag(Vector3 position)
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit hit; 

            // var ray : Ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            // var hit : RaycastHit;
            // if (Physics.Raycast (ray, hit)) 
            // {
            //     Debug.Log(hit.point);
            // }

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.transform.root.gameObject.tag == TagConfig.ORGAN_TAG)
                {
                    if (hit.collider.transform.parent == ObjectManager.Instance.CurrentObject.transform)
                    {
                        // return (hit.collider.gameObject, hit.point.z);
                        // return (hit.collider.gameObject, hit.transform);
                        return (hit.collider.gameObject, hit.point);
                    }
                }
            }
            return (null, new Vector3(0f, 0f, 0f));
        }

        private void HandleDoupleTouch(Touch touch)
        {

        }
    }
}

