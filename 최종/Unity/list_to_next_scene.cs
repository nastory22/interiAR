using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class gotoAR : MonoBehaviour
{
    public Button myButton; // 버튼 객체
    private bool isListFetched = false; // 리스트가 성공적으로 가져왔는지 확인하는 변수
    private string firstItem; // 첫 번째 항목의 내용 저장

    void Start()
    {
        // 처음에는 버튼을 비활성화
        myButton.interactable = false;
    }

    // 리스트가 성공적으로 가져와졌을 때 호출되는 함수
    public void EnableButton(string item)
    {
        isListFetched = true; // 리스트가 성공적으로 가져왔다는 상태로 변경
        myButton.interactable = true; // 버튼을 활성화
        firstItem = item; // 첫 번째 항목 저장
    }

    // 버튼 클릭 시 호출되는 함수
    public void GotoARScene()
    {
        if (isListFetched) // 리스트가 성공적으로 가져와졌을 때만 씬 전환
        {
            Debug.Log("리스트를 가져왔으므로 AR 씬으로 전환합니다.");

            // 첫 번째 항목에 따라 씬을 다르게 로드
            if (firstItem == "미끄럼 방지 패드")
            {
                SceneManager.LoadScene("AR_scene_1");
            }
            else if (firstItem == "보조 의자")
            {
                SceneManager.LoadScene("AR_scene_2");
            }
            else
            {
                SceneManager.LoadScene("AR_scene"); // 기본 씬
            }
        }
    }
}
