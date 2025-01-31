/*
1 Data inlezen m.b.v. Vicon Nexus
	> TODO: User interface

Bouwblok-1.1 Inlezen van ongelabelde markers uit Vicon datastream m.b.v. Nexus. (Timestamp, Markerpositie x,y,z) 
	> Geimplementeerd
	> Zie ook: Unlabeled::id
	> NOTE: Onbruikbaar, doordat je niet (zeker) weet of de marker van de vorige frame dezelfde is als de huidige frame.

Bouwblok-1.2 Inlezen van Locklab gerelateerde informatie uit Vicon datastream m.b.v. Nexus.
	> Geimplementeerd

Bouwblok-1.3 Inlezen van gelabelde losse markers uit Vicon datastream m.b.v. Nexus. (Timestamp, Markernaam, Markerpositie x,y,z)
	> Geimplementeerd
	> Zie ook: Reference
	> NOTE: De label moet binnen een subject plaatsvinden
	
Bouwblok-1.4 Inlezen van gelabelde segmenten uit Vicon datastream m.b.v. Nexus. (Timestamp, Segmentnaam, Segmentrotatie)
	> Geimplementeerd
	> NOTE: Segmenten in vicon geven altijd 0 waarden
	> Zie ook: Reference



2 Data inlezen m.b.v. Vicon Tracker
	> N.v.T.



3 File I/O
Bouwblok-3.1	Realtime opslaan van ruwe data (In afgesproken formaat).
	> Ruwe data wordt opgeslagen
	> TODO: Test parser maken

Bouwblok-3.2	Data bufferen en na afloop van meting opslaan.
	> TODO: User interface

Bouwblok-3.3 	Inlezen van variabelen uit een configuratiefile.
	> Geimplementeerd
	> Zie ook: App

Bouwblok-3.4 	Variabelen wegschrijven naar een configuratiefile.
	> Geimplementeerd
	> Zie ook App



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


