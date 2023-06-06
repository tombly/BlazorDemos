
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

## Caveat #2: AOT Limitations

iOS apps are typically built using ahead-of-time (AOT) compilation which was originally necessary to adhere to Apple's policies. A more recent option is to use the Xamarin Interpreter which uses just-in-time (JIT) compilation while still adhering to Apple's guidelines.

The advantage of AOT is that the code executes faster and is smaller, with the disadvantage of not supporting specific dynamic typing scenarios.

The advantage of JIT is that it can handle dynamic typing, but has the disadvantage of slower execution time and larger executable (since JIT apps cannot be linked to remove unreferenced code).

It appears that Telerik UI for Blazor uses some C# language features that are not supported with AOT (related to how their component input parameters are initialized). The components throw an error when the page is rendered (TODO LINK TO IMAGE).

Telerik acknowledges the issue and recommends using JIT. Coincidentally, SyncFusion's Blazor components also do not support AOT and require JIT. [Details](https://blazor.syncfusion.com/documentation/getting-started/maui-blazor-app).

TODO: code execution/size comparison for small/large apps.
