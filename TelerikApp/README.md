
# Introduction

Using Blazor for the user interface of a mobile app allows developers to use web tools and languages to write the UI once and run it on the web or any mobile/desktop platform supported by MAUI, reducing time to market. Taking that one step further, using pre-built UI control suites saves additional time because developers don't need to create complex controls and layouts.

Telerik is one of the most popular UI control suites that offers Blazor UI components and officially supports using those components in Blazor hybrid apps ([link](https://docs.telerik.com/blazor-ui/hybrid-blazor-apps)). In practice, though, there are a few important caveats that must be addressed.

# JavaScript compatibility

Telerik UI for Blazor officially supports only the latest version of iOS and Android for hybrid apps (more specifically, it supports the WebViews that are included with the latest OS versions). This is because the JavaScript behind the components uses recent language features and syntax (e.g. the null termination operator) that are not supported by the WebViews included with older OS versions.

This has different implications for iOS vs. Android. Android's WebView can be updated independently from the OS (it's distributed as an app via the Play Store) so that devices running older OS versions can also use the same WebViews that ship with the latest OS versions. Note that this may require the user to manually update the WebView "app". This is not possible on iOS - the WebView that ships with iOS can only be updated via an OS update.

We can still use Telerik's UI for Blazor with the WebViews provided in older mobile OS's, we just need to modify the JavaScript that ships with it so that these older WebViews can run it. The following section demonstrates how to do this and assumes that we've created a simple Blazor hybrid app and followed the [instructions for adding Telerik UI for Blazor to the app](https://docs.telerik.com/blazor-ui/getting-started/hybrid-blazor).

## Transpiling & Polyfilling

One of the steps in adding Telerik UI for Blazor to a hybrid app is to add a `<script>` element to the `index.html` to load all the JavaScript code needed by the components:

``` html
<script src="_content/Telerik.UI.for.Blazor/js/telerik-blazor.js" defer></script>
```

If we then run the app on a device running the latest iOS or Android version then everything works great. However, if we run the app on an older OS, such as Android 11 (API 30) the WebView fails to load and throws the following error:

```
[chromium] [INFO:CONSOLE(28)] "Uncaught SyntaxError: Unexpected token '='", source: https://0.0.0.0/_content/Telerik.UI.for.Blazor/js/telerik-blazor.js (28)
```

If you take a look at the JavaScript file you'll see that it's failing on the use of the Nullish Coalescing operator `??` because this was added in ECMAScript 2020 (in June) and Android 11 was released shortly thereafter (September). We can use [Babel](https://babeljs.io) to transpile this sort of newer syntax down to older syntax that older WebViews can understand. There's a second issue as well, even if we transpiled the code and then bundled it up (see below) and used it in our app we'd get a different error:

```
[chromium] [INFO:CONSOLE(2)] "Uncaught TypeError: t.split(...).at is not a function", source: https://0.0.0.0/js/telerik-blazor-custom.js (2)
```

This is because, in addition to syntactical differences, newer JavaScript standards also include new functions that don't exist in older standards. In this case it's the `String.split()` function that is missing, so we'll need to address this by instructing Babel to use [Core-JS](https://github.com/zloirock/core-js) to provide polyfilling, that is, to add these missing functions that are built-in to newer WebViews.

Create a subfolder in your solution folder and install Babel:

``` bash
mkdir babel
cd babel
npm install @babel/core @babel/cli @babel/preset-env core-js
```

Create a config file for Babel called `babel.config.json`. Here we're using the `env` preset that include all plugins to support modern JavaScript (ES2015, ES2016, etc.). The `env` preset supports targeting specific browsers and versions so only the necessary transforms are applied. It doesn't support targeting specific mobile WebViews, but we can look in the Android Settings to see what version of Chromium ships with the OS we want to support, and for iOS the best we can do is choose a version of Safari that was released with the OS we want to support. The `useBuiltIns` option instructs Babel to import any necessary functions from Core-JS (i.e. to polyfill).

``` json
{
  "presets": [
    [
      "@babel/preset-env",
      {
        "targets": {
          "chrome": "67",
          "safari": "11.1"
        },
        "useBuiltIns": "usage",
        "corejs": {
          "version": "3.32.1"
        }
      }
    ]
  ]
}
```

Run Babel to transpile and polyfill the JavaScript that ships with the Telerik components. We have to copy the file locally since Babel will add `require()` statements with relative paths to the Core-JS modules it needs.

``` bash
cp ~/.nuget/packages/telerik.ui.for.blazor/4.3.0/staticwebassets/js/telerik-blazor.js .
./node_modules/.bin/babel telerik-blazor.js -o telerik-blazor-custom.js
```

Update the `index.html` file to reference our custom version instead of what ships with Telerik:

``` html
<script src="js/telerik-blazor-custom.js" defer></script>
```

If we launch the Android app now we'll get another error:

```
[chromium] [INFO:CONSOLE(1)] "Uncaught ReferenceError: require is not defined", source: https://0.0.0.0/js/telerik-blazor-custom.js (1)
```

This is because Babel generates CommonJS code (i.e. it uses `require()` to pull in the necessary functions for polyfilling) which can't be used in a broswer so we need to bundle the code so that a browser can use it.

## Bundling

We can use [WebPack](https://webpack.js.org) to bundle our transpiled/polyfilled code into something a browser can use.

Update the `package.json` file as such:

``` json
{
  "name": "webpack-telerik",
  "version": "",
  "description": "",
  "private": true,
  "scripts": {
  },
  "keywords": [],
  "author": "",
  "license": "",
  "dependencies": {
    "@babel/cli": "^7.22.10",
    "@babel/core": "^7.22.10",
    "@babel/preset-env": "^7.22.10",
    "core-js": "^3.32.1"
  }
}
```

Install Webpack:

``` bash
npm install webpack webpack-cli
```

Create a config file named `webpack.config.js`:

``` js
const path = require('path');

module.exports = {
  entry: path.resolve(__dirname, 'telerik-blazor-custom.js'),
  module: {
    rules: [
      {
        test: /\.(js)$/,
        exclude: /node_modules/
      }
    ]
  },
  resolve: {
    extensions: ['*', '.js']
  },
  output: {
    path: path.resolve(__dirname, '.'),
    filename: 'telerik-blazor-custom.js',
    library: 'TelerikBlazor',
    libraryTarget: "var",
  },
  devServer: {
    static: path.resolve(__dirname, '.'),
  },
};
```

Run WebPack and move the file to the mobile app's `wwwroot` folder:

``` bash
npx webpack
mv telerik-blazor-custom.js ../MobileApp/wwwroot/js/
```

Now when we build and launch the mobile app the Telerik controls load correctly and we're good to go. This process will need to be performed any time Telerik releases an updated version of UI for Blazor. Hopefully in the future Telerik will start supporting both the latest and previous mobile OS versions so that we no longer need to do this.


# AOT limitations

iOS apps are typically built using ahead-of-time (AOT) compilation which was originally necessary to adhere to Apple's policies. A more recent option is to use the Xamarin Interpreter which uses just-in-time (JIT) compilation while still adhering to Apple's App Store policies (or at least that was the intention).

The advantage of AOT (in addition to official support by Apple) is that the code executes faster, but has the disadvantages of longer build times and does not support some scenarios in which generics are used [Microsoft Learn](https://learn.microsoft.com/en-us/xamarin/ios/internals/limitations).

The advantages of JIT include support for any use of generics, builds are faster, and executables are smaller, but has the disadvantage of slower execution time (and possibly policy issues with Apple's App Store). Using JIT may also result in less-useful stack traces from crashes (as pointed out by Microsoft's docs [Microsoft Learn](https://learn.microsoft.com/en-us/xamarin/mac/internals/aot
)).

It appears that Telerik UI for Blazor uses some C# language features that are not supported with AOT (related to how their component input parameters are initialized). The components throw an error when the page is rendered. This is a known bug in ASP .NET and is currently planned to be fixed in .NET 8.0.0. [GitHub Issue](https://github.com/dotnet/runtime/issues/74015)

```
Unable to set property 'Enabled' on object of type 'Telerik.Blazor.Components.TelerikTextBox'. The error was: Attempting to JIT compile method '(wrapper delegate-invoke) void :invoke_callcirt_void_TelerikTextBox_bool (Telerik.Blazor.Components.TelerikTextBox,bool)' while running in aot-only mode.
```

Telerik acknowledges the issue and recommends using JIT. Coincidentally, SyncFusion's Blazor components also do not support AOT and require JIT. [Details](https://blazor.syncfusion.com/documentation/getting-started/maui-blazor-app).

## Analysis

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
