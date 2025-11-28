## Plan: Upgrade Blazor Static Web App to .NET 10 ✅ COMPLETED

The solution has been successfully upgraded to .NET 10 with all warnings resolved:

### Completed Steps

1. ✅ **Removed unused `Api` template project** — Deleted the in-process Azure Functions folder and removed from `BlazorStaticWebApps.sln`

2. ✅ **Removed unused `ApiIsolated` project** — Deleted the isolated worker Azure Functions folder and removed from solution (not in use)

3. ✅ **Upgraded `Shared` library** — Changed `<TargetFramework>` from `netstandard2.0` to `net10.0`

4. ✅ **Upgraded `Client` Blazor WebAssembly** — Updated `<TargetFramework>` to `net10.0` and upgraded all Microsoft packages from 8.0.0 to 10.0.0:
   - `Microsoft.AspNetCore.Components.WebAssembly` 8.0.0 → 10.0.0
   - `Microsoft.AspNetCore.Components.WebAssembly.DevServer` 8.0.0 → 10.0.0
   - Removed redundant `System.Net.Http.Json` package (included in .NET 10)

5. ✅ **Fixed all nullable reference warnings** — Resolved 22 compiler warnings:
   - Added `required` modifier to dependency-injected properties
   - Initialized collections and string properties with default values
   - Added null coalescing operators for API calls
   - Added null checks for file operations
   - Replaced `ReadAsync` with `ReadExactlyAsync` to fix CA2022 warning

6. ✅ **Verified build** — Solution builds successfully with **zero errors and zero warnings**

### Final Solution Structure

The solution now contains only two projects:
- **Client** (.NET 10) - Blazor WebAssembly frontend
- **Shared** (.NET 10) - Shared models and types

Both Azure Functions projects have been removed as they were not being used.

### Build Status

✅ **Clean build with no warnings or errors** - The solution is fully upgraded to .NET 10 with modern C# practices including proper nullable reference type handling.

### Further Considerations

1. **Replace Newtonsoft.Json with System.Text.Json?** — You already reference `System.Net.Http.Json` 8.0.0. Removing `Newtonsoft.Json` would reduce bundle size and improve performance, but requires updating any code using `JsonConvert` methods

2. **Add global.json for SDK version pinning?** — Creating a `global.json` at solution root ensures consistent .NET 10 SDK usage across development machines and CI/CD pipelines

3. **Update Azure Static Web Apps configuration?** — Verify `staticwebapp.config.json` and deployment settings support .NET 10 Functions runtime
