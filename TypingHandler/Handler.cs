using TextCopy;
using System.IO;
using System;
using System.Threading.Tasks;
using WindowsInput;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace TypingHandler
{
    public class Handler
    {
        private readonly TypingBridgeConfig appConfig;
        public bool IsSourceTypeValid { get; set; }
        public int SourceType { get; set; }
        public bool IsFileValid { get; set; }

        private readonly IInputSimulator input = new InputSimulator();

        public Handler(TypingBridgeConfig appConfig) 
        {
            this.appConfig = appConfig;
        }

        private string[] LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
                Console.WriteLine("Error: Cannot find file " + fileName);
            try
            {
                string[] strArray = File.ReadAllLines(fileName);
                IsFileValid = true;
                Console.WriteLine("File content loaded.");
                return strArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex);
                return (string[])null;
            }
        }

        private string[] LoadClipboard()
        {
            return ClipboardService.GetText().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        private  void DoTyping(string[] lines)
        {
            foreach (string line in lines)
            {
                foreach (var singleChar in line.ToCharArray())
                {
                    input.Keyboard.TextEntry(singleChar);
                    if (appConfig.LatencyInMs > 0)
                    {
                        Thread.Sleep(appConfig.LatencyInMs);
                    }
                }
                input.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
            }
            IsFileValid = false;
            Console.WriteLine("Completed.");
        }

        public void ReadSourceType() 
        {
            while (!IsSourceTypeValid)
            {
                Console.WriteLine("Please choose source: [0] clipboard, [1] file");
                if (int.TryParse(Console.ReadLine(), out var sourceType))
                {
                    if (sourceType < 0 || sourceType > 1)
                        Console.WriteLine("Incorrect input, please try again");
                    else
                    {
                        IsSourceTypeValid = true;
                        SourceType = sourceType;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect input, please try again");
                }
            }
        }
        public async Task DoType()
        {

            while (true)
            {
                string[] lines = null;

                switch (SourceType)
                {
                    case 0:
                        Console.WriteLine("Copy TEXT to clipboard then press ENTER.");
                        Console.ReadKey();
                        lines = LoadClipboard();
                        break;
                    case 1:
                        if (string.IsNullOrEmpty(appConfig.SourceFileName))
                        {
                            Console.WriteLine("Please specify file name in the appsettings.json");
                        }
                        else
                        {
                            Console.WriteLine($"Reading text from {appConfig.SourceFileName}");
                            while (!IsFileValid)
                            {
                                lines = LoadFile(appConfig.SourceFileName);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Incorrect input, please try again");
                        break;
                }
                Console.WriteLine("Type [Y] when you are ready.");

                if (string.Equals(Console.ReadLine(), "Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    for (int index = appConfig.SecondsBeforeStart; index >= 0; --index)
                    {
                        Console.WriteLine(index);
                        await Task.Delay(1000);
                    }
                }
                DoTyping(lines);
            }
        }
    }
}
