resource "azurerm_resource_group" "rg" {
  location = var.az_region
  name     = "${var.rg_name}-${random_string.rg_suffix.result}"
}

resource "azurerm_app_configuration" "matts_appconfig" {
  name                          = "${var.rg_name}-configuration"
  location                      = azurerm_resource_group.rg.location
  resource_group_name           = azurerm_resource_group.rg.name
  disable_local_auth            = false
  soft_delete_retention_in_days = 0
  enable_purge_protection        = false

  identity {
    type = var.identity_type
  }
}

resource "azurerm_key_vault" "matts_keyvault" {
  name                        = "${var.rg_name}-vault"
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 90
  purge_protection_enabled    = false
  enable_rbac_authorization   = true
  public_network_access       = true

  sku_name = "standard"
}

resource "random_string" "rg_suffix" {
  length = 3
  special = false
  upper = false
}
