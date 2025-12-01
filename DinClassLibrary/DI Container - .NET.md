# DI Container \- .NET

Je bouwt een DI Container die gelijkaardig is aan Microsoft.Extensions.DependencyInjection.

De NuGet package en namespace ‘Microsoft.Extensions.DependencyInjection’ is slechts een richtlijn. Je hoeft *niet* dezelfde namen te gebruiken (van klassen, methodes, ...) en mag bvb. ook het bootstrapping-mechanisme op een andere manier in werking laten treden.  
Daarnaast hoef je niet de volledige featureset te bouwen, maar wel de opgesomde features.

# Opdracht

 - [ ] Je ondersteunt assembly scanning voor bepaalde sets van klassen, zoals bvb. controllers (Web) of hubs (SignalR). Je kan de scanning baseren op een annotatie, een superklasse of een klassenaam prefix/suffix.  
 - [ ] Je ondersteunt het toevoegen van singletons aan de container, met ondersteuning voor het optioneel specifiëren van een interface. (bijv. AddSingleton)  
 - [ ] Je ondersteunt constructor injection waarbij de voorkeur gegeven wordt aan de niet-default constructor. Je geeft een foutmelding als er meerdere kandidaat-constructors zijn.  
   - [ ] Je onderteunt constructors met parameters.  
   - [ ] Klassen moeten meer dan één afhankelijkheid kunnen hebben.  
 - [ ] Je ondersteunt controllers (zie eerste puntje). Dit kan een controller zijn die HTTP requests afhandelt (bijv. ‘Controller’). Je kan er ook voor kiezen om een soort controller te bouwen die keyboard input afhandelt i.p.v. HTTP requests. Je controller hoeft dus geen HTTP/Web controller te zijn, maar het mag wel.  
   - [ ] Je ondersteunt acties op deze controllers (bijv. \[HttpGet\], \[KeyboardInput\], \[KeyPress\], …).  
   - [ ] Je kan bijvoorbeeld text van standard input (Console.In) mappen naar *actionmethods*. Een console applicatie is dus voldoende.  
 - [ ] Je ondersteunt *dynamic interception*, een [aspect-oriented programming](https://en.wikipedia.org/wiki/Aspect-oriented_programming) techniek om ‘decorators’ toe te voegen *at runtime*. Ook hier hoef je het niet te ver te zoeken (\[Logged\]: entry/exit van de methode wordt gelogd op standard output, \[Timed\]: doorlooptijd van de methode wordt gelogd op standard output, ...).  
 - [ ] Je legt een dependency graph aan en detecteert cyclische dependencies.  
 - [ ] Je gebruikt een logging framework en doet voldoende logging op verschillende log-levels. Het moet mogelijk zijn om, mits aanpassing van het logging niveau, te kunnen zien welke objecten wanneer aangemaakt worden.  
 - [ ] Je bouwt een performante oplossing: nadat je demo-applicatie is opgestart zou reflection tot een minimum beperkt moeten worden\!  
 - [ ] Je voorziet een demo-applicatie waarmee je de volledige werking kan laten zien. o.a.:  
   - [ ] Meerdere services, controllers, … met o.a. transitieve (onrechtstreekse) dependencies.  
   - [ ] Verschillende acties leiden ertoe dat verschillende controllers en hun methodes aangesproken worden.  
   - [ ] Interception van een geannoteerde services.  
 - [ ] Je zorgt voor een mooie en onderhoudbare architectuur. SRP: elke klasse heeft één verantwoordelijkheid.  
 - [ ] Je voorziet volgende unit tests:  
   - [ ] Tests die aantonen dat services door je framework aangemaakt worden.  
   - [ ] Tests die aantonen dat cyclische dependencies gedetecteerd worden.  
   - [ ] Tests die aantonen dat dubbelzinnigheden gedetecteerd worden (services die niet aangemaakt kunnen worden).  
   - [ ] Tests die aantonen dat je interception-mechanisme werkt.  
 - [ ] Je levert één solution op met drie projecten: het framework, de demo-applicatie en de tests.  
 - [ ] In je README.md noteer je \- in correct [Markdown-formaat](https://en.wikipedia.org/wiki/Markdown) \- het volgende:  
   - [ ] Jouw naam en de naam van je framework.  
   - [ ] Hoe je framework gebruikt moet worden, a.d.h.v. code-voorbeelden.  
   - [ ] Met welke commando’s:  
     - [ ] … je de code kan compileren.  
     - [ ] … je de demo-applicatie kan uitvoeren.  
     - [ ] … je de tests kan uitvoeren.

# Niet vereist

Voor de duidelijkheid:

- Je hoeft geen ‘scopes’ of transient te implementeren (AddScoped, AddTransient).  
- Je hoeft geen HTTP/Web server framework te bouwen. Je kan bvb. de functionaliteit demonstreren via een console applicatie.