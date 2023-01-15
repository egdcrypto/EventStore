data "terraform_remote_state" "inf" {
  backend = "azurerm"
  config = {
    resource_group_name  = var.tfstate_resource_group_name
    storage_account_name = var.tfstate_storage_account_name
    container_name       = var.tfstate_container_name
    key                  = "infrastructure/inf-${var.inf_env}/terraform.tfstate"
  }
}

data "azurerm_resource_group" "rg" {
  name = "${var.app_name}-${var.env}-rg"
}

data "azurerm_user_assigned_identity" "managed-id" {
  name                = "${local.app_name}-uai"
  resource_group_name = data.azurerm_resource_group.rg.name
}

locals {
  app_name = "${var.app_name}-${var.env}"
  connection_list = [
    {
      name  = "MongoDB"
      type  = "Custom"
      value = var.mongo-db-connection
    },
    {
      name  = "ServiceBus"
      type  = "Custom"
      value = var.service-bus-connection
    }
  ]
}