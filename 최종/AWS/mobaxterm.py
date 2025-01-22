#!/bin/bash

# 시작 메시지
echo "Starting run_all.sh"

# 이미지 다운로드 (S3 -> EC2)
IMAGE_PATH="/home/ubuntu/yolov5/data/images/uploaded_image.jpg"
echo "Downloading image from S3 to $IMAGE_PATH..."
aws s3 cp s3://unity-na/uploaded_image.jpg $IMAGE_PATH

# Python 경로 확인 및 torch 버전 확인
echo "Checking Python3 environment..."
which python3  # python3 경로 확인
python3 -c "import torch; print(torch.__version__)"  # torch 버전 확인

# YOLO 분석 실행
echo "Running YOLO analysis on $IMAGE_PATH..."
python3 /home/ubuntu/yolov5/detect.py --weights /home/ubuntu/yolov5/best.pt --img 640 --conf 0.25 --source $IMAGE_PATH --save-txt
if [ $? -ne 0 ]; then
  echo "YOLO analysis failed."
  exit 1
fi

# YOLO 분석 결과 파일 확인
LABEL_PATH="/home/ubuntu/yolov5/runs/detect/exp/labels/uploaded_image.txt"
if [ ! -f "$LABEL_PATH" ]; then
  echo "No objects detected. Proceeding with empty recommendation."
  touch "$LABEL_PATH"  # 빈 파일 생성하여 후속 처리가 가능하게 함
fi

# 추천 가구 필터링
echo "Filtering recommendations..."
python3 /home/ubuntu/yolov5/recommendation_filter.py
if [ $? -ne 0 ]; then
  echo "Recommendation filtering failed."
  exit 1
fi

# Flask 환경 변수 설정
export FLASK_APP=/home/ubuntu/yolov5/app.py
export FLASK_ENV=development  # 프로덕션 환경에서 개발 환경으로 변경 가능

# Flask 서버 백그라운드에서 실행
echo "Starting Flask web server..."
nohup python3 /home/ubuntu/yolov5/app.py > /home/ubuntu/flask.log 2>&1 &
if [ $? -ne 0 ]; then
  echo "Flask server failed to start."
  exit 1
fi

# 성공 메시지
echo "run_all.sh executed successfully."
