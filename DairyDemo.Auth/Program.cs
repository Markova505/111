using DairyDemo.Auth.UI.Forms;

namespace DairyDemo.Auth;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }
}
