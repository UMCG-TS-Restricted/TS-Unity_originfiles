/*
1 Data inlezen m.b.v. Vicon Nexus
Bouwblok-1.1 Inlezen van ongelabelde markers uit Vicon datastream m.b.v. Nexus. (Timestamp, Markerpositie x,y,z) 
Bouwblok-1.2 Inlezen van Locklab gerelateerde informatie uit Vicon datastream m.b.v. Nexus.
Bouwblok-1.3 Inlezen van gelabelde losse markers uit Vicon datastream m.b.v. Nexus. (Timestamp, Markernaam, Markerpositie x,y,z)
Bouwblok-1.4 Inlezen van gelabelde segmenten uit Vicon datastream m.b.v. Nexus. (Timestamp, Segmentnaam, Segmentrotatie)


2 Data inlezen m.b.v. Vicon Tracker
> N.v.T.


3 File I/O
Bouwblok-3.1	Realtime opslaan van ruwe data (In afgesproken formaat).
Bouwblok-3.2	Data bufferen en na afloop van meting opslaan.
Bouwblok-3.3 	Inlezen van variabelen uit een configuratiefile.
Bouwblok-3.4 	Variabelen wegschrijven naar een configuratiefile.


4 Bewerken
Bouwblok-4.1	Realtime bewerken van ruwe data.
Bouwblok-4.2	Data bufferen en na afloop van meting bewerken.
Bouwblok-4.3	Variabelen bewerken.


5 Displayen
Bouwblok-5.1	Realtime data weergeven op het Unity Display
Bouwblok-5.2	Data bufferen en na afloop van meting weergeven op het Unity Display.
Bouwblok-5.3	Realtime data weergeven op het Head Mounted Display
Bouwblok-5.4	Data bufferen en na afloop van meting weergeven op het Head Mounted Display.
Bouwblok-5.5	Variabelen weergeven op het Unity Display
Bouwblok-5.6	Avatar realtime weergeven op Head Mounted Display.
 */



/**
 * TODO:
 * - HMD callibration
 * - Replay in game maken
 * - Game mooi maken
 * - Player met verscheidene plugins maken
 * 
 */



/*
 * De software is in lagen opgebouwd.
 * 
 * 0. Platform: (intern) dit bevat niet specifieke functionaliteiten,
 * maar maakt de implementatie eenvoudiger voor de lagen hierna.
 * 
 * 1. Driver: (intern) De laag die de rauwe data binnen haalt en prepareerd.
 * Daarnaast is het ook hetgeen wat de serialisatie uitvoert.
 * Hierin komen bouwblokker 1, 2 en 3.
 * 
 * 2. Brug: De laag die de rauwe data bruikbaar maakt voor Unity en
 * deze in "components" plaatst. Dit is niet specifiek voor een bouwblok,
 * maar het doel is het gebruik van de bouwblokken menselijk te maken.
 * 
 * 2a. Displayen: Dit is een los systeem en is direct relateerbaar aan
 * de bouwblokken 5.1, 5.3, 5.5 en 5.6. Hier zouden geen afhankelijkheden
 * op gemaakt moeten worden om de structuur zo plat mogelijk te maken.
 * 
 * 2b: Bewerken: Dit zijn losse extensies op bug componenten die de uitvoer
 * van de brug aanpassen, en voor de volgende laag als invoer is.
 * 
 * 3: Game: Gebruike alle bovenstaande lagen. En is verantwoordelijk voor
 * de serialisatie en deserialisatie.
 * 
 * 
 */
