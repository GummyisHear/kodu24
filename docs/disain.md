# Kodu24 disainivisand

## Värvipalett
| Otstarve | Värv | HEX |
|---|---|---|
| Põhivärv (nupud, päis) | Sügavroheline | #1F6F5C |
| Põhivärv hover | Tumeroheline | #17574A |
| Aktsent (lemmikud, olulised tegevused) | Kollakasoranž | #F2A541 |
| Taust | Soe valge | #F7F5F0 |
| Kaardi taust | Valge | #FFFFFF |
| Tekst | Tumehall | #1F2933 |
| Teisene tekst | Hall | #6B7280 |
| Veateade | Punane | #C0392B |
| Õnnestumine | Roheline | #2E9E5B |

## Tüpograafia
- Font: Inter (varuvariandina sans-serif)
- Pealkirjad: 600 paksus, tekst: 400

## Logo

![Kodu24 logo](logo.svg)

Maja siluett, mille katuse all on number 24, kõrval sõna "Kodu24".
Põhivärv #1F6F5C, aktsent #F2A541. Fail: `docs/logo.svg`.

## Lehtede skeem

| Leht | Aadress | Ligipääs |
|---|---|---|
| Avaleht (otsing ja viimased kuulutused) | `/` | kõik |
| Kuulutuste nimekiri filtritega | `/listings` | kõik |
| Kuulutuste kaart | `/map` | kõik |
| Kuulutuse detailvaade | `/listings/:id` | kõik |
| Hinnastatistika (€/m²) | `/stats` | kõik |
| Sisselogimine | `/login` | külaline |
| Registreerimine | `/register` | külaline |
| Lemmikud | `/favorites` | Seeker |
| Salvestatud otsingud ja teavitused | `/saved-searches` | Seeker |
| Minu kuulutused | `/my-listings` | Landlord |
| Kuulutuse lisamine ja muutmine | `/my-listings/new`, `/my-listings/:id/edit` | Landlord |
| Saabunud päringud (reaalajas) | `/inquiries` | Landlord |
| Admini paneel (kasutajad, kuulutused) | `/admin` | Admin |

## Üldine paigutus
- Päis: logo vasakul, menüü keskel (Kuulutused, Kaart, Statistika), paremal sisselogimine või kasutajamenüü (lemmikud, päringud, välju)
- Põhiala: max laius 1200 px, keskele joondatud
- Jalus: projekti nimi ja GitHubi link

## Põhivaadete skeemid

**Kuulutuste nimekiri**
```
+----------------------------------------------------+
| LOGO   Kuulutused  Kaart  Statistika     [Logi sisse] |
+----------------------------------------------------+
| Filtrid:   | Sorteeri: [Uuemad v]     Leitud: 42    |
| Hind min   | +--------+ +--------+ +--------+        |
| Hind max   | | pilt   | | pilt   | | pilt   |        |
| Tubade arv | | 450 €  | | 520 €  | | 610 €  |        |
| Linnaosa   | | 2 tuba | | 1 tuba | | 3 tuba |        |
| [Otsi]     | | Kesklinn| | Kalamaja| | Mustamäe|     |
| [Salvesta] | +--------+ +--------+ +--------+        |
|            |     < 1 2 3 4 5 >                       |
+----------------------------------------------------+
```

**Kuulutuse detailvaade**
```
+----------------------------------------------------+
| [   suur pilt   ]   Aadress, linnaosa              |
| [pisipildid ....]   Hind: 450 €/kuu                |
|                     Tubade arv, pindala, €/m²      |
| Kirjeldus ...       [♥ Lemmikuks]                  |
|                     [Saada päring üürileandjale]   |
| Asukoht kaardil                                    |
+----------------------------------------------------+
```

**Üürileandja päringute leht**
```
+----------------------------------------------------+
| Saabunud päringud               (uus päring! ●)    |
| ------------------------------------------------   |
| Kuulutus: Tondi 12   Saatja: Mari   12.11 14:05    |
| "Tere, kas korter on veel vaba?"                   |
+----------------------------------------------------+
```