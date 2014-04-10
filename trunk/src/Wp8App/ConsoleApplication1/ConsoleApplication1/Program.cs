using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 1; i < 1000; i++)
            {
                var j = i;
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    while (true)
                    {

                        Thread.Sleep(1000);
                        Console.WriteLine("Thread #" + j);
                    }
                }));
            }
            Console.ReadLine();
        }
    }
}
