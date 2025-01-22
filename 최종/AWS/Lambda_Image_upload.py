import json
import boto3
import base64
import os

s3 = boto3.client('s3')
BUCKET_NAME = os.environ['BUCKET_NAME']  # 환경 변수를 통해 S3 버킷 이름 설정

def lambda_handler(event, context):
    print("Received event: " + json.dumps(event))  # 디버그 로그 추가

    try:
        # 이미지 데이터 가져오기
        image_data = event.get('file')  # 'body'를 사용하지 않고 'file'에서 직접 가져오기
        
        if image_data is None:
            raise ValueError("No image data found in the request body")

        # Base64로 인코딩된 이미지를 디코딩
        image_bytes = base64.b64decode(image_data)  # Base64 디코딩
        file_name = "uploaded_image.jpg"  # JPG 형식으로 저장
        
        # S3에 이미지 저장
        response = s3.put_object(Bucket=BUCKET_NAME, Key=file_name, Body=image_bytes)
        print("S3 Response:", response)  # S3 응답 확인
        
        return {
            'statusCode': 200,
            'body': json.dumps('Image uploaded successfully!')
        }
    except Exception as e:
        print("Error:", str(e))  # 에러 메시지 출력
        return {
            'statusCode': 400,
            'body': json.dumps('Error uploading image: ' + str(e))
        }
