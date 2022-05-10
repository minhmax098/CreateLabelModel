using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

using System.Reflection;
using System.Runtime.Versioning;


namespace BuildLesson
{
    public class BuildLessonManager : MonoBehaviour
    {
        
        void Start()
        {
            ObjectManager.Instance.InitOriginalExperience();
        }

        void Update()
        {
            TouchHandler.Instance.HandleTouchInteraction();
        }
    }
}
