# Yet Another League Statistics Tracker

YALST is a Statistics Tracker for League of Legends that utilises Riot Games' API to retrieve data about a given user. 

Currently, the project is on Version 2 with the original being a C# Blazor project. The original project can be found on the 
[main branch](https://github.com/Portablefire22/YALST/tree/master), and a ramble about the change can be found at 
[Kitten.rs](https://kitten.rs/blog/Yalst).

## Back-end

The back-end uses C# ASP.NET that is accessible through a public API. Accessing Riot Games' API is performed through a rate limit supporting service, which interacts with an external MariaDB database to cache results and reduce API calls. The back-end supports queueing actions to allow for large API tasks to be fragmented and slowly progressed through, auto-updating the page for the user when finished.

## Front-end

The front-end uses the Angular framework to provide a dynamic client-side rendered web-page. The front-end additionally pulls from Riot Games' and CommunityDragon's CDN to provide 
up-to-date information to the client without relying on local updating.
