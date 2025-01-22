using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class FetchRecommendations : MonoBehaviour
{
    public TMP_Text resultText; // 추천 리스트를 표시할 TextMeshPro UI 텍스트
    public gotoAR gotoARScript; // GotoAR 스크립트의 참조
    private string url = "http://44.211.210.30:5000/"; // 서버 URL (적절히 수정 필요)

    void Start()
    {
        // 시작 시 10초 후부터 주기적으로 서버에서 데이터를 불러옴
        InvokeRepeating("UpdateRecommendations", 1f, 3f);
    }

    // 주기적으로 서버에서 추천 리스트를 불러오는 함수
    void UpdateRecommendations()
    {
        StartCoroutine(GetRecommendations());
    }

    // 서버에서 추천 리스트를 가져오는 코루틴
    IEnumerator GetRecommendations()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("서버 연결 오류: " + request.error);
            resultText.text = "리스트 출력중";
        }
        else
        {
            // 서버에서 받은 HTML 데이터를 처리
            string htmlResponse = request.downloadHandler.text;
            Debug.Log("서버 응답: " + htmlResponse);  // 서버 응답 확인용

            // HTML에서 첫 번째 <li> 항목 추출
            string firstItem = ExtractFirstListItem(htmlResponse);

            // 추천 리스트에서 각 항목을 처리
            if (!string.IsNullOrEmpty(firstItem))
            {
                // 항목을 '<br>' 기준으로 나누어 여러 줄로 처리
                string[] lines = firstItem.Split(new string[] { "<br>" }, System.StringSplitOptions.None);

                // 첫 번째 줄만 볼드체 및 크기 90으로 변경
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("안전 손잡이") || lines[i].Contains("미끄럼 방지 패드") || lines[i].Contains("보조 의자"))
                    {
                        lines[i] = "<b><size=90>" + lines[i] + "</size></b>"; // 볼드체와 크기 90 적용
                    }
                }

                // 처리된 항목을 다시 결합하여 하나의 문자열로 만들기
                firstItem = string.Join("<br>", lines);
            }

            // 추천 리스트를 화면에 표시
            resultText.text = firstItem;

            // 첫 번째 항목이 있으면 버튼 활성화
            if (!string.IsNullOrEmpty(firstItem))
            {
                Debug.Log("추천 항목을 성공적으로 불러왔습니다.");
                gotoARScript.EnableButton(firstItem); // GotoAR 스크립트에서 버튼 활성화 함수 호출
            }
        }
    }

    // HTML에서 첫 번째 <li> 항목을 추출하는 함수
    private string ExtractFirstListItem(string html)
    {
        // <li>태그 내의 내용을 찾기 위한 정규식
        Match match = Regex.Match(html, "<li>(.*?)</li>");
        if (match.Success)
        {
            return match.Groups[1].Value; // 첫 번째 <li> 내용 반환
        }
        else
        {
            return "추천 항목 없음"; // <li> 항목이 없을 경우
        }
    }
}
