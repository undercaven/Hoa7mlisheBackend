using System.Collections;
using System.ComponentModel.Design;

public class AppServiceContainer : IServiceContainer
{
    private SortedList localServices;
    private SortedList localServiceTypes;

    public AppServiceContainer()
    {
        localServices = new SortedList();
        localServiceTypes = new SortedList();
    }

    // IServiceProvider.GetService implementation for a linked 
    // service container architecture.
    public object GetService(System.Type serviceType)
    {
        object serviceInstance = localServices[serviceType.FullName];
        if (serviceInstance == null)
        {
            return null;
        }
        else if (serviceInstance.GetType() == typeof(ServiceCreatorCallback))
        {
            // If service instance is a ServiceCreatorCallback, invoke 
            // it to create the service.
            return ((ServiceCreatorCallback)serviceInstance)(this, serviceType);
        }
        return serviceInstance;
    }

    // IServiceContainer.AddService implementation for a linked 
    // service container architecture.
    public void AddService(System.Type serviceType,
        System.ComponentModel.Design.ServiceCreatorCallback callback, bool promote)
    {
        localServiceTypes[serviceType.FullName] = serviceType;
        localServices[serviceType.FullName] = callback;
    }

    // IServiceContainer.AddService implementation for a linked 
    // service container architecture.
    public void AddService(System.Type serviceType,
        System.ComponentModel.Design.ServiceCreatorCallback callback)
    {
        AddService(serviceType, callback, true);
    }

    // IServiceContainer.AddService implementation for a linked 
    // service container architecture.
    public void AddService(System.Type serviceType,
        object serviceInstance, bool promote)
    {
        localServiceTypes[serviceType.FullName] = serviceType;
        localServices[serviceType.FullName] = serviceInstance;
    }

    // IServiceContainer.AddService (defaults to promote service addition).
    public void AddService(System.Type serviceType, object serviceInstance)
    {
        AddService(serviceType, serviceInstance, true);
    }

    // IServiceContainer.RemoveService implementation for a linked 
    // service container architecture.
    public void RemoveService(System.Type serviceType, bool promote)
    {
        localServices.Remove(serviceType.FullName);
        localServiceTypes.Remove(serviceType.FullName);
    }

    // IServiceContainer.RemoveService (defaults to promote 
    // service removal)
    public void RemoveService(System.Type serviceType)
    {
        RemoveService(serviceType, true);
    }
}