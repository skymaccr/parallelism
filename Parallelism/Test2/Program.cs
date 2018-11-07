using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfThreads = 10;
            int numberOfIterations = 10000;
            int startOfRange = 80;

            List<int> numbers = new List<int>();
            numbers = Enumerable.Range(startOfRange, 7).ToList();

            Console.WriteLine($"Parallel processing has started...");

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            ParallelProcessor<int> pp = new ParallelProcessor<int>(numberOfThreads, FindPrimeNumber);

            for (int j = 0; j < numberOfIterations; j++)
            {
                pp.ForEach(numbers);
            }

            stopwatch.Stop();
            Console.WriteLine($"Job's (custom) done over {stopwatch.ElapsedMilliseconds} ms");

            Console.WriteLine($"Process started: Built-in ForEach");

            stopwatch.Reset();
            stopwatch.Start();

            for (int j = 0; j < numberOfIterations; j++)
            {
                Parallel.ForEach(numbers, (currentNumber) =>
                {
                    FindPrimeNumber(currentNumber);
                });
            }

            Console.WriteLine($"Job's (built-in) done over {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Process started: sequential");
            stopwatch.Reset();
            stopwatch.Start();

            for (int j = 0; j < numberOfIterations; j++)
            {
                for (int i = 0; i < numbers.Count; i++)
                {
                    FindPrimeNumber(numbers[i]);
                }
            }

            Console.WriteLine($"Job's (sequential) done over {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadKey();
        }

        static void FindPrimeNumber(int n)
        {
            int count = 0;
            long a = 2;
            while (count < n)
            {
                long b = 2;
                int prime = 1;
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
        }
    }
}
