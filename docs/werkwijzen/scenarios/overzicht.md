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

Het OMC kiest het digitale adres van de partij in OpenKlant in deze volgorde:

1. **Gekoppeld aan de zaak** — een digitaal adres waarvan de `referentie` de zaak is
2. **Voorkeursadres** — het `voorkeursDigitaalAdres` van de partij
3. **Standaardadres** — een digitaal adres met `isStandaardAdres: true`; zijn er zowel een e-mailadres als een telefoonnummer, dan wint het e-mailadres
4. **Portaalvoorkeur** — een digitaal adres met `referentie` = `portaalvoorkeur` (zoals Den Haag dat vastlegt); ook hier wint het e-mailadres van het telefoonnummer

Een adres dat aan geen van deze vier voldoet, wordt niet gekozen, ook als het het enige adres van de partij is. Het kanaal volgt uit het gekozen adres: een e-mailadres wordt een e-mail, een telefoonnummer een sms. Als PostGuard is geconfigureerd en de burger een Yivi-wallet heeft, kan ook een brief via PostGuard verstuurd worden.

Als er geen contactgegevens beschikbaar zijn voor een burger, wordt de notificatie overgeslagen en wordt dit gelogd.

---

## Nooit twee keer versturen

Dezelfde notificatie kan het OMC vaker dan eens bereiken, bijvoorbeeld als Open Notificaties hem na een fout opnieuw aflevert. Vóór elke verzending vraagt het OMC daarom bij NotifyNL na of er al een notificatie van dezelfde soort (e-mail, sms, brief of Berichtenbox) met precies dezelfde `reference` bestaat:

- **Bestaat al:** het OMC verstuurt niets en gaat verder alsof de verzending gelukt is. Het contactmoment volgt uit de afleverstatus van die eerdere verzending.
- **Bestaat niet:** het OMC verstuurt zoals gewoonlijk.
- **Navragen lukt niet:** het OMC verstuurt niet blind. In de scenario's voor zaken, besluiten, taken, de printstraat en de Berichtenbox geeft het een fout terug, zodat het later opnieuw geprobeerd wordt; bij producten, waar het event dan al is aangenomen, registreert het een mislukt contactmoment voor die eigenaar.

Dat werkt omdat de `reference` bij elke aflevering van dezelfde notificatie gelijk is: er staat geen tijdstip in, en de trace-id van het dashboard wordt afgeleid van de notificatie zelf. Een herlevering verschijnt op het dashboard daardoor als dezelfde trace. De soort telt mee, omdat de e-mail en de sms van één notificatie dezelfde `reference` hebben.

Hetzelfde geldt voor de klantcontacten. Elk klantcontact uit een afleverstatus krijgt het notificatie-id van NotifyNL in zijn `metadata` (`notifyNotificationId`). Komt dezelfde afleverstatus nog eens binnen, omdat NotifyNL hem na een fout opnieuw stuurt of hem dubbel stuurt, dan zoekt het OMC eerst het klantcontact van die partij, dat onderwerpobject en dat kanaal met dat id op. Bestaat dat al, dan maakt het OMC alleen af wat nog ontbrak: de koppeling met de actor van het OMC, en bij de printstraat de PDF als bijlage.

Wat dit niet afvangt: twee afleveringen van dezelfde notificatie binnen dezelfde (milli)seconden, wat alleen bij een dubbel abonnement in Open Notificaties gebeurt, en een herlevering na meer dan 7 dagen, de bewaartermijn van NotifyNL.

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
