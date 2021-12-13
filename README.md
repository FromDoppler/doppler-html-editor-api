# Doppler HTML Editor API

## Campaigns

```http
GET /accounts/{accountName}/campaigns/{campaignId}/content/design
```

Response:

```json
{
	"counters": {},
	"body": {},
	"schemaVersion": 6
}
```

## Thumbnails

```http
GET /accounts/{accountName}/campaigns/{campaignId}/content/thumbnail
```

Response:

PNG/JPG file or redirect to image URI

## Save Campaign

```http
PUT /accounts/{accountName}/campaigns/{campaignId}/content/

{
	"counters": {...},
	"body": {...},
	"schemaVersion": 6,
	"HTML": "",	// HTML
	"thumbnailUrl": "" // URI Image
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
	  "counters": {},
	  "body": {},
	  "name": "",
	  "schemaVersion": 6
}
```

## Save Template

```http
PUT /accounts/{accountName}/templates/{templateId}

{
	"name": "",
	"counters": {...},
	"body": {...},
	"schemaVersion": 6,
	"HTML": "",	// HTML
	"thumbnailUrl": "" // URI Image
}
```

## Create Template

```http
POST /accounts/{accountName}/templates

{
	"name": "",
	"counters": {},
	"body": {...},
	"schemaVersion": 6,
	"HTML": "",	// HTML
	"thumbnailUrl": "" // URI Image
}
```

## Shared Templates

```http
GET /shared/templates/{templateId}
```

Response:

```json
{
	  "counters": {},
	  "body": {},
	  "name": "",
	  "schemaVersion": 6
}
```