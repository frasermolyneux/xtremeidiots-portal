# Manual Steps

* Chicken/Egg Bug: Key Vault is needed to store the SQL username/password and therefore needed for the Bicep stage. However is has not yet been deployed. Workaround for now is to manually grant permissions for the service principal to access Key Vault. Need to put in a proper fix though.

* `SetDeployPrincipalPermissions.ps1` has a hardcoded ID for the deploy principal
