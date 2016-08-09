Bouvet Battle Royale 2015
=========================
This is a code-for-fun concept implemented for a code camp battle between the Java and Microsoft developers in Bouvet - a norwegian IT-consultancy company.

Here is a rather long blog post (in norwegian) covering most details of the 2015 event:
https://utbrudd.bouvet.no/2015/11/30/bouvet-battle-royale-rematch/

## Concept ###
Bouvet Battle Royale is a geo-based game with a few concepts:
- Players are organised in teams
- Teams are engaged in a match
- The players are looking for, and registers, control points

A control point has
- a coordinate (latitude, longitude)
- a score value (can decrease as more teams register it)
- a visibility that can be time-based (visible from-to)

Each team get a status feed with
- A list of visible controls 
- If a control point is registered (with score)
- The team score
- The team rank
- Weapon holdings (yes, there are weapons)

Weapons
- Bomb: hides the control point a certain time
- Trap: blows up on the next team that tries to register the control, yielding minus the control point value - and hides the control point for a while.
- Weapons are used when registering a control point

## Implementation Challenge ##
Originally, the teams were meant to implement their own clients - using the HTTP API. However, the solution contains sample client that can get non-developer teams into the match.

## Technology ##
- The data storage is SQL Server - it runs on Azure SQL as well as LocalDb on your developer laptop.
- Data persistence is handled by Entity Framework 6 with Code First and Migrations
- The API service is run on ASP.NET WebApi, and documented with Swashbuckle

# Getting started with the code #
- Clone the source code from here
- Build the solution (it will restore all NuGet packages)
- Run the unit test project
