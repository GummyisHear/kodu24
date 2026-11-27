# Kodu24 rollid ja ligipääs

Igal kasutajal on täpselt üks roll. Roll on JWT tokenis (`role` väide) ja API kontrollib seda igal päringul.

| Roll | Kes see on | Kuidas konto tekib |
|---|---|---|
| `Seeker` | Üürija, kes otsib korterit | Registreerib ise (`POST /api/auth/register`) |
| `Landlord` | Üürileandja, kes lisab kuulutusi | Registreerib ise |
| `Admin` | Portaali haldaja | Ainult algandmetes, ise registreerida ei saa |
| `Service` | Taustateenus (Worker) | Ainult algandmetes, ise registreerida ei saa |

## Kaks kontrollitaset

1. **Rolli kontroll** kontrolleris: `[Authorize(Policy = Policies.Landlord)]`. Vale roll → **403**, token puudub → **401**.
2. **Omaniku kontroll** teenuste kihis (`AccessRules`): näiteks üürileandja tohib muuta ainult **enda** kuulutusi. Rikkumine → **403**.

Ainult rolli kontrollist ei piisa, sest kõigil üürileandjatel on sama roll. Seepärast kontrollib teenus lisaks, kas kuulutus kuulub sisselogitud kasutajale.

## Reeglid (policy)

| Policy | Lubatud rollid | Kasutus |
|---|---|---|
| `LandlordOrAdmin` | Landlord, Admin | Kuulutuste lisamine, muutmine, kustutamine, piltide haldus, saadud päringud |
| `Seeker` | Seeker | Lemmikud, päringu saatmine, salvestatud otsingud |
| `Admin` | Admin | Halduse otspunktid |
| `Service` | Service | Workeri otspunktid (aegunud kuulutused, teavitused) |

## Ligipääsutabel

✅ = lubatud, ❌ = keelatud, 👤 = ainult oma andmed

| Tegevus | Külaline | Seeker | Landlord | Admin | Service |
|---|---|---|---|---|---|
| Kuulutuste otsing ja vaatamine | ✅ | ✅ | ✅ | ✅ | ✅ |
| Linnaosad ja €/m² statistika | ✅ | ✅ | ✅ | ✅ | ✅ |
| Registreerimine, sisselogimine | ✅ | – | – | – | – |
| Kuulutuse lisamine | ❌ | ❌ | ✅ | ✅ | ❌ |
| Kuulutuse muutmine, kustutamine, pildid | ❌ | ❌ | 👤 | ✅ kõik | ❌ |
| Saadud päringute vaatamine ja loetuks märkimine | ❌ | ❌ | 👤 | ✅ kõik | ❌ |
| Ühe päringu vaatamine | ❌ | 👤 saatja | 👤 saaja | ✅ | ❌ |
| Lemmikud | ❌ | 👤 | ❌ | ❌ | ❌ |
| Päringu saatmine üürileandjale | ❌ | ✅ | ❌ | ❌ | ❌ |
| Salvestatud otsingud | ❌ | 👤 | ❌ | ❌ | ❌ |
| Teavitused | ❌ | 👤 | ❌ | ❌ | ❌ |
| Kasutajate ja kuulutuste haldus (`/api/admin`: rollid, kustutatud kuulutused ja taastamine, ülevaade) | ❌ | ❌ | ❌ | ✅ | ❌ |
| Aegunud kuulutuste märkimine, teavituste loomine | ❌ | ❌ | ❌ | ❌ | ✅ |
| Kõigi salvestatud otsingute lugemine | ❌ | ❌ | ❌ | ❌ | ✅ |

Isiklikke andmeid (salvestatud otsingud, teavitused) ei näe ka admin: neid näeb ainult omanik.

Admin ei saa muuta enda rolli (et mitte end välja lukustada) ega teenusekonto rolli. Uus roll hakkab kehtima pärast uut sisselogimist.

## Kus see koodis on

- `src/Kodu24.Api/Auth/Policies.cs`: rollide nimed ja reeglid
- `src/Kodu24.Api/Auth/AccessRules.cs`: omaniku kontrollid
- `src/Kodu24.Api/Auth/CurrentUser.cs`: sisselogitud kasutaja tokenist
- Testid: `tests/Kodu24.Tests/PoliciesTests.cs`, `AccessRulesTests.cs`
