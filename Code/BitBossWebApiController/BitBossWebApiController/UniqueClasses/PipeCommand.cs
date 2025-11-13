using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitBossWebApiController
{
    public class PipeCommand {
        public static object mylock = new object();
        static NamedPipeClientStream pipeStream = null;
        static StreamReader sr = null;
        static StreamWriter sw = null;
        static int waiting = 0;
        public static object Command(string command)
        {
            long start = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            object ret = null;

            try {
                waiting++;
                // Console.WriteLine($"Command start {command} {waiting}");
                lock(mylock) {
                    if (pipeStream == null) {
                        Console.WriteLine($"new NamedPipeClientStream {pipeStream}");
                        pipeStream = new NamedPipeClientStream(
                                ".", "Abc1",
                                PipeDirection.InOut,
                                PipeOptions.Asynchronous
                            ); // InOut
                        sr = new StreamReader(pipeStream);
                        sw = new StreamWriter(pipeStream);
                        Console.WriteLine($"pipeStream.Connect()");
                        try {
                            pipeStream.Connect(200);
                        } catch (Exception e) {
                            pipeStream = null;
                            return new { error = $"controller is down" };
                        }
                        sw.AutoFlush = true;
                    }
                    // Console.WriteLine($"sw.WriteLine");
                    sw.WriteLine(command);
                    // pipeStream.WaitForPipeDrain();
                    // Console.WriteLine($"sr.ReadLine");
                    string json = sr.ReadLine();
                    ret = System.Text.Json.JsonSerializer.Deserialize<object>(json);
                    return ret;
                }
            } catch (System.IO.IOException e) {
                pipeStream = null;
                Console.WriteLine(e);
                return new { error = $"{e}" };
            } catch (Exception e) {
                Console.WriteLine(e);
                return new { error = $"{e}" };
            } finally {
                long now = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                Console.WriteLine($"releasing mylock: {now - start}");
                waiting--;
            }
        }
    }
}
