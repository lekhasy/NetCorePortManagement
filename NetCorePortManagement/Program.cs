using System;
using System.Net.Sockets;
using System.Threading;

namespace PortManagement
{
    class MainClass
    {
        static readonly string serverHost = "151.101.65.69";
        static readonly int numberOfConcurrentConnections = 3000;
        static readonly int maxopenSocketCall = 60000;
        static int openSocketCallCount;
        public static readonly Semaphore smp = new Semaphore(numberOfConcurrentConnections, numberOfConcurrentConnections);

        public static void Main()
        {
            ThreadPool.SetMaxThreads(numberOfConcurrentConnections, numberOfConcurrentConnections);
            ThreadPool.SetMinThreads(numberOfConcurrentConnections, numberOfConcurrentConnections);

            for (int i = 0; i < maxopenSocketCall; i++)
            {
                OpenConnectionAndDoStuff();
            }

            Console.ReadLine();
        }

        static void OpenConnectionAndDoStuff()
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    smp.WaitOne();
                    using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        sock.Connect(serverHost, 80);
                    }
                    IncreaseOpenSocketCallCountAndPrint();
                }
                catch(Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    smp.Release();
                }
            });
        }

        static void SimulateOpenConnectionAndDoStuff()
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    smp.WaitOne();
                    IncreaseOpenSocketCallCountAndPrint();
                    Thread.Sleep(50);
                }
                finally
                {
                    smp.Release();
                }
            });
        }

        static readonly Object lockObj = new Object();
        static void IncreaseOpenSocketCallCountAndPrint()
        {
            lock (lockObj)
            {
                openSocketCallCount++;
                Console.WriteLine(openSocketCallCount);
            }
        }
    }
}
