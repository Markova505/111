using System.Windows.Forms;

namespace DairyDemo.Auth;

static class ApplicationConfiguration
{
    public static void Initialize()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(true);
    }
}
