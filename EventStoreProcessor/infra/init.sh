terraform init -reconfigure \
-backend-config="resource_group_name=" \
-backend-config="storage_account_name=" \
-backend-config="container_name=tf-environments" \
-backend-config="key=eventstore-worker/dev/terraform.tfstate"