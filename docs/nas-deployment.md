# NAS Deployment Guide (Synology / media-server stack)

Audio Batch Converter runs alongside the existing `media-server` Docker stack on the NAS.
ffmpeg executes locally, so conversion happens at full disk speed — no files travel over the network.

**Web UI:** `http://<nas-hostname>:8080`

---

## Add to the media-server stack

### 1 — Add the service to docker-compose.yml

Append the following service block to the `media-server` `docker-compose.yml`,
inside the `services:` key (and before the closing `networks:` key):

```yaml
  audio-batch-converter:
    image: ghcr.io/nachitous/audio-batch-converter:latest
    container_name: audio-batch-converter
    user: "${PUID}:${PGID}"
    environment:
      - TZ=${TZ}
      - Worker__BrowserRoot=/volume1
    volumes:
      - ${MUSIC_DIR}:${MUSIC_DIR}:rw
      - ${DOWNLOADS_DIR}:${DOWNLOADS_DIR}:rw
    ports:
      - "8080:8080"
    restart: unless-stopped
    networks:
      - media-network
```

All referenced variables (`PUID`, `PGID`, `TZ`, `MUSIC_DIR`, `DOWNLOADS_DIR`) are already
defined in the stack's `.env` file — no new variables are needed.

### 2 — Copy the updated file to the NAS

From Windows, over the SMB share — replace `<nas-share>` with your NAS share path:

```
copy docker-compose.yml \\<nas-share>\docker\media-server\docker-compose.yml
```

Or edit the file directly on the share with any text editor.

### 3 — Pull the image and start the service

```powershell
$NAS = "$NAS_USER@$NAS_HOST"
"/c/Program Files/PuTTY/plink" -ssh -pw $NAS_PASS -P 22 $NAS `
  "cd $MEDIA_SERVER_DIR && $DOCKER compose pull audio-batch-converter && $DOCKER compose up -d audio-batch-converter"
```

Set the shell variables once (see [SSH reference](#ssh-reference) below).
Only `audio-batch-converter` is affected; all other containers keep running.

---

## Updating to a new version

```powershell
$NAS = "$NAS_USER@$NAS_HOST"
"/c/Program Files/PuTTY/plink" -ssh -pw $NAS_PASS -P 22 $NAS `
  "cd $MEDIA_SERVER_DIR && $DOCKER compose pull audio-batch-converter && $DOCKER compose up -d audio-batch-converter"
```

---

## Configuration

Settings are environment variables in the `environment:` block of the service definition.

| Variable | Description |
|----------|-------------|
| `Worker__BrowserRoot` | Root path the UI is allowed to browse (default: `${MUSIC_DIR}`) |
| `Worker__Extensions` | Comma-separated audio extensions to find (default: `.flac,.wav,.ogg,.m4a,.aac,.wma,.opus`) |

---

## SSH reference

> **Note:** `ssh` and `sshpass` do not work on Windows here.
> Always use `plink` from PuTTY with `-pw`.

Set these variables once in your PowerShell session before running any of the commands below:

```powershell
$NAS_USER        = "your-nas-username"
$NAS_HOST        = "your-nas-ip-or-hostname"
$NAS_PASS        = "your-nas-password"
$MEDIA_SERVER_DIR = "/volume1/docker/media-server"   # adjust if different
$DOCKER          = "/volume1/@appstore/ContainerManager/usr/bin/docker"
```

### Common operations

```powershell
$NAS = "$NAS_USER@$NAS_HOST"

# Live logs
"/c/Program Files/PuTTY/plink" -ssh -pw $NAS_PASS -P 22 $NAS `
  "cd $MEDIA_SERVER_DIR && $DOCKER compose logs -f audio-batch-converter"

# Stop
"/c/Program Files/PuTTY/plink" -ssh -pw $NAS_PASS -P 22 $NAS `
  "cd $MEDIA_SERVER_DIR && $DOCKER compose stop audio-batch-converter"

# Restart
"/c/Program Files/PuTTY/plink" -ssh -pw $NAS_PASS -P 22 $NAS `
  "cd $MEDIA_SERVER_DIR && $DOCKER compose restart audio-batch-converter"

# Container status
"/c/Program Files/PuTTY/plink" -ssh -pw $NAS_PASS -P 22 $NAS `
  "$DOCKER ps --filter name=audio-batch-converter"
```

---

## Standalone deployment (without media-server)

Use the `docker-compose.yml` at the repo root as a standalone stack:

1. Copy `.env.example` → `.env` and fill in your values.
2. Copy both files to a directory on the NAS (e.g. a `audio-converter` folder under your Docker config directory).
3. Start:

```powershell
"/c/Program Files/PuTTY/plink" -ssh -pw $NAS_PASS -P 22 $NAS `
  "cd /path/to/audio-converter && $DOCKER compose pull && $DOCKER compose up -d"
```
