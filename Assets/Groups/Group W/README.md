Im Folgenden finden sich TODOs und DONEs.

# TODOs

## Integration mit Interconnections Gruppe
## Spielende: Zeitablauf // alle besiegt --> Punkteverteilung
## Custom Animationen
	* Schlagen (für Nahkampfwaffe)
	* Werfen (für Fernkampfwaffe)
	* Sterben (langsam und qualvoll zu Boden fallen)

## Verschönerungen
	* Anzeige "Weakness", "Neutral", "Resistence", wenn Charakter angegriffen wurde
	* HP-Balken sollte sich zwar mit bewegen, wenn ein Charakter läuft, aber nicht, wenn er sich umdreht 
		** evlt. PlayerProperties an MinifigCharakter statt PlayerMinifig XXXX anhängen

## KI Mitspieler
## Tutorial? Brauchen wir das?

# DONEs Larissa
* Scenes, Scripts Ordner in unserem Group W Ordner angelegt
* Charaktere entsprechend der Kampfsituation plaziert
* PlayerProperties Skript zum definieren von:
  * Maximaler / aktueller HP
  * möglicher Reihenpositionen / aktueller Reihenposition
  * auswählbaren Waffen / aktueller Waffe
	* kann nun über enstprechende Tastenbelegung während Decision Phase gesetzt werden
  * auswählbaren Ziel-Reihenpositionen / aktuell ausgewählte Ziel-Reihenposition
	* kann nun über enstprechende Tastenbelegung während Decision Phase gesetzt werden

 * Timeranzeige (inkl. Beschreibung, in welcher Phase wir uns befinden, und was zu tun ist)
  * x Sek. bis Entscheidungsphase vorbei.
  * Wenn abgelaufen: Übergang zu Action Phase
  * Hier gibt es einen Boolean der entscheidet, ob die Kampfphase vorbei ist; sollte vorbei sein, wenn alle Kampfanimationen vorbei sind und demnach über eine andere Komponente gesetzt werden, wenn wir soweit sind. Bis dahin: erstmal gleicher Zeitablauf wie in der Entscheidungsphase
  * Auswahl der Waffe / des Ziels über entsprechende Tasten. Standard-Tastenbelegung wurde ausgetauscht.
	* Tastenbelegung (anhand Beispiel des WASD-Charakters)
	* Q = Lego
	* W = Paper
	* E = Scissors
	* R = Front row
	* F = Back row
* Feedback für "Waffe ausgewählt" und "Ziel ausgewählt" (lokaler Gegner soll Auswahl nicht sehen können!)
	* Feedback über triggern einer entsprechenden Animation des Charakters.
	* Verfügbare Animationen finden sich in Assets/Packages/LEGO/Scripts/Lego Minifig/MinifigController.cs, enum SpecialAnimation.
	* Wenn in Decision Phase:
		* Waffe ausgewählt: Dance
		* Ziel ausgewählt: Wave
	* Wenn in Action Phase:
		* Waffe/Ziel ausgwählt: IdleImpatient
* JSONs zum externen konfigurieren von Waffeneigenschaften
	* type
	* weakness
	* strength
	* asset
	* power
	* row
* Schadensberechnung - Base damage der Waffe (Vorder/Hinterreihe) mal Multiplier (Stärke/Schwäche/Neutralität gegenüber Ziel)
* Angriffsanimation
	* Front: zu Gegner hinlaufen, angreifen (derzeit auch für Back)
	* Update des HP-Balkens
* Beschränkung der Handlungsmöglichkeiten innerhalb Decision Phase
	* Target ist valide, wenn 
		* front vs. front
		* back vs. front 
		* back vs. back
		* && hp > 0

	* Target ist invalide, wenn
		* front vs. back
		* hp <= 0

	* Abspielen entsprechender Animation, wenn invalide Auswahl getroffen wird
* Wenn Charakter tot: zu Boden fallen, liegen bleiben
* Angriffsanimationen
	* Back: stehenbleiben, Fernkampfwaffe nutzen (z.B. Waffe werfen)
	* Front: zum Gegner hinlaufen

# DONEs Polina
* Methode/Funktion mit folgendem Verhalten:
	* eine Reihe (Front/Back) und einen Waffentyp (Lego/Paper/Scissors) als Parameter entgegen nehmen
		*  für entsprechende Enums, siehe PlayerProperties.cs
	    *  Assets Fernkampf + Nahkampf unterschiedlich
	* der entsprechende Charakter, an dem das Script attached ist, sollte dann (visuell) die entsprechende Waffe in die Hand gelegt bekommen
		* Waffe sollte sich bei Bewegung des Charakters mitbewegen

* Environment
	* Zusammenbasteln einer hübschen Umgebung
		* Boden
		* Himmel / Decke
		* Objekte (z.B. Bäume, Gebäude etc.)
	* Beleuchtung
	* Passendere Asssets für Rundenanzeige und HP-Balken


# Known Bugs
* Animation auf Dpad open wird nicht abgespielt, Event wird aber korrekt abgegriffen und Methode ausgeführt. Fehlersuche ausstehend.