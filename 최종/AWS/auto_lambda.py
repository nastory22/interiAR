import boto3
import time

# AWS SDK 클라이언트 생성
ssm_client = boto3.client('ssm')

def lambda_handler(event, context):
    ec2_instance_id = "i-07b5d7c1377abe146"  # 실제 EC2 인스턴스 ID로 변경
    image_name = event['Records'][0]['s3']['object']['key']
    print(f"Processing image: {image_name}")

    # EC2에서 실행할 명령어
    full_command = """
    sudo apt-get update -y &&
    sudo apt-get install python3-pip -y &&
    pip3 install torch &&
    pip3 install ultralytics &&  # ultralytics 패키지 설치
    chmod +x /home/ubuntu/yolov5/run_all.sh &&
    /home/ubuntu/yolov5/run_all.sh {image_name}
    """.format(image_name=image_name)

    # SSM을 통해 EC2에서 명령 실행
    response = ssm_client.send_command(
        InstanceIds=[ec2_instance_id],
        DocumentName="AWS-RunShellScript",
        Parameters={'commands': [full_command]},
        TimeoutSeconds=7200  # 타임아웃을 2시간(7200초)으로 설정
    )

    command_id = response['Command']['CommandId']
    print(f"Command ID: {command_id}")

    # 명령 상태를 주기적으로 확인하기
    is_command_complete = False
    retries = 10  # 최대 재시도 횟수
    attempt = 0

    # 명령 실행 후 잠시 대기
    time.sleep(10)  # 명령 실행 후 10초 대기

    while not is_command_complete and attempt < retries:
        try:
            # 명령 실행 상태 확인
            command_invocation = ssm_client.get_command_invocation(
                CommandId=command_id,
                InstanceId=ec2_instance_id
            )

            status = command_invocation['Status']
            print(f"Command invocation status: {status}")

            if status == 'Success':
                is_command_complete = True
            elif status == 'Failed':
                is_command_complete = True
                error_message = command_invocation.get('StandardErrorContent', 'No error message provided.')
                print(f"Command failed: {error_message}")
            else:
                # 명령이 아직 실행 중이면 1분 대기 후 재시도
                time.sleep(60)
                attempt += 1

        except Exception as e:
            print(f"Error occurred while checking command status: {e}")
            time.sleep(60)
            attempt += 1

    # 명령이 완료되었으면 결과 반환
    if is_command_complete:
        return {
            'statusCode': 200,
            'body': f"Command to process {image_name} with YOLO has been successfully executed on EC2."
        }
    else:
        return {
            'statusCode': 500,
            'body': f"Command failed to execute after {retries} attempts for {image_name}."
        }
