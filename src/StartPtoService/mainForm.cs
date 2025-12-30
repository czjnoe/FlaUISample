using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Security.Policy;

namespace StartPtoService
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 启用所有程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            CloseAll();
            string serverTrayPath = @"C:\Program Files\PEER Group\PTO 8.6 SP2\Server\PSI.ServerTray.exe";
            if (File.Exists(serverTrayPath))
                Process.Start(serverTrayPath);

            string mcServerPath = @"C:\MachineControl\mc-api\Start.bat";
            if (File.Exists(mcServerPath))
                if (File.Exists(mcServerPath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k \"{mcServerPath}\"",  // /k 会执行后保持窗口
                        WorkingDirectory = Path.GetDirectoryName(mcServerPath),
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }

            string mcmServerPath = @"C:\MachineControl\mcm-api\Start.bat";
            if (File.Exists(mcmServerPath))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k \"{mcmServerPath}\"",  // /k 会执行后保持窗口
                    WorkingDirectory = Path.GetDirectoryName(mcmServerPath),
                    UseShellExecute = true
                };
                Process.Start(psi);
            }

            string peServerPath = @"C:\MachineControl\pe-web\Start.bat";
            if (File.Exists(peServerPath))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k \"{peServerPath}\"",  // /k 会执行后保持窗口
                    WorkingDirectory = Path.GetDirectoryName(peServerPath),
                    UseShellExecute = true
                };
                Process.Start(psi);
            }

            string textDriverSimulatorPath = @"C:\EFEM-Simulation\Debug\TextDriverSimulator.exe";
            var textDriverSimulatorApp = FlaUI.Core.Application.Launch(textDriverSimulatorPath);
            FlaUI.Core.AutomationElements.Window textDriverSimulatorWindow = null;
            using var automation = new UIA3Automation();

            textDriverSimulatorWindow = (Retry.WhileNull<FlaUI.Core.AutomationElements.Window>(() =>
            {
                return textDriverSimulatorApp.GetMainWindow(automation);
            }, timeout: TimeSpan.FromSeconds(10)
            )).Result;//等待窗口

            // var btnListen = textDriverSimulatorWindow.FindFirstDescendant(cf =>
            //cf.ByAutomationId("btnListen"))?
            //.AsButton();
            // btnListen?.Click();//获取 btnListen 控件

            var btnListen = (Retry.WhileNull<FlaUI.Core.AutomationElements.Button>(() =>
                textDriverSimulatorWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnListen")).AsButton(),
                TimeSpan.FromSeconds(5)
            )).Result;//等待 btnListen 控件出现
            btnListen?.Click();

            var rtxbLog = (Retry.WhileNull<FlaUI.Core.AutomationElements.TextBox>(() =>
                textDriverSimulatorWindow.FindFirstDescendant(cf => cf.ByAutomationId("rtxbLog"))
                                    ?.AsTextBox(), TimeSpan.FromSeconds(5)
           )).Result;//等待 rtxbLog 控件出现
            bool connectedSuccess = false;
            while (!connectedSuccess)
            {
                connectedSuccess = rtxbLog.Text.Contains("AcceptTcpClient Connected");
            }//监听RichTextBox 内容，直到出现“AcceptTcpClient Connected”
            var sendBtn = (Retry.WhileNull<FlaUI.Core.AutomationElements.Button>(() =>
              textDriverSimulatorWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnSendALUCommand")).AsButton(),
              TimeSpan.FromSeconds(5)
          )).Result;//等待 btnListen 控件出现
            sendBtn?.Click();


            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            StartChrome("http://localhost:5000");
            Thread.Sleep(2 * 1000);  // 等待几秒让页面加载
            MessageBox.Show("启动成功");
        }

        /// <summary>
        /// 打开chrome 网站
        /// </summary>
        /// <param name="targetUrl"></param>
        private void StartChrome(string targetUrl)
        {
            using var automation = new UIA3Automation();

            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            try
            {
                // 直接启动 Chrome 并打开网址
                // Chrome 会自动处理已运行/未运行的情况
                Process.Start(new ProcessStartInfo
                {
                    FileName = chromePath,
                    Arguments = targetUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseAll();
            MessageBox.Show("关闭成功");
        }

        /// <summary>
        /// 关闭所有程序
        /// </summary>
        private void CloseAll()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("dotnet", "."); // use "." for this machine
            foreach (var proc in procs)
                proc.Kill(true);

            System.Diagnostics.Process[] textDriverSimulatorProcs = System.Diagnostics.Process.GetProcessesByName("TextDriverSimulator", "."); // use "." for this machine
            foreach (var proc in textDriverSimulatorProcs)
                proc.Kill(true);

            System.Diagnostics.Process[] ptoProcs = System.Diagnostics.Process.GetProcessesByName("PSI.ServerTray", "."); // use "." for this machine
            foreach (var proc in ptoProcs)
                proc.Kill(true);
        }
    }
}
