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
