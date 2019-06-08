param
(
    [string]$SubscriptionName,
    [string]$ResourceGroupName
)

Set-AzContext -SubscriptionName $SubscriptionName
Get-AzResourceGroup -Name $ResourceGroupName ` -ErrorAction SilentlyContinue ` -ErrorVariable rgError

if ($rgError)
{ # Resource Group Does not exists. Create a new one.   
   New-AzResourceGroup -Name $ResourceGroupName -Location "West US"
}
New-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile "./armtemplate.json"