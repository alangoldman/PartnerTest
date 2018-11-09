Setup steps:
0. Have a tenant in the partner with an Azure subscription. The subscription should have a role assignment for "Foreign Principal"
1. Create an application in the partner directory with url of http://localhost
2. Change it to multitenant
3. Go into its manifest and change the requiredResourceAccess to:
```
  "requiredResourceAccess": [
    {
      "resourceAppId": "797f4846-ba00-4fd7-ba43-dac1f8f63013",
      "resourceAccess": [
        {
          "id": "41094075-9dad-400e-a0bd-54e686782033",
          "type": "Scope"
        }
      ]
    },
    {
      "resourceAppId": "00000003-0000-0000-c000-000000000000",
      "resourceAccess": [
        {
          "id": "09850681-111b-4a89-9bed-3f2cae46d706",
          "type": "Role"
        },
        {
          "id": "19dbc75e-c2e2-444c-a770-ec69d8559fc7",
          "type": "Role"
        },
        {
          "id": "741f803b-c850-494e-b5df-cde7c675a1ca",
          "type": "Role"
        }
      ]
    },
    {
      "resourceAppId": "00000002-0000-0000-c000-000000000000",
      "resourceAccess": [
        {
          "id": "78c8a3c8-a07e-4b9e-af1b-b5ccab50a175",
          "type": "Role"
        },
        {
          "id": "5778995a-e1bf-45b8-affa-663a9f3f4d04",
          "type": "Role"
        },
        {
          "id": "1cda74f2-2616-4834-b122-5cb1b07f8a59",
          "type": "Role"
        }
      ]
    }
  ],
```
4. Grant permissions to the app in the partner directory
5. Pre-consent the app's service principal: https://blogs.msdn.microsoft.com/iwilliams/2017/12/01/cloud-solution-provider-pre-consent/
6. Create a key for the application, place it in PartnerTest/App.Config: `BootstrapAppSecret`
7. Get a refresh token for the application, place it in PartnerTest/App.Config: `RefreshToken`
	1. First get an authcode using: https://login.microsoftonline.com/common/OAuth2/Authorize?client_id=<applicationid>&prompt=admin_consent&redirect_uri=http://localhost&response_mode=query&response_type=code
	2. Take the code from the url, and make the following HTTP request. THe response will have the `refresh_token`
```
POST https://login.microsoftonline.com/common/oauth2/token
Headers:
Content-Type:application/x-www-form-urlencoded

Body:
grant_type:authorization_code
code:<authcode>
client_id:<applicationId>
redirect_uri:http://localhost
client_secret:<applicationSecret>
resource:https://management.azure.com/
```
7. Put the application id, subscription id, tenant directory id in PartnerTest/Program.cs
8. Run the console application, it will list the service principals and resource groups