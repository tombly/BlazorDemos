# UI Composition with MAUI

This demo shows how Blazor components can be shared between a web assembly app and a MAUI mobile (or desktop) app in two ways:

- The mobile app UI matches the web app UI.
- The mobile app has a customized UI.

There are 4 projects:

- ComponentLib: A Razor Class Library that contains a base layout `MainLayout` and 3 content components `HomeComponent`, `BlueComponent`, and `RedComponent`, where the layout contains a navigation menu from which the components can be accessed.
- WebAssemblyApp: A web assembly app that simply hosts the base layout from the library.
- AppWebNavigation: A MAUI app that simply hosts the base layout from the library.
- AppNativeNavigation: A MAUI app that uses native tabs to host the content components from the library.

The following diagram illustrates the difference between MAUI apps that share the base layout with the web app (right) and those that share the content components but use a native UI container (left):

TODO DIAGRAM HERE

On the right, the MAUI mobile app simply consists of a single `BlazorWebView` UI component that hosts the same `MainLayout` as the web assembly app. The advantage of this approach is that the app runs as a mobile app with no additional coding on the mobile side - the mobile app will look exactly like the web app running in a small browser window. The disadvantage is just that - the app will look and feel like it's just a website (although this can be mitigated somewhat with some clever styling).

On the left, the MAUI mobile app does not use the base layout but instead uses native tabs (defined in XAML) each of which contains a `BlazorWebView` to host the content components. The advantage of this approach is that the mobile app has a familiar tabbed user interface that is consistent with other native apps while still reusing the content components. The disadvantage is that this requires custom code in the MAUI app.

Combining Blazor with MAUI offers a great deal of flexibility - at one extreme the entire UI can be reused across web and mobile, while at the other extreme the entire mobile UI can be native with just a few embedded Blazor components. This flexibility is particularly advantageous for green field projects where the complete UI is shared initially and then the UI is customized for mobile over time as the product matures.
