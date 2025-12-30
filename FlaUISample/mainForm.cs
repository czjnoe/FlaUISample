using FlaUI.UIA3;

namespace FlaUISample
{
    /// <summary>
    /// FlaUI：UI 自动化库
    /// https://github.com/FlaUI/FlaUI
    /// FlaUInspect：一个基于 FlaUI 的 UI 自动化检查工具
    /// https://github.com/FlaUI/FlaUInspect
    /// </summary>
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // 1️⃣ 启动应用
            string exePath = @"C:\Program Files\Notepad++\notepad++.exe";
            FlaUI.Core.Application app = null;
            while (app == null)
            {
                try
                {
                    // 尝试 Attach 已启动进程
                    app = FlaUI.Core.Application.Attach("notepad++");
                }
                catch
                {
                    // 没有启动就 Launch
                    try
                    {
                        app = FlaUI.Core.Application.Launch(exePath);
                    }
                    catch
                    {
                        // 程序还没准备好
                        Thread.Sleep(1000);
                    }
                }
            }

            using var automation = new UIA3Automation();
            var mainWindow = app.GetMainWindow(automation);
            Console.WriteLine("窗口标题：" + mainWindow.Title);
        }
    }
}
