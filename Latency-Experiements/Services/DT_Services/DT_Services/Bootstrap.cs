using System;
using System.Collections.Generic;
using System.Threading;
using Services;

class Bootstrap
{
    static ServiceGateway serviceGateway = new ServiceGateway();
    static DirectoryService directoryService = new DirectoryService();
    static ExploratoryAnalyticsService exploratoryService = new ExploratoryAnalyticsService();

    private List<Service> servicesList = new List<Service>();
    private static LoadExcel excel = new LoadExcel();
    public static void Main(string[] args)
    {
        Thread directoryThread = new Thread(directoryService.InitialiseDirectoryServiceAsync);
        Thread exploratoryThread = new Thread(exploratoryService.InitialiseExploratoryAnalyticsService);

        directoryThread.Start();
        exploratoryThread.Start();

        serviceGateway.InitialiseServiceGateway();        
    }
}
