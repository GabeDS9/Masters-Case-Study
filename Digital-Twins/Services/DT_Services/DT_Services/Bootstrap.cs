using System;

class Bootstrap
{
    static Services.ServiceGateway myServices = new Services.ServiceGateway();
    public static void Main(string[] args)
    {
        myServices.InitialiseServices();
    }
}
