using System;

namespace RedisClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (dynamic redisClient = new RedisClient("localhost", 6379))
            {
                dynamic result = redisClient.set("foo", "bar");
                if (result != null)
                {
                    Console.WriteLine("Response from server: " + result);

                    dynamic o = redisClient.get("foo");
                    Console.WriteLine("Response from server: " + o);
                }
            }

            Console.ReadLine();
        }
    }
}
