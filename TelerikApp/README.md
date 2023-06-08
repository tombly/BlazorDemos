
# Introduction

Using Blazor for the user interface of a mobile app allows developers to use web tools and languages to write the UI once and run it on any platform (supported by MAUI) reducing time to market. Taking that one step further, using pre-built UI control suites saves additional time because developers don't need to create complex controls and layouts.

Telerik is one of the most popular UI control suites and includes Blazor UI components. Currently they do not officially support hybrid Blazor apps because these apps use web views instead of traditional web browsers, but the controls do work in iOS and Android applications with some caveats [Details](https://docs.telerik.com/blazor-ui/hybrid-blazor-apps).

## Caveat #1: JavaScript compatibility

Telerik UI for Blazor officially supports only the latest version of the 4 most common web browsers. The JavaScript implementation uses recent language features (like the null termination operator) which are supported by these browsers.

However, the web views that ship with mobile OS's have varying levels of JavaScript support depending on when the OS was released. If you need to support previous mobile OS versions, even just the current and previous, then your JavaScript may need to adhere to standards from several years ago and can't assume that new language features are available.

> Note that the Android OS does allow users to update the web view independently from the OS, making it easier for devices to support the latest JavaScript standards.

To workaround this issue, the JavaScript included in Telerik UI for Blazor can be transpiled and/or polyfilled to support older JavaScript standards. In initial investigations we found that it would work by using Babel to transpile the JavaScript and then manually adding a missing function.

``` bash
babel ...
```

 Your code would reference a custom JS file instead of the one that ships with the control suite.

 ``` csharp
 TODO: show index.html
 ```

## Caveat #2: AOT limitations

iOS apps are typically built using ahead-of-time (AOT) compilation which was originally necessary to adhere to Apple's policies. A more recent option is to use the Xamarin Interpreter which uses just-in-time (JIT) compilation while still adhering to Apple's App Store policies (or at least that was the intention).

The advantage of AOT (in addition to official support by Apple) is that the code executes faster, but has the disadvantages of longer build times and does not support some scenarios in which generics are used [Details](https://learn.microsoft.com/en-us/xamarin/ios/internals/limitations).

The advantages of JIT include support for any use of generics, builds are faster, and executables are smaller, but has the disadvantage of slower execution time (and possibly policy issues with Apple's App Store). Using JIT may also result in less-useful stack traces from crashes (as pointed out by Microsoft's docs [Details](https://learn.microsoft.com/en-us/xamarin/mac/internals/aot
)).

It appears that Telerik UI for Blazor uses some C# language features that are not supported with AOT (related to how their component input parameters are initialized). The components throw an error when the page is rendered (TODO LINK TO IMAGE).

Telerik acknowledges the issue and recommends using JIT. Coincidentally, SyncFusion's Blazor components also do not support AOT and require JIT. [Details](https://blazor.syncfusion.com/documentation/getting-started/maui-blazor-app).

### Analysis

Here is a comparison of AOT vs. JIT in terms of executable size and launch performance for a small demo app and a large production app.

The following command was used to build the IPA file:

``` bash
dotnet publish --configuration Release --framework net7.0-ios --runtime ios-arm64
```

The following was added to the csproj file to enable JIT:

``` xml
  <PropertyGroup>
    <UseInterpreter>true</UseInterpreter>
  </PropertyGroup>
```

The launch time was measured using Xcode Instruments and is the total time between when the app is launched and the UI is first rendered (which happens after all the application lifecycle callbacks (e.g. `didFinishLaunchingWithOptions`).

| Sample App | IPA size    | Launch time |
| ---------- | ----------- | ----------- |
| AOT        | 30.5 MB     | 1.201 sec.  |
| JIT        | 10.2 MB     | 1.037 sec.  |

The results are similar for a large production app:

| Prod App   | IPA size    | Launch time |
| ---------- | ----------- | ----------- |
| AOT        | 56.3 MB     | 1.905 sec.  |
| JIT        | 21.5 MB     | 1.475 sec.  |

There was no perceivable performance difference (screen load time, tap responsiveness, etc.) when navigating around the JIT app as compared to the AOT app. 

Interestingly, the launch time is faster with JIT, contrary to what Microsoft states: This is opposite of what Microsoft states: https://learn.microsoft.com/en-us/xamarin/mac/internals/aot
