using R3.WinForms;

namespace WinFormsApp1;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var form = new Form1();

        WinFormsProviderInitializer.SetDefaultObservableSystem(static exception =>
            MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error),
            form);

        Application.Run(form);
    }
}
