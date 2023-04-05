using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ClientOnamaeDdns
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var argData = ParseArgs(args);

                // 現在のグローバル IP を取得
                var httpClient = new HttpClient();
                var ip = await httpClient.GetStringAsync("https://api.ipify.org");
                WriteLineToConsole($"My public IP address is: {ip}");

                // グローバル IP が更新されているか確認
                var currentIp = ReadCurrentIp();
                if (currentIp == ip)
                {
                    WriteLineToConsole("IP is NOT changed");
                    return;
                }

                // 引数から諸々の情報を取得
                var hostName = "ddnsclient.onamae.com";
                var port = 65010;
                var userId = argData["user_id"];
                var password = argData["password"];
                var host = argData["hostname"];
                var domname = argData["domainname"];

                // 現在のグローバル IP を DDNS サーバーに通知
                using (var client = new TcpClient())
                {
                    var ipAddresses = Dns.GetHostAddresses(hostName);
                    client.Connect(new IPEndPoint(ipAddresses[0], port));

                    using (var sslStream = new SslStream(client.GetStream(), false, (s, ce, ch, errors) => true))
                    {
                        sslStream.AuthenticateAsClient(hostName);

                        var requests = new string[]
                        {
                            $"LOGIN\nUSERID:{userId}\nPASSWORD:{password}\n.\n",
                            $"MODIP\nHOSTNAME:{host}\nDOMNAME:{domname}\nIPV4:{ip}\n.\n",
                            $"LOGOUT\n.\n",
                        };

                        foreach (var request in requests)
                        {
                            WriteLineToConsole(request);
                            sslStream.Write(Encoding.UTF8.GetBytes(request));
                        }
                        sslStream.Flush();

                        var resBuffer = new byte[1024];
                        int n;
                        while ((n = sslStream.Read(resBuffer, 0, resBuffer.Length)) > 0)
                        {
                            var s = Encoding.ASCII.GetString(resBuffer, 0, n);
                            WriteLineToConsole(s);
                        }
                    }
                }
                // 現在のグローバル IP を外部ファイルに保存
                WriteCurrentIp(ip);
                WriteLogLine(ip);
                WriteLineToConsole("Finished!");
            }
            catch (Exception ex)
            {
                WriteLineToConsole(ex.Message);
                WriteLineToConsole(ex.StackTrace);
            }
        }

        static Dictionary<string, string> ParseArgs(string[] args)
        {
            var result = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                var delimiter = arg.IndexOf('=');
                if (delimiter < 0) { continue; }
                var key = arg.Substring(0, delimiter).Trim('-');
                var value = arg.Substring(delimiter + 1).Trim('\"');
                result[key] = value;
            }
            return result;
        }
        static void WriteLineToConsole(string message)
        {
            Console.WriteLine(message);
        }
        static string GetAppDataDir()
        {
            var dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClientDdns");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return dirPath;
        }
        static string GetTxtPath()
            => Path.Combine(GetAppDataDir(), "ip.txt");
        static string ReadCurrentIp()
        {
            var txtPath = GetTxtPath();
            if (File.Exists(txtPath))
            {
                var text = File.ReadAllText(txtPath);
                return text;
            }
            return "";
        }
        static void WriteCurrentIp(string ip)
        {
            File.WriteAllText(GetTxtPath(), ip);
        }
        static void WriteLogLine(string line)
        {
            var logPath = Path.Combine(GetAppDataDir(), "log.txt");
            var nowDate = DateTime.Now;
            var logLine = $"{nowDate} : {line}";
            File.AppendAllLines(logPath, new string[] { logLine });
        }
    }
}