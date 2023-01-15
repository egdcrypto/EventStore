variable "app_name" {
  type    = string
  default = ""
}
variable "resource_group_location" {
  type    = string
  default = ""
}
variable "env" {
  type    = string
  default = ""
}
variable "inf_env" {
  type    = string
  default = ""
}

variable "tfstate_resource_group_name" {
  type    = string
  default = ""
}

variable "tfstate_storage_account_name" {
  type    = string
  default = ""
}

variable "tfstate_container_name" {
  type    = string
  default = ""
}

variable "database_name" {
  type    = string
  default = ""
}

variable "topic_name" {
  type    = string
  default = ""
}

variable "mongo-db-connection" {
  type    = string
  default = ""
}

variable "service-bus-connection" {
  type    = string
  default = ""
}

