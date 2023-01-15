terraform {
  required_version = "1.2.2"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.5.0"
    }
  }

  backend "azurerm" {
  }
}

provider "azurerm" {
  features {}
}



data "azurerm_service_plan" "service_plan" {
  name                = "${var.app_name}-${var.env}-asp"
  resource_group_name = data.azurerm_resource_group.rg.name
}

resource "azurerm_application_insights" "ai" {
  name                = "${var.app_name}-${var.env}-ai"
  location            = var.resource_group_location
  resource_group_name = data.azurerm_resource_group.rg.name
  application_type    = "web"
}

data "azurerm_subnet" "subnet" {
  name                 = "${local.app_name}-subnet"
  resource_group_name  = "inf-${var.inf_env}"
  virtual_network_name = data.terraform_remote_state.inf.outputs.east_vnet_name
}

resource "azurerm_windows_web_app" "app" {
  name                = "${var.app_name}-worker-${var.env}"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  service_plan_id     = data.azurerm_service_plan.service_plan.id

  app_settings = {
    AzureServicesAuthConnectionString = "RunAs=App;AppId=${data.azurerm_user_assigned_identity.managed-id.client_id}"
    APPINSIGHTS_INSTRUMENTATIONKEY    = azurerm_application_insights.ai.instrumentation_key
    TopicName                         = var.topic_name
    DatabaseName                      = var.database_name
    DD_API_KEY = ""
    DD_ENV = var.env
    DD_LOGS_INJECTION = "true"
    DD_SERVICE = "${var.app_name}-worker-${var.env}"
    DD_SITE = "us3.datadoghq.com"
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [data.azurerm_user_assigned_identity.managed-id.id]
  }

  dynamic "connection_string" {
    for_each = local.connection_list
    content {
      name  = connection_string.value["name"]
      type  = connection_string.value["type"]
      value = connection_string.value["value"]
    }
  }

  site_config {
    always_on              = true
    use_32_bit_worker      = false
    vnet_route_all_enabled = true

    application_stack {
      current_stack = "dotnet"
      dotnet_version = "v6.0"
    }
  }
}

resource "azurerm_app_service_virtual_network_swift_connection" "appsubnet" {
  app_service_id = azurerm_windows_web_app.app.id
  subnet_id      = data.azurerm_subnet.subnet.id
}
