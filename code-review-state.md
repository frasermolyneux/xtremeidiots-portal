# Code Review and Refactor State File

This file tracks the progress of our systematic code review and refactoring process according to the coding guidelines defined in `.github/instructions/` and `.github/copilot-instructions.md`.

## Review Guidelines Applied

- **ASP.NET REST APIs** (`aspnet-rest-apis.instructions.md`) - Applied to `**/*.cs, **/*.json`
- **Controllers** (`controllers.instructions.md`) - Applied to `**/Controllers/**/*.cs`
- **C# Guidelines** (`csharp.instructions.md`) - Applied to `**/*.cs`
- **Razor Views** (`razor-views.instructions.md`) - Applied to `**/*.cshtml`
- **Code Comments** (`code-comments.instructions.md`) - Applied to `**`
- **Copilot Instructions** (`.github/copilot-instructions.md`) - Applied to all code

## Files to Review

### Core Application Files
- [ ] `src/XtremeIdiots.Portal.Web/Program.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Startup.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/TelemetryInitializer.cs` ✅ **COMPLETED**

### Controllers (Priority - Follow specific controller patterns)
- [x] `src/XtremeIdiots.Portal.Web/Controllers/BaseController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/AdminActionsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/BanFileMonitorsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/BannersController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/ChangeLogController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/CredentialsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/DemosController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/ErrorsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/ExternalController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/GameServersController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/HealthCheckController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/HomeController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/IdentityController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/IPAddressesController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/MapManagerController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/MapPacksController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/MapsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/PlayerAnalyticsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/PlayersController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/PlayerTagsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/ProfileController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/ProtectedNamesController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/ServerAdminController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/ServersController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/StatusController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/TagsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Controllers/UserController.cs` ✅ **COMPLETED**

### API Controllers
- [x] `src/XtremeIdiots.Portal.Web/ApiControllers/BaseApiController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ApiControllers/BannersController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ApiControllers/DataController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ApiControllers/ExternalController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ApiControllers/HealthCheckController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ApiControllers/PlayerAnalyticsController.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ApiControllers/ServerAdminController.cs` ✅ **COMPLETED**

### Authorization System
- [x] `src/XtremeIdiots.Portal.Web/Auth/Constants/AuthPolicies.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/AdminActionsAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/BanFileMonitorsAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/ChangeLogAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/CredentialsAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/DemosAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/GameServersAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/HomeAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/MapsAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/PlayersAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/ProfileAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/ServerAdminAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/ServersAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/StatusAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Requirements/UsersAuthRequirements.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/AdminActionsAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/BanFileMonitorsAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/BaseAuthorizationHelper.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/ChangeLogAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/CredentialsAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/DemosAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/GameServersAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/HomeAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/MapsAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/PlayersAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/PlayerTagsAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/ProfileAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/ServerAdminAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/ServersAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/StatusAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/Handlers/UsersAuthHandler.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/XtremeIdiots/IXtremeIdiotsAuth.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/XtremeIdiots/XtremeIdiotsAuth.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Auth/XtremeIdiots/XtremeIdiotsAuthResult.cs` ✅ **COMPLETED**

### Models and ViewModels
- [x] `src/XtremeIdiots.Portal.Web/Models/Alert.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Models/DataTableAjaxPostModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Models/DataTableColumn.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Models/DataTableOrder.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Models/DataTableSearch.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Models/MapTimelineDataPoint.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Models/PlayerDtoExtensions.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/AddPlayerTagViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/CreateAdminActionViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/CreateBanFileMonitorViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/CreateMapPackViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/CreateProtectedNameViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/CreateTagViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/DeleteMapFromHostModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/EditAdminActionViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/EditBanFileMonitorViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/EditTagViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/GameServerViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/IPAddressDetailsViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/ManageMapsViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/PlayerDetailsViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/PlayerIpAddressViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/ProtectedNameReportViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/ProtectedNamesViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/PushMapToRemote.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/ServerAdminGameServerViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/ServersGameServerViewModel.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewModels/TagsViewModel.cs` ✅ **COMPLETED**

### Extensions and Helpers
- [x] `src/XtremeIdiots.Portal.Web/Extensions/AlertExtensions.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Extensions/ClaimsPrincipalExtensions.cs` ✅ **COMPLETED**

### Services
- [x] `src/XtremeIdiots.Portal.Web/Services/ProxyCheckService.cs` ✅ **COMPLETED**

### ViewComponents
- [x] `src/XtremeIdiots.Portal.Web/ViewComponents/AdminActionsViewComponent.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewComponents/GameServerListViewComponent.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/ViewComponents/PlayerTagsViewComponent.cs` ✅ **COMPLETED**

### Constants
- [x] `src/XtremeIdiots.Portal.Web/Constants/ChatLogSupportedGames.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/Constants/DemosSupportedGames.cs` ✅ **COMPLETED**

### Configuration Files
- [x] `src/XtremeIdiots.Portal.Web/appsettings.json` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/appsettings.Development.json` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Web/libman.json` ✅ **COMPLETED**

### Razor Views (Follow razor-views.instructions.md - **REQUIRES RE-REVIEW FOR AUTOMATION IDs**)

#### Core Layout and Shared Views
- [ ] `src/XtremeIdiots.Portal.Web/Views/_ViewImports.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/_ViewStart.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/_Layout.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/_Footer.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/_Navigation.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/_TopNavbar.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/_ValidationScriptsPartial.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)

#### Controller Views - AdminAction
- [ ] `src/XtremeIdiots.Portal.Web/Views/AdminAction/Create.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/AdminAction/Edit.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/AdminAction/Delete.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/AdminAction/Claim.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/AdminAction/Lift.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)

#### Controller Views - BanFileMonitors
- [ ] `src/XtremeIdiots.Portal.Web/Views/BanFileMonitors/Index.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/BanFileMonitors/Create.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/BanFileMonitors/Edit.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/BanFileMonitors/Details.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/BanFileMonitors/Delete.cshtml`

#### Controller Views - Banners
- [ ] `src/XtremeIdiots.Portal.Web/Views/Banners/GameServersList.cshtml`

#### Controller Views - ChangeLog
- [ ] `src/XtremeIdiots.Portal.Web/Views/ChangeLog/Index.cshtml`

#### Controller Views - Credentials
- [ ] `src/XtremeIdiots.Portal.Web/Views/Credentials/Index.cshtml`

#### Controller Views - Demos
- [ ] `src/XtremeIdiots.Portal.Web/Views/Demos/Index.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Demos/Delete.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Demos/DemoClient.cshtml`

#### Controller Views - Errors
- [ ] `src/XtremeIdiots.Portal.Web/Views/Errors/Display.cshtml`

#### Controller Views - External
- [ ] `src/XtremeIdiots.Portal.Web/Views/External/LatestAdminActions.cshtml`

#### Controller Views - GameServers
- [ ] `src/XtremeIdiots.Portal.Web/Views/GameServers/Index.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/GameServers/Create.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/GameServers/Edit.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/GameServers/Details.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/GameServers/Delete.cshtml`

#### Controller Views - Home
- [ ] `src/XtremeIdiots.Portal.Web/Views/Home/Index.cshtml`

#### Controller Views - Identity
- [ ] `src/XtremeIdiots.Portal.Web/Views/Identity/Login.cshtml`

#### Controller Views - IPAddresses
- [ ] `src/XtremeIdiots.Portal.Web/Views/IPAddresses/Details.cshtml`

#### Controller Views - MapManager
- [ ] `src/XtremeIdiots.Portal.Web/Views/MapManager/Manage.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/MapManager/DeleteMapFromHostPartial.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/MapManager/PushMapToRemotePartial.cshtml`

#### Controller Views - MapPacks
- [ ] `src/XtremeIdiots.Portal.Web/Views/MapPacks/Create.cshtml`

#### Controller Views - Maps
- [ ] `src/XtremeIdiots.Portal.Web/Views/Maps/Index.cshtml`

#### Controller Views - Players
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/Index.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/Details.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/Analytics.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/IpIndex.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/MyActions.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/Unclaimed.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/AddPlayerTag.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/RemovePlayerTag.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/AddProtectedName.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/ProtectedNames.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Players/ProtectedNameReport.cshtml`

#### Controller Views - Profile
- [ ] `src/XtremeIdiots.Portal.Web/Views/Profile/Manage.cshtml`

#### Controller Views - ServerAdmin
- [ ] `src/XtremeIdiots.Portal.Web/Views/ServerAdmin/Index.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/ServerAdmin/ChatLogIndex.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/ServerAdmin/ChatLogPermaLink.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/ServerAdmin/ViewRcon.cshtml`

#### Controller Views - Servers
- [ ] `src/XtremeIdiots.Portal.Web/Views/Servers/Index.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Servers/Map.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Servers/ServerInfo.cshtml`

#### Controller Views - Status
- [ ] `src/XtremeIdiots.Portal.Web/Views/Status/BanFileStatus.cshtml`

#### Controller Views - Tags
- [ ] `src/XtremeIdiots.Portal.Web/Views/Tags/Index.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Tags/Create.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Tags/Edit.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/Tags/Delete.cshtml`

#### Controller Views - User
- [ ] `src/XtremeIdiots.Portal.Web/Views/User/Index.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/User/ManageProfile.cshtml`
- [ ] `src/XtremeIdiots.Portal.Web/Views/User/Permissions.cshtml`

#### ViewComponents Templates
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/Components/AdminActions/Default.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/Components/GameServerList/Default.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Views/Shared/Components/PlayerTags/Default.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)

#### Identity Area Views
- [ ] `src/XtremeIdiots.Portal.Web/Areas/Identity/Pages/_ViewStart.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)
- [ ] `src/XtremeIdiots.Portal.Web/Areas/Identity/Pages/_ValidationScriptsPartial.cshtml` 🔄 **NEEDS RE-REVIEW** (Add automation IDs)

### Forums Integration Project
- [x] `src/XtremeIdiots.Portal.Integrations.Forums/AdminActionTopics.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Integrations.Forums/DemoManager.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Integrations.Forums/IAdminActionTopics.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Integrations.Forums/IDemoManager.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Integrations.Forums/Extensions/GameTypeExtensions.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Integrations.Forums/Extensions/ServiceCollectionExtensions.cs` ✅ **COMPLETED**
- [x] `src/XtremeIdiots.Portal.Integrations.Forums/Models/DemoManagerClientDto.cs` ✅ **COMPLETED**

