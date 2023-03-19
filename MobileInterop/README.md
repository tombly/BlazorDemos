
# Mobile Interop with Blazor

When Blazor components are embedded within mobile apps they can access a device's hardware functionality via standard HTML5 APIs, such as `navigator.geolocation` to obtain latitude/longitude, or `MediaDevices.getUserMedia()` to capture images from a camera. However, if a particular HTML5 device API isn't full-featured or well-supported by MAUI's `BlazorWebView` or if a particular device API isn't accessible via HTML5 APIs (e.g. Apple's Core ML) then Blazor components need a different way to invoke these device APIs.

This demo shows how multi-framework targeting in .NET can be used to create a single class library that contains separate platform-specific implementations. This is one way in which platform-agnostic Blazor components can access platform APIs.

There are 4 projects:

- CodeLib: A multi-target class library that provides separate platform-specific implementations of a class.
- ComponentLib: A Razor Class Library that contains a base layout `MainLayout` and 3 content components `HomeComponent`, `BlueComponent`, and `RedComponent`, where the layout contains a navigation menu from which the components can be accessed.
- WebApp: A web assembly app that simply hosts the base layout from the component library.
- MobileApp: A MAUI app that simply hosts the base layout from the component library.

The shared Blazor components (`ComponentLib`) are hosted by the web and mobile apps and make calls into the code library (`CodeLib`) which includes a single class `SharedClass` with a separate implementation for each targeted platform (.NET, iOS, and Android). For example, this is the Android implementation of the class:

```csharp
public static async Task<string?> GetPostalCode(string address)
{
    using (var geocoder = new Android.Locations.Geocoder(Android.App.Application.Context))
    {
        var addr = await geocoder.GetFromLocationNameAsync(address, 1);
        return addr?.First().PostalCode;
    }
}
```

The `CodeLib`'s project file includes conditionals that control which folders are included when building each of the targets. For example, the following condition excludes the Android-specific code implementation from the build when not building the Android target:

```xml
<!-- Android -->
<ItemGroup Condition="$(TargetFramework.StartsWith('net7.0-android')) != true">
    <Compile Remove="**\Android\**\*.cs" />
    <None Include="**\Android\**\*.cs" Exclude="$(DefaultItemExcludes);$(DefaultExcludesInProjectFolder)" />
</ItemGroup>
```

When the library is built, 3 sets of assemblies are created, one for each targeted platform. The Blazor component library `ComponentLib` only targets .NET but it still pulls in the platform-specific `CodeLib` assemblies when it's built. That way, when the mobile app is built (which uses multi-targeting) it gets the .NET assembly for the `ComponentLib` and the target-specific assemblies for the `CodeLib`.

The compiler ensures that each multi-target class shares the same signature across all of the target implementations (i.e. the methods in the class and the method signatures must match). If the implementation for a particular target doesn't match the others (e.g. if the iOS implementation of a class is missing a method) then the compiler throws an error.

In this example the .NET implementation of the class is used when the library runs under any target other than iOS or Android (conceptually a fall-back). This allows for graceful handing when device APIs aren't accessible.

> The key observation here is that the Blazor components remain platform-agnostic, but they exhibit platform-specific behaviors through the multi-target library.

# Essentials

One of the most common reasons to use the above pattern is to allow Blazor components to access native device APIs such as location services, battery, network connectivity, etc. MAUI Essentials (formerly Xamarin Essentials) provides a set of abstractions for many of these common device APIs. These can be made accessible in the `CodeLib` library by enabling MAUI (project file's `UseMAUI=true`) and then called as such:

```csharp
public static string GetConnectivity()
{
    return $"{Microsoft.Maui.Networking.Connectivity.NetworkAccess}";
}
```