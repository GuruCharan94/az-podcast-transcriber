param
(
    [string]$SubscriptionName,
    [string]$ResourceGroupName
)

$URI = "https://westus.cris.ai/api/speechtotext/v2.1/transcriptions/hooks"

Set-AzContext -SubscriptionName $SubscriptionName
Get-AzResourceGroup -Name $ResourceGroupName ` -ErrorAction SilentlyContinue ` -ErrorVariable rgError

if ($rgError)
{ # Resource Group Does not exists. Create a new one.   
   New-AzResourceGroup -Name $ResourceGroupName -Location "West US"
}
$Result = New-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile "./armtemplate.json"

# Using Get-AzCognitiveServicesAccountKey results below error. Hence the change to CLI.
# 'Get-AzCognitiveServicesAccountKey' command was found in the module 'Az.CognitiveServices', 
# but the module could not be loaded. Formore information, run 'Import-Module Az.CognitiveServices'.

$key=az cognitiveservices account keys list --name $Result.Outputs.speechServicesName.value --resource-group $ResourceGroupName | jq -r .key1

$headers = @{
    'Content-Type' = 'application/json'
    'Ocp-Apim-Subscription-Key' = $key
}

$webhooks = Invoke-RestMethod -URI $URI -Method Get -Headers $headers 

if ( $webhooks.Count -eq 0 )
{
    Write-Host "No WebHook Registered. Registering a new one right away."
    
    $body = @{
        "configuration" = @{ "url" = $Result.Outputs.functionAppWebhookURL.Value ; "secret"="<my_secret>"; }
        "events" = @("TranscriptionCompletion")
        "active" = $true
        "name" = "TranscriptionCompletionWebHook"
        "properties" = @{"Active" = "True"}
    } | ConvertTo-Json -Depth 5

    Invoke-RestMethod -URI $URI -Method Post -Headers $headers -Body $body

}
else
{
    Write-Host "WebHook Already Registered. Not registering a new one."
}