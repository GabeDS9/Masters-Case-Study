using System;

namespace DT_Services
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceGateway myGateway = new ServiceGateway();
            myGateway.StartGateway();
        }
    }
}
