# TINProject

Wymagane oprogramowania do odpalenia: 
- docker
- npm (Node.js)
- .NET Core

## Jak to odpalić?(wykonywać w odpowiedniej kolejności )

### 1 Przygotowania:

1.1 Wejście do katalogu Lab3ChatKlient oraz odpalenie terminama 
1.2 ```npm install``` w celu instalacji wymaganych paczek(spis package.json)
1.3 Wejście do katalogu Lab3DB
1.4 Odpalenie skryptu init.bat (tworzy on folder documents który będzie montowany do kontenera w celu przechowywania bazy danych oraz buduje obraz kontenera)


### 2 Uruchamianie bazy danych:

2.1 Wejście do katalogu Lab3DB
2.2 Odpalenie skryptu start.bat (uruchamia on bazę danyc oraz daje dostęp do konsoli kontenera z bazą, NIE ZAMYKAJ OKNA TERMINALA DO ZAKONCZENIA PRACY!!!)

### 3 Uruchamianie servera: 

3.1 Odpalenie projektu VS studio (czatSerwerTIN). Tu liczę na własną kreatywność 

### 4 Uruchomienie klienta: 

4.1 Wjście do katalogu Lab3ChatKlient oraz odpalenie terminama
4.2 ```npx gulp``` odpala aplikacje klienta 
