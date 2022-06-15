using System;
using System.Collections.Generic;
using System.Threading;
using Services;

class Bootstrap
{
    static ServiceGateway serviceGateway = new ServiceGateway();
    static DirectoryService directoryService = new DirectoryService();
    static ExploratoryAnalyticsService exploratoryService = new ExploratoryAnalyticsService();
    static EnergyCostService energyCost = new EnergyCostService();
    public static void Main(string[] args)
    {
        Thread directoryThread = new Thread(directoryService.InitialiseDirectoryServiceAsync);
        Thread exploratoryThread = new Thread(exploratoryService.InitialiseExploratoryAnalyticsService);
        Thread energyCostThread = new Thread(energyCost.InitialiseEnergyCostService);

        directoryThread.Start();
        exploratoryThread.Start();
        energyCostThread.Start();

        serviceGateway.InitialiseServiceGateway();        
    }
}
