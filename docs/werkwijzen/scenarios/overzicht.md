# Scenario's — Overzicht

Het OMC verwerkt events van de NotificatiesAPI en bepaalt voor elk event welk notificatiescenario van toepassing is. Als het scenario overeenkomt, haalt het OMC de relevante gegevens op en verstuurt een notificatie via NotifyNL.

---

## Hoe scenario's werken

1. Open Notificaties levert een event af op `POST /Events/Listen`
2. Het OMC valideert het JWT Bearer-token
3. Het OMC inspecteert het event (`kanaal`, `resource`, `actie`, `kenmerken`)
4. Het OMC zoekt het overeenkomende scenario op
5. Het OMC controleert of het zaaktype/besluittype/objecttype/producttype op de whitelist staat
6. Het OMC haalt aanvullende gegevens op uit de ZGW API's (zaak, status, klantgegevens)
7. Het OMC selecteert het communicatiekanaal (e-mail, sms, brief) op basis van de klantvoorkeur
8. Het OMC verstuurt de notificatie via NotifyNL met het geconfigureerde template

---

## Partij niet gevonden

Vindt het OMC in OpenKlant geen partij bij het BSN, KVK-nummer of de partij-id die het opzoekt, dan wordt het event in **elk** scenario afgebroken met `206 Partial Content` en **niet** opnieuw aangeboden door Open Notificaties: opnieuw proberen lost ontbrekende gegevens niet op. De reden in het antwoord noemt het soort identificator, bijvoorbeeld:

> No partij found in OpenKlant for this BSN (partijIdentificator "bsn"). Nobody was notified.

Het BSN of KVK-nummer zelf staat nooit in de reden, logs of traces. Een partij-id (uuid) wel — dat is geen persoonsgegeven.

Dit geldt alleen wanneer OpenKlant daadwerkelijk antwoordt dat de partij er niet is. Is OpenKlant onbereikbaar, of antwoordt het met een andere fout, dan blijft het een mislukking (`412`) en biedt Open Notificaties het event opnieuw aan.

De print- en Berichtenbox-flows maken een ontbrekende partij zelf aan; daar treedt dit alleen op als de partij ook na het aanmaken niet gevonden wordt.

---

## Adresselectie

Het OMC bepaalt het contactkanaal en -adres in de volgende volgorde:

1. **E-mail** — als er een e-mailadres beschikbaar is in de klantregistratie
2. **Sms** — als er een telefoonnummer beschikbaar is en geen e-mailadres
3. **Brief (PostGuard)** — als PostGuard is geconfigureerd en de burger een Yivi-wallet heeft

Als er geen contactgegevens beschikbaar zijn voor een burger, wordt de notificatie overgeslagen en wordt dit gelogd.

---

## Templateplaceholders

Elk scenario ondersteunt een set van `((placeholder))`-variabelen die kunnen worden gebruikt in NotifyNL-templates. Deze worden dynamisch gevuld door het OMC op basis van de opgehaalde ZGW-gegevens.

Placeholders volgen de naamgevingsconventie `((object.veldnaam))`, waarbij de veldnamen directe verwijzingen zijn naar ZGW API-responsvelden.

---

## Beschikbare scenario's

| Scenario | Trigger kanaal | Trigger resource |
|---|---|---|
| [Zaak aangemaakt](zaak-aangemaakt.md) | `zaken` | `status` (volgnummer=1) |
| [Zaak gewijzigd](zaak-gewijzigd.md) | `zaken` | `status` (niet eindstatus) |
| [Zaak afgesloten](zaak-afgesloten.md) | `zaken` | `status` (eindstatus) |
| [Taak toegewezen](taak-toegewezen.md) | `objecten` | taakobjecttype |
| [Besluit genomen](besluit-genomen.md) | `besluiten` | `besluit` |
| [Bericht ontvangen](bericht-ontvangen.md) | `objecten` | berichtobjecttype |
| [Print (printstraat)](print-printstraat.md) | `objecten` | printobjecttype |
| [Berichtenbox (MOBB)](berichtenbox-mobb.md) | — | CloudEvent `nl.overheid.berichten.bericht-gepubliceerd` |
| [Product aangemaakt](producten.md) | `producten` | `product` (actie `create`) |

---

## Niet-geïmplementeerd scenario

Als het OMC een event ontvangt waarvoor geen scenario overeenkomt (bijv. een onbekend kanaal, niet-ondersteunde resource of zaaktype niet op de whitelist), reageert het met `200 OK` en logt het de reden voor het overslaan. Dit voorkomt dat Open Notificaties het event herhaaldelijk opnieuw levert.
