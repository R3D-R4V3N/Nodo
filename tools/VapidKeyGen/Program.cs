using System;
using WebPush;

class Program
{
    static void Main(string[] args)
    {
        var keys = VapidHelper.GenerateVapidKeys();

        Console.WriteLine("VAPID keys gegenereerd:");
        Console.WriteLine($"Subject : mailto:admin@jouwdomein.be");
        Console.WriteLine($"Public  : {keys.PublicKey}");
        Console.WriteLine($"Private : {keys.PrivateKey}");
    }
}