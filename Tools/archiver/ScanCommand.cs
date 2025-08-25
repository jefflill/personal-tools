// -----------------------------------------------------------------------------
// FILE:        ScanCommand.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2023 by Jeff Lill
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

using Neon.Common;

namespace archiver
{
    /// <summary>
    /// Implements the <b>scan ...</b> command.
    /// </summary>
    public class ScanCommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScanCommand()
        {
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="commandLine">Specifies the command line.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task<int> ExecuteAsync(CommandLine commandLine)
        {
            var folder       = commandLine.Arguments[0];
            var lastSlashPos = folder.LastIndexOf('\\');
            var folderName   = folder.Substring(lastSlashPos + 1);
            var mp4Files     = Directory.GetFiles(folder, "*.mp4", SearchOption.AllDirectories);
            var logPath      = Path.Combine("C:\\Temp", $"ffmpeg-{folderName}.log");
            var logFile      = new StreamWriter(logPath);
            var files        = 0;
            var badFiles     = new List<string>();

            foreach (var mp4File in mp4Files)
            {
                Console.WriteLine($"file: {mp4File}");
                logFile.WriteLine($"file: {mp4File}");
                logFile.Flush();

                var response = await NeonHelper.ExecuteCaptureAsync("ffmpeg.exe", new object[] { "-v", "error", "-i", mp4File, "-f", "null", "-" });

                files++;

                if (response.ExitCode != 0)
                {
                    badFiles.Add(mp4File);
                    Console.WriteLine($"*** ERROR: ****************");
                    Console.WriteLine($"{response.ErrorText.Trim()}");
                    Console.WriteLine($"***************************");

                    Console.WriteLine($"*** ERROR: ****************");
                    logFile.WriteLine($"{response.ErrorText.Trim()}");
                    logFile.WriteLine($"***************************");
                }

                logFile.Flush();
            }

            Console.WriteLine("***********************************************************");
            Console.WriteLine($"Files Processed: {files}");
            Console.WriteLine($"Bad Files:       {badFiles.Count}");
            Console.WriteLine("***********************************************************");

            logFile.WriteLine("***********************************************************");
            logFile.WriteLine($"Files Processed: {files}");
            logFile.WriteLine($"Bad Files:       {badFiles.Count}");
            logFile.WriteLine("***********************************************************");

            if (badFiles.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("BAD FILES **************************************************");

                logFile.WriteLine();
                logFile.WriteLine("BAD FILES **************************************************");

                foreach (var badFile in badFiles)
                {
                    Console.WriteLine(badFile);
                    logFile.WriteLine(badFile);
                }

                Console.WriteLine("************************************************************");
                logFile.WriteLine("************************************************************");
            }

            logFile.Flush();

            Console.WriteLine();
            Console.WriteLine($"*** Log file: {logPath}");
            Console.WriteLine();

            return badFiles.Count == 0 ? 0 : 1;
        }
    }
}
