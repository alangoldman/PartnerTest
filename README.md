Setup steps:
1. Create an application in the partner directory
2. Change it to multitenant
3. Go into its manifest and change the requiredResourceAccess to:
```
"requiredResourceAccess": [
    {
      "resourceAppId": "00000002-0000-0000-c000-000000000000",
      "resourceAccess": [
        {
          "id": "311a71cc-e848-46a1-bdf8-97ff7156d8e6",
          "type": "Scope"
        },
        {
          "id": "a42657d6-7f20-40e3-b6f0-cee03008a62a",
          "type": "Scope"
        }
      ]
    },
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
          "id": "7ab1d382-f21e-4acd-a863-ba3e13f7da61",
          "type": "Role"
        },
        {
          "id": "19dbc75e-c2e2-444c-a770-ec69d8559fc7",
          "type": "Role"
        },
        {
          "id": "df021288-bdef-4463-88db-98f22de89214",
          "type": "Role"
        },
        {
          "id": "741f803b-c850-494e-b5df-cde7c675a1ca",
          "type": "Role"
        },
        {
          "id": "09850681-111b-4a89-9bed-3f2cae46d706",
          "type": "Role"
        }
      ]
    }
  ],
```
4. Grant permissions to the app in the partner directory
5. Pre-consent the app's service principal: https://blogs.msdn.microsoft.com/iwilliams/2017/12/01/cloud-solution-provider-pre-consent/
6. Create a key for the application, place it in PartnerTest/App.Config: `BootstrapAppSecret`
7. Create a new customer tenant from the Partner Center UI
8. Wait 3 minutes
9. Put the application id and tenant directory id in PartnerTest/Program.cs
10. Run the console application, observe the error `The identity of the calling application could not be established.`