
# Test Hash App

This solution contains three parts:
- Hash.API
- Hash.Data
- Hash.Processor

Hash.API represents RestAPI in .Net Core 8. Contains two endpoints:
- GET /hashes
- POST /hashes

Hash.Data contains the business logic of the app. Database is code first approach with entity framework. It's already has the needed migrations and they are auto executed on startup on both apps (API and Processor).

Hash.Processor is console app in .Net Core 8, run as hosted background app. Re-uses the DI implementation of Service and Extensions in Hash.Data to take advantage of already written code. 

RabbitMQ is configured with following props:
- host: localhost
- username: guest
- password: guest
- queue name: HashQueue
