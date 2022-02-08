# Doppler HTML Editor API

## API preliminar design

### Campaigns

```http
GET /accounts/{accountName}/campaigns/{campaignId}/content
```

Response:

```json
{
  "type": "unlayer",
  "meta": {
    "counters": {...},
    "body": {...},
    "schemaVersion": 6,
  },
  "HTML": "",
  "thumbnailUrl": ""
}
```

or

```json
{
  "type": "html",
  "HTML": "",
  "thumbnailUrl": ""
}
```

### Thumbnails

```http
GET /accounts/{accountName}/campaigns/{campaignId}/content/thumbnail
```

Response:

PNG/JPG file or redirect to image URI

## Save Campaign

```http
PUT /accounts/{accountName}/campaigns/{campaignId}/content

{
  "type": "unlayer",
  "meta": {
    "counters": {...},
    "body": {...},
    "schemaVersion": 6,
  },
  "HTML": "",
  "thumbnailUrl": ""
}
```

or

```http
PUT /accounts/{accountName}/campaigns/{campaignId}/content

{
  "type": "html",
  "HTML": "",
  "thumbnailUrl": ""
}
```

---

## Templates

```http
GET /accounts/{accountName}/templates/{templateId}
```

Response:

```json
{
  "name": "",
  "meta": {
    "counters": {},
    "body": {},
    "schemaVersion": 6
  },
  "HTML": "",
  "thumbnailUrl": ""
}
```

## Save Template

```http
PUT /accounts/{accountName}/templates/{templateId}

{
  "name": "",
  "meta": {
    "counters": {},
    "body": {},
    "schemaVersion": 6
  },
  "HTML": "",
  "thumbnailUrl": ""
}
```

## Create Template

```http
POST /accounts/{accountName}/templates

{
  "name": "",
  "meta": {
    "counters": {},
    "body": {},
    "schemaVersion": 6
  },
  "HTML": "",
  "thumbnailUrl": ""
}
```

### Shared Templates

```http
GET /shared/templates/{templateId}
```

Response:

```json
{
  "name": "",
  "meta": {
    "counters": {},
    "body": {},
    "schemaVersion": 6
  },
  "HTML": "",
  "thumbnailUrl": ""
}
```

## DB Data

Current campaigns data:

```txt
Name                        IdCampaign  IdSendingTimeZone   IdUser  FromName        FromEmail                   ReplyTo UTCCreationDate             CurrentStep     Active  UTCLastUpdatedDate  Subject                     CampaignType    IdContent   IsSoftBounced   ContentType     HtmlSourceType  Status
Prueba de importar ZIP      12092745    NULL                88469   Prueba Doppler  amoschini@fromdoppler.net   NULL    2022-02-08 16:57:42.083     2               1       NULL                Prueba de importar ZIP      CLASSIC         NULL        NULL            2               1               1
Prueba Importar HTML simple 12092726    NULL                88469   Prueba Doppler  amoschini@fromdoppler.net   NULL    2022-02-08 16:50:50.690     2               1       NULL                Prueba Importar HTML simple CLASSIC         NULL        NULL            2               1               1
Prueba Editor HTML          12092720    NULL                88469   Prueba Doppler  amoschini@fromdoppler.net   NULL    2022-02-08 16:49:56.877     2               1       NULL                Prueba Editor HTML          CLASSIC         NULL        NULL            2               3               1
Prueba MSEditor             12092719    NULL                88469   Prueba Doppler  amoschini@fromdoppler.net   NULL    2022-02-08 16:48:58.617     2               1       NULL                Prueba MSEditor             CLASSIC         NULL        NULL            2               2               1
Prueba Sin Contenido        12092716    NULL                88469   Prueba Doppler  amoschini@fromdoppler.net   NULL    2022-02-08 16:46:59.900     1               1       NULL                Prueba Sin Contenido        CLASSIC         NULL        NULL            2               NULL            1
```

Current contents data:

```txt
Name                            IdCampaign  Content                     PlainText   IsPlainTextUpdated  IdTemplate  Head                EditorType  Meta
Prueba de importar ZIP          12092745    "<h1>HTML en Zip</h1>..."   NULL        0                   NULL        NULL                NULL        NULL
Prueba Importar HTML simple     12092726    "<h1>HTML Simple</h1>..."   NULL        0                   NULL        NULL                NULL        NULL
Prueba Editor HTML              12092720    "<div>Hola, esto es..."     NULL        0                   NULL        NULL                NULL        NULL
Prueba MSEditor                 12092719    "{""id"":""12092719..."     NULL        0                   214173      "<meta http-equiv=  4           NULL
Prueba Sin Contenido            NULL        NULL                        NULL        NULL                NULL        NULL                NULL        NULL
```

Summary:

- Campaign without content (step 1/4)

  - `Campaign.ContentType = 2`
  - `Campaign.Status = 1`
  - `Campaign.HtmlSourceType = NULL`
  - `Content.Meta = NULL`

- MSEditor campaign (step 2/4)

  - `Campaign.ContentType = 2`
  - `Campaign.Status = 1`
  - `Campaign.HtmlSourceType = 2 (TEMPLATE)`
  - `Content.EditorType = 4`
  - `Content.Meta = NULL`

- HTML Editor campaign (step 2/4)

  - `Campaign.ContentType = 2`
  - `Campaign.Status = 1`
  - `Campaign.HtmlSourceType = 3 (EDITOR)`
  - `Content.EditorType = NULL`
  - `Content.Meta = NULL`

- Campaign with imported content (step 2/4)

  - `Campaign.ContentType = 2`
  - `Campaign.Status = 1`
  - `Campaign.HtmlSourceType = 1 (IMPORT)`
  - `Content.EditorType = NULL`
  - `Content.Meta = NULL`

New Unlayer Editor Campaign:

- MSEditor campaign (step 2/4)

  - `Campaign.ContentType = 2`
  - `Campaign.Status = 1`
  - `Campaign.HtmlSourceType = 2 (TEMPLATE)` TBD
  - `Content.EditorType = 5`
  - `Content.Meta = NON-NULL`
