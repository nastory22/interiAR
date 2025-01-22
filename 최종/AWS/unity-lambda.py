import boto3
import time

s3_client = boto3.client('s3')
ec2_client = boto3.client('ec2')
ssm_client = boto3.client('ssm')

def lambda_handler(event, context):
    bucket_name = event['Records'][0]['s3']['bucket']['name']
    object_key = event['Records'][0]['s3']['object']['key']

    print(f"New image uploaded to S3 bucket: {bucket_name}/{object_key}")

    ec2_instance_id = "your ec2 ID"  # 올바른 인스턴스 ID
    command = f"aws s3 cp s3://{bucket_name}/{object_key} /home/ubuntu/yolov5/data/images"

    ec2_client.start_instances(InstanceIds=[ec2_instance_id])
    print("Starting EC2 instance...")
    
    while True:
        response = ec2_client.describe_instance_status(InstanceIds=[ec2_instance_id])
        instance_status = response['InstanceStatuses'][0]['InstanceState']['Name']
        if instance_status == 'running':
            print("EC2 instance is running.")
            # SSM 에이전트가 시작되고 준비될 때까지 대기
            time.sleep(10)  # 추가 대기 시간
            break
        else:
            print("Waiting for EC2 instance to start...")
            time.sleep(5)  # 5초 대기

    # SSM을 통해 EC2에서 S3 이미지 가져오는 명령어 실행
    response = ssm_client.send_command(
        InstanceIds=[ec2_instance_id],
        DocumentName="AWS-RunShellScript",
        Parameters={'commands': [command]}
    )

    command_id = response['Command']['CommandId']
    print(f"Command ID: {command_id}")

    return {
        'statusCode': 200,
        'body': f"Command to download {object_key} from S3 has been sent to EC2."
    }
