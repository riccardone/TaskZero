# TaskZero
A fully working sample C# .Net Core Console App using Event Sourcing, CQRS patterns and Event Store for the storage of events. The app manage TO-DO lists keeping the history of the events and showing how to separate write and read models.  
The synchroniser keep up to date the current state in a cache object used as read model. It subscribe events using the by_category projection. No external dependencies in the core domain.  
  
The adapter, the domain, the synchroniser are separate parts all hosted in the same single process. That is for simplicity and also showing how you can do Event Sourcing + CQRS without a Distributed Architecture.  
A version with all these parts split in separate Microservices working toghether will follow soon.  
  
# Run  
You can build and run the app with the following command:  
> dotnet run TaskZero.dll <optional-youeventstorelink-default-localhost>  
  
To start an Event Store instance, download the binaries from https://eventstore.org/downloads/ and run it starting default projections:  
  
Example on Windows:  
> EventStore.ClusterNode.exe --start-standard-projections  
  
Linux  
> mono EventStore.ClusterNode.exe --start-standard-projections  
  
# References  
This project's patterns are discussed in the following blog's articles  
http://www.dinuzzo.co.uk/2019/01/12/event-sourcing-step-by-step/  
http://www.dinuzzo.co.uk/2017/04/28/domain-driven-design-event-sourcing-and-micro-services-explained-for-developers/  
http://www.dinuzzo.co.uk/2015/08/21/ddd-event-sourcing-commands-and-events-handlers-responsabilities/
