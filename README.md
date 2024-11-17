# Programming-With-Sockets-Gr.10

# Përshkrimi
Ky është një projekt për një sistem të thjeshtë komunikimi midis një serveri dhe klientëve duke përdorur protokollin UDP dhe gjuhën programuese C#. Projekti përbëhet nga një server dhe disa klientë që kanë mundësi të dërgojnë dhe të marrin mesazhe, si dhe të lexojnë, shkruajnë dhe ekzekutojnë komanda në server.

# Serveri
# Funksionalitete:
 1.	Variablat e IP adresës dhe portit:
  -	IP adresa reale dhe numri i portit të vendosen në kod.
 2.	Dëgjimi i kërkesave nga anëtarët e grupit:
  -	Serveri mund të dëgjojë të gjitha kërkesat nga anëtarët e grupit.
 3.	Pranimi i kërkesave nga pajisjet:
  -	Serveri pranon kërkesa nga klientët dhe i përpunon ato.
 4.	Leximi i mesazheve nga klientët:
  -	Serveri lexon dhe përpunon mesazhet e dërguara nga klientët.
 5.	Qasje e plotë për një klient:
  -	Serveri mund të japë qasje të plotë për një klient për të menaxhuar fajllat dhe folderat.

# Klienti
# Funksionalitete:
 1.	Krijimi i socket lidhjes me serverin:
  -	Klienti krijon një lidhje socket me serverin.
 2.	Privilegjet për pajisjet (klientët):
  -	Njëri nga klientët ka privilegje për të shkruar (write), lexuar (read) dhe ekzekutuar (execute) komanda.
  -	Klientët tjerë kanë vetëm privilegje leximi (read).
 3.	Lidhja me serverin:
  -	Klienti lidhet me serverin duke përcaktuar portin dhe IP adresën e serverit.
 4.	Leximi i përgjigjeve nga serveri:
  -	Klienti është në gjendje të lexojë përgjigjet që i kthehen nga serveri.
 5.	Dërgimi i mesazheve serverit:
  -	Klienti dërgon mesazhe tek serveri në formë teksti.
 6.	Qasje e plotë në folderat dhe përmbajtjen e serverit:
  -	Klienti me privilegje të plota ka qasje për të lexuar, shkruar dhe ekzekutuar komanda në server.

# Kërkesat për Projektin
# 1.	Serveri:
  -	Vendosja e variablave për numrin e portit dhe IP adresën reale.
  -	Dëgjimi i kërkesave nga të gjithë anëtarët e grupit.
  -	Pranimi i kërkesave nga pajisjet që dërgojnë kërkesa.
  -	Leximi i mesazheve që dërgohen nga klientët.
  -	Dhënia e qasjes së plotë të paktën njërit klient për qasje në folderat/përmbajtjen në file-t në server.
# 2.	Klienti:
  -	Krijimi i socket lidhjes me serverin.
  -	Një nga klientët të ketë privilegjet për të shkruar (write), lexuar (read) dhe ekzekutuar (execute).
  -	Klientët tjerë të kenë vetëm privilegje leximi (read).
  -	Lidhja me serverin duke përcaktuar portin dhe IP adresën.
  - Definimi i saktë i socket-it të serverit dhe lidhja të mos dështojë.
  -	Leximi i përgjigjeve që i kthehen nga serveri.
  -	Dërgimi i mesazheve serverit në formë teksti.
  -	Qasje e plotë në folderat/përmbajtjen në server

# Teknologjitë e Përdorura
 •	Protokolli: UDP (User Datagram Protocol)
 
 •	Gjuha e Programimit: C#

# Si të Ekzekutoni Projektin
 1.	Klononi repository-n nga GitHub.
 2.	Hapni projektin në një IDE si Visual Studio.
 3.	Kompiloni dhe ekzekutoni server.cs për të nisur serverin.
 4.	Kompiloni dhe ekzekutoni client.cs për të nisur klientin.
 5.	Ndiqni udhëzimet në konsolë për të krijuar lidhjen dhe për të dërguar kërkesa.

# Struktura e Projektit
├── server.cs   # Kodi për serverin që menaxhon kërkesat e klientëve

├── client.cs   # Kodi për klientin që ndërvepron me serverin

├── README.md   # Dokumentimi i projektit


# Klonimi i Projektit
Klononi këtë depo duke përdorur komandën më poshtë:

git clone https://github.com/AnilaLuta/Programming-With-Sockets-Gr.10/

# Punoi
• Anila Luta

• Albiona Mustafa

• Hare Luma





    




