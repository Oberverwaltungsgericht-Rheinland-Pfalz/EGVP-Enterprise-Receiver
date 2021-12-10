<img src="icon.png" align="right" />

# EGVP-Enterprise-Receiver
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)![License:EUPL-1.2](https://img.shields.io/badge/license-EUPL--1.2-blue)

Bei diesem Projekt handelt es sich um eine .NET Kommandozeilen Anwendung, mit welcher es möglich ist, Nachrichten von dem Egvp-Enterprise Webservice  (von der Firma Governikus) abzuholen und in das Dateisystem zu exportieren.

## Installation
Da es sich um eine .NET Framework-Anwendung handelt, kann die Anwendung auf Windows-Systemen (mit mind. .net Framework 4.6.1)  einfach irgendwo im Dateisystem abgelegt und gestartet werden. <br>
Bei Wiederkehrender Ausführung bietet es sich an, das Programm alle x Minuten als geplanten Task (mit Parameter -a) auszuführen.

## Konfiguration
Die Konfiguration lässt sich in der Datei "EgvpEpReceiver.exe.config" vornehmen.

### Bereich \<applicationSettings>
Hier können verschieden Grundeinstellungen vorgenommen werden:

|Setting|Beschreibung|Abhängigkeit zu|Angabe verpflichtend|
|---|---|---|---|
|configfile|Vollständiger Pfad zu einer XML-Konfigurationsdatei, welche die Postfächer enthält (siehe Konfiguration der Postfächer)|  | [X] |
|tempDir|Vollständiger Pfad eines Temporären Verzeichnisses, in welchem Nachrichten entpackt und zwischengespeichert werden|   | [X] |
|LockFile|Hier kann Optional ein Dateiname (bspw. "export.log") angegeben werden, welche während dem Kopieren der Nachrichten im Exportverzeichnis erzeugt wird. <br>Vorgesehen ist dies bspw. für eine Fachanwendung, damit diese darüber Informiert ist, dass gerade Nachrichten exportiert werden.|   |
|LogFile|Vollständiger Pfad zu einer Datei oder zu einem Verzeichnis, wohin die Logs geschrieben werden. Wird ein Verzeichnis angegeben, werden die Logs je nach Status Fatal, Error, Warning oder Info in getrennte Dateien geschrieben.| | |
|LogDbConnectionString|Hier kann zusätzlich ein Connection String zum Logging in eine Datenbank hinterlegt werden <br> bspw. "Data Source=5500S-SQL1\ERV;Initial Catalog=APPDATADEV;Integrated Security=True"|LogDbDatatable| |
|LogDbDatatable|Datenbank-Tabelle, wo die Logs reingeschrieben werden|LogDbConnectionString| |
|WaitingHoursAfterError|Hier kann eine Anzahl von Stunden hinterlegt werden. Vor einem erneutem Abholversuch wird dies Zeit abgewartet. <br>Dies kann bspw. für Datenbanklogging wichtig werden, damit nicht alle x Minuten ein Fehlerlog erzeugt wird|LogDbDatatable, LogDbConnectionString| |

### Bereich \<connectionStrings>

Hier muss bei Datenbanklogging zusätzlich noch die Verbindung zur Datenbank konfiguriert werden.

### Bereich \<client>

Hier muss die Endpunkt-Adresse des Egvp-Enterprise-Webservice hinterlegt/ersetzt werden

## Konfiguration der Postfächer
Die Konfigurationsdatei der Postfächer kann wie in "Bereich applicationSettings" beschrieben, angegeben werden. <br><br>
Die xml-Datei muss bis Version 1.x folgendes Format einhalten:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<egvpepreceiver>
	<common>
	</common>
	<postboxes>
	
		<!-- OVG RLP Testpostfach -->
		<postbox Id="DE.Justiz.231654-acdf-1324-1234-12341234.4444">
			<export>
				<path>\\dfsshare\Verzeichnis\Gericht1\ERV\Import</path>
			</export>
			<exportEeb>
				<path>\\dfsshare\Verzeichnis\Gericht1\ERV\Import_eEB</path>
			</exportEeb>
			<archiv>
				<path>\\dfsshare\Verzeichnis\Gericht1\ERV\Archiv\[yyyy]_[MM]</path>
			</archiv>
		</postbox>
    
    </postboxes>
</egvpepreceiver>
```

Achtung, ab Version 2.x muss die Datei im Json-Format angegeben werden:

```json
{
  "postboxes": [
    {
      "Id": "DE.Justiz.231654-acdf-1324-1234-12341234.4444",
      "Name": "OVG RLP Testpostfach",
      "ExportPath": [
        "\\\\dfsshare\\Verzeichnis\\Gericht1\\ERV\\Import"
      ],
      "ExportPath_EEB": [
        "\\\\dfsshare\\Verzeichnis\\Gericht1\\ERV\\Import_eEB"
      ],
      "ArchivPath": [
        "\\\\dfsshare\\Verzeichnis\\Gericht1\\ERV\\Archiv\\[yyyy]_[MM]"
      ],
      "ReceiveDepartments": {
        "LogLevelDepartmentNotFound": "Warning",
        "XPathDepartmentId": "//xjustiz:nachrichtenkopf/xjustiz:auswahl_empfaenger/xjustiz:empfaenger.gericht/code",
        "Departments": [
          {
            "Name": "Department D2700S",
            "Id": "A501558",
            "ExportPath": [
              "\\\\dfsshare\\Verzeichnis\\Gericht1\\ERV\\Import_D2700S"
            ],
            "ExportPath_EEB": [
              "\\\\dfsshare\\Verzeichnis\\Gericht1\\ERV\\Import_eEB_D2700S"
            ],
            "ArchivPath": [
              "\\\\dfsshare\\Verzeichnis\\Gericht1\\ERV\\Archiv_D2700S\\[yyyy]_[MM]"
            ]
          }
        ]
      }
    }
  ]
}
```
im Bereich "ReceiveDepartments" (optionales Attribut) können weitere Empfängergerichte anhand der xjustiz.xml hinterlegt werden. (Ein Zentrales Postfach mit Empfängercodes):
|Attribut|Beschreibung|
|---|---|
|LogLevelDepartmentNotFound|hier kann angegeben werden, wie das Programm reagiert, wenn an das Postfach Nachrichten ohne Empfängergericht oder mit einem nicht definierten Empfängergericht geschickt werden. <br> info => es wird lediglich ein Info-Log geschrieben <br> warning => es wird ein Warning-Log geschrieben <br> error => es wird ein Fehler-Log geschrieben und die Verarbeitung der Nachricht wird abgebrochen|
|XPathDepartmentId|hier kann der XPath für die Ermittlung des Empfängergerichtscodes hinterlegt werden. Der namespace "www.xjustiz.de" ist under "xjustiz" verfügbar und angabepflichtig|
|Departments|hier können die einzelnen Empfängergerichte mit Name, code und Exportpfaden hinterlegt werden|

<br><br>

## CLI Optionen
der EgvpEpReiceiver kann mit folgenden Parametern aufgerufen werden:

|Parameter|Beschreibung|
|---|---|
|-a, --autoimport|Automatischer Import anhand des Settings 'configfile'|
|-u, --userid=EGVP-Postbox-Id |EGVP-Postbox-Id, nur in Verbindung mit Angabe der 'msgid'|
|-m, --msgid=EGVP-Message-Id |EGVP-Message-Id, nur in Verbindung mit Angabe der 'userid'|
|--stat, --status|Osci-Status zu einer Nachricht prüfen (nur in Verbindung mit User- und Message-Id)|
|-h, -?, --help|Hilfe anzeigen|
|-v, --version|Versionsinformationen anzeigen|

## Development

Um das Projekt aus der Entwicklungsumgebung zu erstellen/starten sind folgende Schritte nowendig
- in src\OvgRlp.EgvpEpReceiver\Configuration\ muss die Datei PostboxSettings.template.json in PostboxSettings.json umbenannt und entsprechend mit eigenen Werten gefüllt werden.
- in src\OvgRlp.EgvpEpReceiver\ kann die Datei App.Debug.template.config in App.Debug.config und die Datei App.Release.template.config in App.Release.config umbenannt werden. Dort können anschließend der connectionString sowie die weiteren Einstellungen zur lokalen Serverumgebung hinterlegt werden.
- um den Webservice zu aktualisieren, bspw. bei upgrade auf eine neuere Egvp-Enterprise version, sind folgende Schritte notwendig
  - Rechtsklick auf Connected Service "EgvpEnterpriseSoap" im Projekt OvgRlp.EgvpEpReceiver.Infrastructure
  - in dem sich öffnenden Kontextmenü muss als erstes via Menüpunkt "Configure Service Reference" ein entsprechend erreichbarer Egvp-Enterprise Webservice konfiguriert werden (siehe auch auskommentierte Beispiele in der Datei)
  - anschließend kann über den Menüpunkt "Update Service Reference" der Webservice entsprechend aktualisiert werden


# Kontakt

Oberverwaltungsgericht Rheinland-Pfalz, Deinhardpassage 1, 56068 Koblenz 
Telefon: 0261 1307 - 0
poststelle(at)ovg.jm.rlp.de

# Lizenz

Copyright ©2021 Oberverwaltungsgericht Rheinland-Pfalz 
Lizenziert unter der EUPL, version 1.2 oder höher
Für weitere Details siehe Lizenz.txt oder EUPL-1.2 EN.txt
oder online unter https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12