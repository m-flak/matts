variable "az_region" {
    type = string
    default = "eastus"
    description = "Azure Region"
}

variable "rg_name" {
    type = string
    default = "matthews-ats"
}

variable "identity_type" {
  type    = string
  default = "SystemAssigned"
}
