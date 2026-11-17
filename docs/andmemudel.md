# Kodu24 andmemudel

Andmebaasis on 8 omavahel seotud tabelit. Andmebaasi poole pöördub ainult Web API.

## ER-diagramm

```mermaid
erDiagram
    USERS ||--o{ LISTINGS : "lisab"
    DISTRICTS ||--o{ LISTINGS : "asub"
    LISTINGS ||--o{ LISTING_IMAGES : "omab"
    USERS ||--o{ FAVORITES : "salvestab"
    LISTINGS ||--o{ FAVORITES : "on lemmik"
    USERS ||--o{ INQUIRIES : "saadab"
    LISTINGS ||--o{ INQUIRIES : "saab"
    USERS ||--o{ SAVED_SEARCHES : "salvestab"
    DISTRICTS |o--o{ SAVED_SEARCHES : "filter"
    USERS ||--o{ NOTIFICATIONS : "saab"
    SAVED_SEARCHES ||--o{ NOTIFICATIONS : "tekitab"
    LISTINGS ||--o{ NOTIFICATIONS : "viitab"

    USERS {
        int Id PK
        string Email UK
        string PasswordHash
        string FullName
        string Phone
        string Role
        datetime CreatedAt
    }
    DISTRICTS {
        int Id PK
        string Name UK
    }
    LISTINGS {
        int Id PK
        int LandlordId FK
        int DistrictId FK
        string Title
        string Address
        decimal Price
        int Rooms
        decimal AreaM2
        string Description
        double Latitude
        double Longitude
        datetime CreatedAt
        datetime ExpiresAt
        bool IsDeleted
        datetime DeletedAt
    }
    LISTING_IMAGES {
        int Id PK
        int ListingId FK
        string FileName
        bool IsPrimary
        int SortOrder
        datetime UploadedAt
    }
    FAVORITES {
        int Id PK
        int UserId FK
        int ListingId FK
        datetime CreatedAt
    }
    INQUIRIES {
        int Id PK
        int ListingId FK
        int SeekerId FK
        string Message
        bool IsRead
        datetime CreatedAt
    }
    SAVED_SEARCHES {
        int Id PK
        int UserId FK
        string Name
        decimal MinPrice
        decimal MaxPrice
        int Rooms
        int DistrictId FK
        datetime CreatedAt
        datetime LastNotifiedAt
    }
    NOTIFICATIONS {
        int Id PK
        int UserId FK
        int SavedSearchId FK
        int ListingId FK
        string Message
        bool IsRead
        datetime CreatedAt
    }
```

## Tabelite kirjeldus

| Tabel | Eesmärk |
|---|---|
| Users | Kasutajad. Roll on Admin, Landlord, Seeker või Service. Workeri teenusekonto on samuti kasutaja rolliga Service. |
| Districts | Linnaosad (nt Kesklinn, Kalamaja, Mustamäe). Kasutatakse filtris ja €/m² statistikas. |
| Listings | Üürikuulutused. Kuulutuse lisab üürileandja. |
| ListingImages | Kuulutuse pildid. Ühel kuulutusel on mitu pilti, üks neist on põhipilt. |
| Favorites | Otsija lemmikud (kasutaja ja kuulutuse seos). |
| Inquiries | Otsija päringud üürileandjale. |
| SavedSearches | Otsija salvestatud filtrid. Worker kasutab neid uutest kuulutustest teavitamiseks. |
| Notifications | Teavitused uute kuulutuste kohta, mille Worker loob salvestatud otsingu põhjal. |

## Seosed
- Üks kasutaja (Landlord) lisab palju kuulutusi (1:N).
- Üks linnaosa kuulub paljudele kuulutustele (1:N).
- Üks kuulutus omab palju pilte (1:N).
- Kasutajad ja kuulutused on lemmikute kaudu seotud mitu-mitmele (M:N, vahetabel Favorites).
- Üks kuulutus saab palju päringuid ja üks otsija saadab palju päringuid (1:N mõlemas suunas).
- Üks kasutaja salvestab palju otsinguid, üks otsing tekitab palju teavitusi (1:N).

## Piirangud ja otsused
- **Unikaalsus:** `Users.Email`, `Districts.Name` ja paar (`Favorites.UserId`, `Favorites.ListingId`) on unikaalsed.
- **Soft delete:** kuulutust ei kustutata andmebaasist, vaid märgitakse `IsDeleted = true` ja `DeletedAt`. Aegunud kuulutused (`ExpiresAt` on möödas) märgib Worker API kaudu. EF Core päringufilter peidab kustutatud kuulutused vaikimisi.
- **Kaskaadkustutamine:** kuulutuse pildid, lemmikud ja päringud kustutatakse koos kuulutusega (kui kuulutus päriselt eemaldatakse). Kasutajate kustutamist piirame (Restrict), et ajalugu ei kaoks.
- **Rahalised väärtused:** `Price` ja `AreaM2` on `decimal`. €/m² arvutatakse päringus (`Price / AreaM2`), seda ei salvestata.
- **Koordinaadid:** `Latitude` ja `Longitude` on kaardi jaoks (hinne 5).
- **Pildid:** failid salvestatakse kettale (nt `uploads/` kausta), andmebaasis on ainult `FileName`.
- **Valikulised väljad:** `SavedSearches.MinPrice`, `MaxPrice`, `Rooms` ja `DistrictId` on nullable, sest otsija võib filtreid vabalt kombineerida.
- **`LastNotifiedAt`:** Worker kasutab seda parameetrina `createdAfter`, et leida ainult uued kuulutused.
- **Indeksid:** `Listings` tabelis `(DistrictId, Price)` ja `CreatedAt`, kuna need on peamised filtrid. 