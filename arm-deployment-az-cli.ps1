$templateFilePath='videostorearmdeploy.json'
$myResourceGroupName='VideoStore'
$devParameterFilePath='videostorearmdeploy.parameters.json'
# az group create `
#   --name $myResourceGroupName `
#   --location 'West Europe'
az deployment group create `
  --name 'testenvironmentdeployment' `
  --resource-group $myResourceGroupName `
  --template-file $templateFilePath `
  --parameters $devParameterFile