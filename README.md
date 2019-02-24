# TaskZero
A fully working sample C# .Net Core Console App using Event Sourcing, CQRS patterns and Event Store for the storage of events. The app manage TO-DO lists keeping the history of the events and showing how to separate write and read models.  
The synchroniser keep up to date the current state in a cache object used as read model. It subscribe events using the by_category projection. No external dependencies in the core domain.  
  
In the src/SingleProcess folder, the adapter, the domain, the synchroniser are separate parts all hosted in the same single process. That is based on requirement. It can be of some help to show how you can do Event Sourcing + CQRS without a Distributed Architecture.  
In the src/Microservices folder, there is a version with all these parts split in separate Microservices working toghether [work in progress]. 
  
# References  
This project's patterns are discussed in the following blog's articles  
http://www.dinuzzo.co.uk/2019/01/12/event-sourcing-step-by-step/  
http://www.dinuzzo.co.uk/2017/04/28/domain-driven-design-event-sourcing-and-micro-services-explained-for-developers/  
http://www.dinuzzo.co.uk/2015/08/21/ddd-event-sourcing-commands-and-events-handlers-responsabilities/
