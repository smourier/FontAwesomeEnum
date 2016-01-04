# FontAwesomeEnum
Creates enums and resources C# classes to be able to use the Font-Awesome icons set (currently version 4.5, available here: http://fortawesome.github.io/Font-Awesome/) in .NET a WPF applications.

To use it, just add the .ttf file in the project's root, and add this to the app.xaml file for example (change MyAssembly to match the project's name):

<Application x:Class="SoftFluent.SiteBuilder.Client.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <FontFamily x:Key="FontAwesome">/MyAssembly;component/#fontawesome</FontFamily>
    </Application.Resources>
</Application>

