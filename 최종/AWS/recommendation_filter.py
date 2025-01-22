# YOLO 분석 결과 파일 경로
yolo_result_file = 'runs/detect/exp/labels/uploaded_image.txt'

# YOLO 분석 결과 파싱
def parse_yolo_result(file_path):
    detected_classes = []
    with open(file_path, 'r') as file:
        for line in file:
            # 클래스 번호만 추출 (파일 내 첫 번째 값)
            class_id = int(line.split()[0])
            detected_classes.append(class_id)
    return detected_classes

# DynamoDB에서 추천 가구 리스트 가져오기 (예시 데이터)
def get_recommendation_list():
    # 예시 데이터 - 실제로 DynamoDB에서 가져올 수 있도록 수정해야 합니다
    return ['person', 'cat', 'dog', 'keyboard', 'mouse', 'bus', 'tie']

# 감지된 객체를 제외한 추천 가구 리스트를 필터링
def filter_recommendation_list(recommendation_list, detected_classes):
    class_to_item_map = {
        0: 'person',    # person
        1: 'cat',       # cat
        2: 'dog',       # dog
        73: 'keyboard', # keyboard
        63: 'mouse',    # mouse
        5: 'bus',       # bus (예시 ID, 실제 클래스 ID로 수정 필요)
        27: 'tie',       # tie (예시 ID, 실제 클래스 ID로 수정 필요)
    }
    
    # 중복된 클래스 제거
    unique_detected_classes = set(detected_classes)
    
    # 필터링된 추천 가구 리스트
    filtered = []
    for item in recommendation_list:
        # item이 class_to_item_map에서 감지된 클래스에 해당하지 않으면 추가
        if item not in [class_to_item_map[cls] for cls in unique_detected_classes if cls in class_to_item_map]:
            filtered.append(item)
    
    return filtered

# 전체 실행
def main():
    # YOLO 분석 결과 파일 경로
    yolo_result_file = 'runs/detect/exp/labels/uploaded_image.txt'
    
    # YOLO 분석 결과 파싱
    detected_classes = parse_yolo_result(yolo_result_file)
    
    # DynamoDB에서 추천 가구 리스트 가져오기
    recommendation_list = get_recommendation_list()
    
    # 감지된 객체를 제외한 추천 가구 리스트 필터링
    filtered_recommendations = filter_recommendation_list(recommendation_list, detected_classes)
    
    # 순서대로 추천 가구 리스트 출력
    print("Filtered Recommendation List:")
    for item in filtered_recommendations:
        print(item)

# 프로그램 실행
if __name__ == "__main__":
    main()
