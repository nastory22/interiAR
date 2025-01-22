from flask import Flask, render_template, jsonify
from recommendation_filter import parse_yolo_result, get_recommendation_list, filter_recommendation_list  # jang.py에서 함수 임포트

app = Flask(__name__)

@app.route('/')
def index():
    # YOLO 분석 결과 파일 경로
    yolo_result_file = 'runs/detect/exp/labels/uploaded_image.txt'  # YOLO 분석 결과 파일 경로로 수정

    # YOLO 분석 결과 파싱
    detected_classes = parse_yolo_result(yolo_result_file)

    # DynamoDB에서 추천 가구 리스트 가져오기
    recommendation_list = get_recommendation_list()

    # 추천 리스트 필터링
    filtered_recommendation_list = filter_recommendation_list(recommendation_list, detected_classes)

    # 필터링된 추천 리스트를 HTML로 렌더링하여 출력
    return render_template('index.html', recommendations=filtered_recommendation_list)

# Unity에서 접근할 수 있도록 JSON 형식으로 추천 리스트 반환
@app.route('/api/recommendations', methods=['GET'])
def api_recommendations():
    # YOLO 분석 결과 파일 경로
    yolo_result_file = 'runs/detect/exp/labels/uploaded_image.txt'  # YOLO 분석 결과 파일 경로로 수정

    # YOLO 분석 결과 파싱
    detected_classes = parse_yolo_result(yolo_result_file)

    # DynamoDB에서 추천 가구 리스트 가져오기
    recommendation_list = get_recommendation_list()

    # 추천 리스트 필터링
    filtered_recommendation_list = filter_recommendation_list(recommendation_list, detected_classes)

    # JSON 형식으로 응답 반환
    return jsonify(recommendations=filtered_recommendation_list)

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
-------------------------------------------------------------------
python3 app.py

# 퍼블릭 IP로 웹사이트 접속 : http://3.83.230.13:5000/
