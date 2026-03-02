# Rapport: Autonome Navigatie en Object-Interactie via Deep Reinforcement Learning in Unity

## Inleiding
Dit rapport documenteert de ontwikkeling en training van een intelligente agent binnen de Unity-omgeving met behulp van de **ML-Agents toolkit**. Het doel van dit onderzoek is om te evalueren of een digitale agent via *Proximal Policy Optimization* (PPO) een complexe, sequentiële taak kan aanleren: het lokaliseren en collecteren van een bewegend doelobject (bol), gevolgd door het navigeren naar een statische bestemmingszone (groene zone). Dit rapport is bedoeld voor ontwikkelaars en onderzoekers die geïnteresseerd zijn in de praktische implementatie van reinforcement learning voor navigatievraagstukken.

---

## Methoden
Het systeem is opgebouwd uit twee primaire componenten binnen het Unity-ecosysteem:

1.  **Behaviour Parameters:** Hierin worden de "hersenen" van de agent gedefinieerd. De **Vector Observation Space Size** werd ingesteld op **10** (posities van agent, target, goal en de status-boolean). De actieruimte is **Continuous** met een grootte van **2** voor de X- en Z-beweging.
2.  **Agent (CubeAgent):** Een C#-script dat de logica bevat. De agent leert door middel van beloningen en straffen.

### Override Methods van de Agent
* **`OnEpisodeBegin()`**: Reset de omgeving, verplaatst de bol naar een willekeurige locatie en herstelt de status van de agent (`hasTarget = false`).
* **`CollectObservations(VectorSensor sensor)`**: Verzamelt de noodzakelijke data uit de omgeving. Cruciaal hierbij is de toevoeging van de **hasTarget** boolean, waardoor de agent "weet" in welke fase van de opdracht hij zich bevindt.
* **`OnActionReceived(ActionBuffers actionBuffers)`**: Vertaalt de neurale netwerk-output naar beweging en berekent de beloningen. Er wordt gebruikgemaakt van **reward shaping** om de agent naar de doelen te leiden.
* **`Heuristic(in ActionBuffers actionsOut)`**: Maakt handmatige besturing via het toetsenbord mogelijk voor testdoeleinden.

---

## Resultaten
Tijdens de trainingsfase in de Conda-omgeving werden de volgende observaties gedaan via TensorBoard:

* **Cumulative Reward:** De gemiddelde beloning steeg consistent vanaf de start van de training en stabiliseerde rond een waarde van **2.94** na circa 160.000 stappen.
* **Episode Length:** De tijdsduur per episode nam exponentieel af in de eerste 100.000 stappen, waarna de curve afvlakte.
* **Gedrag:** In de beginfase vertoonde de agent willekeurig bewegingsgedrag en viel regelmatig van het platform. Naarmate de training vorderde, werd een directere lijn richting de bol en vervolgens de zone waargenomen. Bij 304.000 stappen trad er een verzadiging (plateau) op in de leercurve; extra training leverde geen significante verbetering meer op in de score.

---

## Conclusie
Op basis van de resultaten kan worden geconcludeerd dat de agent de gestelde doelen succesvol heeft gemanifesteerd in een stabiel beleid (policy). De score van **2.94** (nabij het theoretische maximum van 3.0) duidt op een zeer efficiënte navigatie met minimale tijdsverliezen. De stagnatie van de leercurve na 304.000 stappen suggereert dat het model volledig geoptimaliseerd is voor deze specifieke omgeving en dat verdere training bij de huidige hyperparameters geen meerwaarde biedt.

---

## Referenties
* (Unity Technologies, 2024), *Unity ML-Agents Toolkit Documentation*. [Online beschikbaar].
* (Gemini, 2026), *Persoonlijke assistentie bij C# scripting en YAML configuratie*.