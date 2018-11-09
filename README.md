Setup steps:
0. Have a tenant in the partner with an Azure subscription. The subscription should have a role assignment for "Foreign Principal"
1. Create an application in the partner directory
2. Change it to multitenant
3. Go into its manifest and change the requiredResourceAccess to:
```
  "requiredResourceAccess": [
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
7. Put the application id, subscription id, tenant directory id in PartnerTest/Program.cs
8. Run the console application, observe the error `The identity of the calling application could not be established.`