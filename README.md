# Turnierleitung Ansage App

## Zweck
übersichtliche Darstellung der kommenden Spiele auf Grundlage der Tournify Anzeige "upcoming matches"

Über den ```TournifyConnector``` werden Daten des Präsentationsmodus von Tournify ausgelesen und in ein MatchesModel umgewandelt.

Diese aktuellen/demnächst anstehenden Partien werden übersichtlich dargestellt, so dass eine einfach Ankündigung durch die Turnierleitung erfolgen kann.

Außerdem ist es möglich, den Anpiff/Abpfiff und diverse Ankündigungen ("noch eine Minute zu spielen", "noch eine Minute bis zum nächsten Spiel" und "Um 10:30 Uhr spielt Team A gegen Team auf dem Platz X.") automatisiert ausgeben zu lassne.

## Konfiguration
```
/Properties/Settings.settings
```

TournifyUrl => https://www.tournify.de/live/kiwicup/present/5
Verweist die URL, wo sich der 

```
/App/Connectors/TournifyConnector.cs
```
die Daten zu direkt folgenden Spiele abholt

