# Turnierleitung Ansage App

## Zweck
�bersichtliche Darstellung der kommenden Spiele auf Grundlage der Tournify Anzeige "upcoming matches"

�ber den ```TournifyConnector``` werden Daten des Pr�sentationsmodus von Tournify ausgelesen und in ein MatchesModel umgewandelt.

Diese aktuellen/demn�chst anstehenden Partien werden �bersichtlich dargestellt, so dass eine einfach Ank�ndigung durch die Turnierleitung erfolgen kann.

Au�erdem ist es m�glich, den Anpiff/Abpfiff und diverse Ank�ndigungen ("noch eine Minute zu spielen", "noch eine Minute bis zum n�chsten Spiel" und "Um 10:30 Uhr spielt Team A gegen Team auf dem Platz X.") automatisiert ausgeben zu lassne.

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

