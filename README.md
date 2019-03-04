# TaskZero
A fully working sample C# .Net Core Console App using Event Sourcing, CQRS patterns and Event Store for the storage of events. The app manage TO-DO lists keeping the history of the events and showing how to separate write and read models.  
The synchroniser keep up to date the current state in 2 different read-models: an inmemory cache object to show results in the app UI and an Elastic Search index with all the denormalised tasks. No external dependencies in the core domain.  
  
The adapter, the domain, the synchronisers are separate parts all hosted in the same single process at run time. That is based on requirement. It shows how you can do Event Sourcing + CQRS without a Distributed Architecture using separate Microservices. I will create in a separate repository a version with all these parts split in separate Microservices working toghether.  
   
# Run  
Example running without ElasticSearch with EventStore on local host:  
> dotnet TaskZero.dll  
  
Example running with EventStore and ElasticSearch:  
> dotnet TaskZero.dll tcp://localhost:1113 http://localhost:9200  
  
To start an Event Store instance, download the binaries from https://eventstore.org/downloads/ and run it 
  
Example on Windows:  
> EventStore.ClusterNode.exe 
  
Linux  
> mono EventStore.ClusterNode.exe  
  
To start an Elastic Search instance download it from https://www.elastic.co/downloads/elasticsearch and follow the instruction depending on your OS. Elastic Search is present only as optional read model. You can run the app without it to see the streams of events in Event Store and the current state in the in memory cache read model.  
  
# References  
This project's patterns are discussed in the following blog's articles  
http://www.dinuzzo.co.uk/2019/02/27/distributed-architecture-3-steps/  
http://www.dinuzzo.co.uk/2017/04/28/domain-driven-design-event-sourcing-and-micro-services-explained-for-developers/  
http://www.dinuzzo.co.uk/2015/08/21/ddd-event-sourcing-commands-and-events-handlers-responsabilities/
