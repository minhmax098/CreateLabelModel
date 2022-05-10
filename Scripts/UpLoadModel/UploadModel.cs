using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace UpLoadModel
{
    public class UploadModel : MonoBehaviour
    {
        public Image imgLoadingFill;
        public Text txtResult;
        public GameObject uiUploadModelInfo;
        public Button btnUploadModel3D;
        public Button btnBack;

        private void Start()
        {
            SetEventUI();
            //ToastText3D.Instance.Show3DTextToast("Text Message", 10);
        }

        public void SetEventUI()
        {
            btnUploadModel3D.onClick.AddListener(HandlerUploadModel);
            btnBack.onClick.AddListener(() => SceneManager.LoadScene("CreateLesson_Main"));
        }

        public void HandlerUploadModel()
        {
            imgLoadingFill.fillAmount = 0f;

            txtResult.gameObject.SetActive(false);

            AssetLoaderFilePicker.Create()
                .LoadModelFromFilePickerAsync("load model",
                    x =>
                    {
                        var path = $"{x.Filename}";
                        var cam = Camera.main;

                        if (cam != null)
                        {
                            x.RootGameObject.transform.SetParent(cam.transform);
                        }

                        var render = x.RootGameObject.GetComponentsInChildren<MeshRenderer>();

                        foreach (var y in x.MaterialRenderers.Values)
                        {
                            foreach (var mrc in y)
                            {
                                foreach (var r in render)
                                {
                                    if (r.name == mrc.Renderer.name)
                                    {
                                        r.materials = mrc.Renderer.materials;
                                        break;
                                    }
                                }
                            }
                        }

                        var sizeY = x.RootGameObject.GetComponentInChildren<MeshFilter>().mesh.bounds.size.y;

                        while (sizeY<1)
                        {
                            sizeY *= 10f;
                        }

                        sizeY *= 100f;
                        x.RootGameObject.transform.localScale = new Vector3(sizeY, sizeY, sizeY);
                        x.RootGameObject.transform.localPosition = Vector3.zero;
                        x.RootGameObject.transform.localRotation = Quaternion.Euler(Vector3.up * 180f);

                        x.RootGameObject.AddComponent<Rotate>();

                        if (x.RootGameObject.transform.parent != null)
                        {
                            x.RootGameObject.transform.SetParent(null);
                        }

                        StartCoroutine(HandleUploadModel3D(File.ReadAllBytes(path), path));
                        DontDestroyOnLoad(x.RootGameObject);
                    },
                    x => { },
                    (x, y) => { },
                    x => { },
                    x => { },
                    null,
                    ScriptableObject.CreateInstance<AssetLoaderOptions>());
        }

        public IEnumerator HandleUploadModel3D(byte[] fileData, string fileName)
        {
            var form = new WWWForm();

            form.AddBinaryData("model", fileData, fileName);

            const string API_KEY =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjp7InVzZXJfaWQiOjEsImVtYWlsIjoibnRodW9uZ2dpYW5nLml0QGdtYWlsLmNvbSIsInBhc3N3b3JkIjoiJDJhJDEwJEx2ZmNuQ0lmMDJwMkxXb2dGa29EWC4yYVFMbE16WXc5ZThqRDgud2svRFJobmFYLmtaTThLIiwiZnVsbF9uYW1lIjoiTmd1eeG7hW4gVGjhu4sgSMawxqFuZyBHaWFuZyBOZ3V54buFbiBUaOG7iyBIxrDGoW5nIEdpYW5nIE5ndXnhu4VuIFRo4buLIEjGsMahbmcgR2lhbmcgIiwiYXZhdGFyX2ZpbGVfaWQiOm51bGwsImlzX2FjdGl2ZSI6MSwiY3JlYXRlZF9ieSI6bnVsbCwiY3JlYXRlZF9kYXRlIjpudWxsLCJtb2RpZmllZF9ieSI6bnVsbCwibW9kaWZpZWRfZGF0ZSI6bnVsbH0sImlhdCI6MTY1MTY3ODI1NiwiZXhwIjoxNjUyMjgzMDU2fQ.BV_uKhU5o-eGhPNYFOiRzDHC0JfhpuqtePxtDK442B4";

            using var www = UnityWebRequest.Post(APIUrlConfig.Upload3DModel, form);

            www.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            var operation = www.SendWebRequest();

            uiUploadModelInfo.SetActive(true);

            while (!operation.isDone)
            {
                imgLoadingFill.fillAmount = operation.progress * 2f;
                Debug.Log("Percent progress upload:" + operation.progress);
                yield return null;
            }

            uiUploadModelInfo.SetActive(false);
            Debug.Log("Upload API Response: " + www.downloadHandler.text);

            if (www.downloadHandler.text == "Unauthorized" ||
                www.downloadHandler.text.StartsWith("<!DOCTYPE html>"))
            {
                txtResult.gameObject.SetActive(true);
                ToastText3D.Instance.Show3DTextToast( $"Upload Failed with response : {www.downloadHandler.text}!",Camera.main.ScreenToWorldPoint(txtResult.transform.position));
                txtResult.text = $"Upload Failed with response : {www.downloadHandler.text}!";
                yield break;
            }

            var postmodel = JsonConvert.DeserializeObject<PostModel>(www.downloadHandler.text);

            if (postmodel != null)
            {
                switch (postmodel.Message)
                {
                    case "Successfully!":
                        //ToastText3D.Instance.Show3DTextToast( "Upload Success",Camera.main.ScreenToWorldPoint(txtResult.transform.position));
                        //txtResult.text = "Upload Success!";

                        yield return new WaitForSeconds(0f);

                        SceneManager.LoadScene(SceneConfig.interactiveModel);
                        break;

                    default:
                        SSTools.ShowMessage("Upload Failed",SSTools.Position.bottom,SSTools.Time.twoSecond);
                        //ToastText3D.Instance.Show3DTextToast( "Upload Failed", Camera.main.ScreenToWorldPoint(txtResult.transform.position));
                        //txtResult.text = "Upload Failed!";
                        break;
                }
            }
        }
    }
}