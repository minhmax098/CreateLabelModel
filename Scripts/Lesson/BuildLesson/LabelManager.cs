using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BuildLesson 
{
    public class LabelManager : MonoBehaviour
    {
        private static LabelManager instance; 
        public static LabelManager Instance
        {
            get 
            {
                if (instance == null)
                {
                    centerPosition = CalculateCentroid(ObjectManager.Instance.OriginObject);
                    instance = FindObjectOfType<LabelManager>(); 
                }
                return instance; 
            }
        }
         private List<Vector3> pointPositions = new List<Vector3>();
 
        public void Update()
        {
            pointPositions.Add(transform.position);
        }

        private static Vector3 centerPosition;
        public void CreateLabel(GameObject currentGameObject, Vector3 tapPoint)
        {  
            Debug.Log("Centerposition x: " + centerPosition.x + ", Centerposition y: " + centerPosition.y + ", Centerposition z: " + centerPosition.z);
            GameObject labelObject = Instantiate(Resources.Load(PathConfig.MODEL_TAG_CALL) as GameObject);
            labelObject.transform.localScale *=  ObjectManager.Instance.OriginScale.x / ObjectManager.Instance.OriginObject.transform.localScale.x ;
            
            // TagHandler.Instance.AddTag(labelObject);
            labelObject.transform.SetParent(currentGameObject.transform, false); 
            labelObject.transform.localPosition = new Vector3(0, 0, 0);
            SetLabel(currentGameObject, tapPoint, ObjectManager.Instance.OriginObject, centerPosition, labelObject);
            // listLabelObjects.Add(labelObject);
        }

        public void SetLabel(GameObject currentObject, Vector3 tapPoint, GameObject parentObject, Vector3 rootPosition, GameObject label)
        {
            GameObject line = label.transform.GetChild(0).gameObject; 
            GameObject labelName = label.transform.GetChild(1).gameObject;
            // labelName.transform.GetChild(1).GetComponent<TextMeshPro>().text = currentObject.name;

            Bounds parentBounds = GetParentBound(parentObject, rootPosition);

            // GameObject s;
            // s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // s.transform.position = rootPosition;

            // Bounds objectBounds = currentObject.GetComponent<Renderer>().bounds;

            // New get bounds in local (in currentObject coordinate system), not world
            // Bounds objectBounds = currentObject.GetComponent<Renderer>().localBounds;

            // dir is the world vector3 present the direction between center of the game object and the center of currentObject
            // Vector3 dir = currentObject.transform.position - rootPosition; // Old 
            Vector3 dir = currentObject.transform.InverseTransformDirection(tapPoint - rootPosition) ; // New
            // Vector3 dir = tapPoint - rootPosition;
        
            Debug.Log("Magnitude: " + parentBounds.max.magnitude); // Magnitude not a constant
            labelName.transform.localPosition = 1 / parentObject.transform.localScale.x * parentBounds.max.magnitude * dir.normalized;
            line.GetComponent<LineRenderer>().useWorldSpace = false;
            line.GetComponent<LineRenderer>().widthMultiplier = 0.25f * parentObject.transform.localScale.x;  // 0.2 -> 0.05 then 0.02 -> 0.005
            
            line.GetComponent<LineRenderer>().SetVertexCount(2);
            line.GetComponent<LineRenderer>().SetPosition(0, currentObject.transform.InverseTransformPoint(tapPoint));
            line.GetComponent<LineRenderer>().SetPosition(1, labelName.transform.localPosition);
            line.GetComponent<LineRenderer>().SetColors(Color.black, Color.black);
            // Debug.Log("Label name x: " + labelName.transform.localPosition.x + ", Label name y: " + labelName.transform.localPosition.y + ", Label name z: " + labelName.transform.localPosition.z);
            // Debug.Log("Line x: " + line.transform.localPosition.x + ", Line y: " + line.transform.localPosition.y + ", Line z: " + labelName.transform.localPosition.z);
        }

        private static Vector3 CalculateCentroid(GameObject obj)
        {
            Transform[] children;
            Vector3 centroid = new Vector3(0, 0, 0);
            children = obj.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if(child != obj.transform)
                {
                    centroid += child.transform.position;
                }  
            }
            centroid /= (children.Length - 1);
            return centroid;
        }

        public Bounds GetParentBound(GameObject parentObject, Vector3 center)
        {
            foreach (Transform child in parentObject.transform)
            {
                center += child.gameObject.GetComponent<Renderer>().bounds.center;
            }

            center /= parentObject.transform.childCount;
            
            Bounds bounds = new Bounds(center, Vector3.zero);
            foreach(Transform child in parentObject.transform)
            {
                bounds.Encapsulate(child.gameObject.GetComponent<Renderer>().bounds);
            }
            return bounds;
        }
    }
}
