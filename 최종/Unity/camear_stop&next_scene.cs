using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // 씬 전환을 위한 네임스페이스 추가
using UnityEngine.Android;
using System.Collections;
using System.Text;

public class PhotoCapture : MonoBehaviour
{
    [Header("Component")]
    public RawImage cameraDisplay;
    public RectTransform cameraDisplayRect; // RawImage의 RectTransform

    private WebCamTexture webcamTexture;

    void Start()
    {
        // 카메라 권한 요청 및 카메라 시작
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            StartCoroutine(WaitForCameraPermission()); // 권한 요청 후 대기
        }
        else
        {
            StartCamera(); // 권한이 이미 있으면 바로 카메라 시작
        }
    }

    IEnumerator WaitForCameraPermission()
    {
        // 카메라 권한이 승인될 때까지 대기
        while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            yield return null;
        }
        
        StartCamera(); // 권한 승인 후 카메라 시작
    }

    void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected.");
            return;
        }

        int backCamIndex = -1;
        for (int i = 0; i < devices.Length; ++i)
        {
            if (!devices[i].isFrontFacing)
            {
                backCamIndex = i;
                break;
            }
        }

        if (backCamIndex != -1)
        {
            webcamTexture = new WebCamTexture(devices[backCamIndex].name);
            cameraDisplay.texture = webcamTexture;
            webcamTexture.Play();
            AdjustCameraDisplaySize(); // 카메라 출력 크기 조정
        }
        else
        {
            Debug.LogError("No back camera found.");
        }
    }

    void AdjustCameraDisplaySize()
    {
        if (webcamTexture == null || cameraDisplayRect == null) return;

        float screenAspect = (float)webcamTexture.width / webcamTexture.height;
        float displayAspect = cameraDisplayRect.rect.width / cameraDisplayRect.rect.height;

        if (screenAspect > displayAspect)
        {
            // 화면이 더 넓은 경우, 높이를 기준으로 맞추고 좌우를 잘라냄
            float newWidth = cameraDisplayRect.rect.height * screenAspect;
            cameraDisplayRect.sizeDelta = new Vector2(newWidth, cameraDisplayRect.rect.height);
        }
        else
        {
            // 화면이 더 높은 경우, 너비를 기준으로 맞추고 위아래를 잘라냄
            float newHeight = cameraDisplayRect.rect.width / screenAspect;
            cameraDisplayRect.sizeDelta = new Vector2(cameraDisplayRect.rect.width, newHeight);
        }
    }

    public void CaptureAndUploadPhoto()
    {
        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);
        photo.SetPixels(webcamTexture.GetPixels());
        photo.Apply();

        byte[] imageBytes = photo.EncodeToJPG();
        string base64Image = System.Convert.ToBase64String(imageBytes);
        StartCoroutine(UploadImage(base64Image));
    }

    private IEnumerator UploadImage(string base64Image)
    {
        string jsonBody = "{\"file\":\"" + base64Image + "\"}";

        UnityWebRequest request = new UnityWebRequest("https://kdhbyz2nva.execute-api.us-east-1.amazonaws.com/upload/v1", "POST");
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error uploading image: " + request.error);
        }
        else
        {
            Debug.Log("Image uploaded successfully!");
            string jsonResponse = request.downloadHandler.text;
            DisplayResults(jsonResponse);

            // 이미지 업로드 성공 시 카메라 중지
            StopCamera();
        }
    }

    private void DisplayResults(string jsonResponse)
    {
        Debug.Log("Server response: " + jsonResponse);
    }

    // 카메라 중지 함수 추가
    private void StopCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
            Debug.Log("Camera stopped.");
            
            // 1초 후에 다음 씬으로 이동
            StartCoroutine(LoadSceneWithDelay(1f));  // 1초 지연 후 씬 전환
        }
    }

    private IEnumerator LoadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 지연 시간 동안 기다림
        SceneManager.LoadScene("SampleScene2"); // "SampleScene2"을 원하는 씬 이름으로 변경
    }
}
