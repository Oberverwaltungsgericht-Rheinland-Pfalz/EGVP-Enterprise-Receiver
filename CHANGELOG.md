
<a name="2.0.0.2123"></a>

### 2.0.1.2209 (02.03.2022)

#### &Auml;nderungen

* Ungültige Zeichen im Dateinamen der Nachrichtenanhänge werden nun abgefangen (#13)

### 2.0.0.2123 (07.06.2021)

#### &Auml;nderungen

* Neue Möglichkeit Empfängergerichte aus xjustiz.xml zu ermitteln und deren Ablage-Pfade aus der Konfigurationsdatei zu lesen (#10)
* Refaktorisierung der kompletten Postbox-Konfigurationslogik
* Anpassungen auf Egvp-Enterprise 4.0 Webservice (#9)
* Refaktorisierung der Dateistruktur und damit einhergehend bessere Stabilität

#### BREAKING CHANGES

* die Konfigurationsdatei (Setting "configfile") ist nun im json-Format anzugeben - siehe Projektbeschreibung

### 1.5.5.2104 (26.01.2021)

#### &Auml;nderungen

* fix für doppelte Dateinamen
* Logging generell verbessert
* Neues Setting "LockFile". Hiermit ist es möglich während dem Schreibvorgang eine weitere Datei anzulegen (bspw. als Info für ein Fachsystem)
* Logging: Neues Log-Attribut MessageSizeAttachmentsKB in den Metadaten
* fix für Dateinamen mit Überlänge
* Stabilität verbessert

### 1.4.2.1903 (17.01.2019)

#### &Auml;nderungen

* xjustiz.xml-Ermittlung verbessert
* Konsolenanwendung um '-stat' erweitert um den Nachrichten- bzw. Abholstatus zu einer Message zu prüfen
* Stabilität verbessert

### 1.3.0.1825 (19.06.2018)

#### &Auml;nderungen

* Umstellung von lokale Serviceklassen auf OvgRlp.Core 

### 1.1.0.1822 (01.06.2018)

#### &Auml;nderungen

* __EEB - Export in Abweichende Pfade:__ Empfangsbekenntnisse können nun in abweichende Pfade exportiert werden. Dazu kann im Configfile neben dem TAG "export", der Tag "exportEeb" genutzt werden. 
  * Ist der TAG "exportEeb" nicht genutzt, werden die EEB´s weiterhin in die Standard-Exportpfade exportiert (Tag "export").
  * Da die EEB´s keinen besonderen Nachrichtentyp aufweisen, werden diese anhand der Datei "attachments/xjustiz_nachricht.xml" erkannt. Diese Datei wird auf die Zeichenfolge "nachricht.eeb.zuruecklaufend" untersucht.

### 1.0.0.1822 (29.05.2018)

#### &Auml;nderungen

* Programmerstellung
