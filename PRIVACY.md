# Privacy policy

[Español](#política-de-privacidad)

AutoReady runs on your PC. It has no accounts, no analytics and no telemetry, and it never sends your screenshots,
settings or logs anywhere. It only connects to the internet for the features below, and each one can be turned off.

| Service | What is sent | When | How to turn it off |
|---|---|---|---|
| **GitHub** (`api.github.com`, `github.com`) | A request for the list of AutoReady releases, and the download of a new version (installed automatically when no game is open) | At startup and once a day | Panel → More options → *Update automatically* |
| **Steam Community** (`steamcommunity.com`) | A request for the public mini-profile of the Steam account signed in on this PC, to read its status (for example "Finding A Match") | Every few seconds, only while a supported game that uses it is open | During installation (*Use my Steam status*), or later with `UseSteamStatus=False` in `%APPDATA%\AutoReady\autoready.ini` |
| **ntfy** (`ntfy.sh`) | The notification text (for example "Match accepted") sent to your private topic | Only if you turn on *Phone* notifications, and only when there's something to notify | Panel → *Phone notifications* (off by default) |
| **Discord** (`discord.com`) | A short message (for example "Dota 2 match accepted") posted to the webhook link you pasted, in your own server | Only if you connect a webhook, and only when AutoReady accepts a match | Panel → More options → *Tell friends on Discord* → *Remove* (off by default) |

What stays on your PC:

- Settings and logs in `%APPDATA%\AutoReady`.
- During the beta, screenshots of the game window to improve detection, in `%APPDATA%\AutoReady\samples` (latest 15).
  They are never uploaded. Turn them off with `SaveSamples=False` in `%APPDATA%\AutoReady\autoready.ini`.
- For League of Legends, AutoReady talks only to the League client's local API on `127.0.0.1`, using the credentials the client writes to its own folder while open. Nothing leaves your PC.
- A Game State Integration config file in the folders of supported games (Dota 2 and CS2 today), so those games tell AutoReady (and only AutoReady,
  on `127.0.0.1`) when you're in a match.

The phone setup page ([hercase.github.io/autoready/phone](https://hercase.github.io/autoready/phone/)) that the QR code
opens is a static page on GitHub Pages with no cookies, analytics or third-party scripts. Your code goes after the `#`
in the link, so the browser never sends it to any server.

These services have their own privacy policies: [GitHub](https://docs.github.com/site-policy/privacy-policies/github-general-privacy-statement),
[Steam](https://store.steampowered.com/privacy_agreement/), [ntfy](https://ntfy.sh/docs/privacy/), [Discord](https://discord.com/privacy).

The Discord webhook link is a secret (anyone with it can post to that channel). It is stored only in
`%APPDATA%\AutoReady\autoready.ini` and never written to the log.

---

# Política de privacidad

AutoReady funciona en tu PC. No tiene cuentas, ni estadísticas, ni telemetría, y nunca manda tus capturas, tu
configuración ni tus logs a ningún lado. Solo se conecta a internet para estas funciones, y cada una se puede apagar.

| Servicio | Qué se envía | Cuándo | Cómo apagarlo |
|---|---|---|---|
| **GitHub** (`api.github.com`, `github.com`) | Un pedido de la lista de versiones de AutoReady, y la descarga de una versión nueva (se instala sola cuando no hay un juego abierto) | Al iniciar y una vez por día | Panel → Más opciones → *Actualizar automáticamente* |
| **Comunidad de Steam** (`steamcommunity.com`) | Un pedido del mini-perfil público de la cuenta de Steam conectada en esta PC, para leer su estado (por ejemplo "Buscando partida") | Cada pocos segundos, solo mientras está abierto un juego compatible que lo usa | Al instalar (*Usar mi estado de Steam*), o después con `UseSteamStatus=False` en `%APPDATA%\AutoReady\autoready.ini` |
| **ntfy** (`ntfy.sh`) | El texto del aviso (por ejemplo "Partida aceptada") enviado a tu canal privado | Solo si activás los avisos al *Celular*, y solo cuando hay algo para avisar | Panel → *Avisos al celular* (apagado por defecto) |
| **Discord** (`discord.com`) | Un mensaje corto (por ejemplo "Partida de Dota 2 aceptada") publicado en el link de webhook que pegaste, en tu propio servidor | Solo si conectás un webhook, y solo cuando AutoReady acepta una partida | Panel → Más opciones → *Avisar en Discord* → *Quitar* (apagado por defecto) |

Lo que queda en tu PC:

- La configuración y los logs, en `%APPDATA%\AutoReady`.
- Durante la beta, capturas de la ventana del juego para mejorar la detección, en `%APPDATA%\AutoReady\samples` (las
  últimas 15). Nunca se suben. Se apagan con `SaveSamples=False` en `%APPDATA%\AutoReady\autoready.ini`.
- Para League of Legends, AutoReady habla solo con la API local del cliente de LoL en `127.0.0.1`, con las credenciales que el cliente escribe en su propia carpeta mientras está abierto. No sale nada de tu PC.
- Un archivo de configuración de Game State Integration en las carpetas de los juegos compatibles (hoy, Dota 2 y CS2), para que esos juegos le avisen
  a AutoReady (y solo a AutoReady, en `127.0.0.1`) cuando estás en partida.

La página para conectar el celular ([hercase.github.io/autoready/phone](https://hercase.github.io/autoready/phone/)) que
abre el código QR es una página estática en GitHub Pages, sin cookies, estadísticas ni scripts de terceros. Tu código va
después del `#` en el link, así que el navegador nunca lo manda a ningún servidor.

Estos servicios tienen sus propias políticas de privacidad: [GitHub](https://docs.github.com/site-policy/privacy-policies/github-general-privacy-statement),
[Steam](https://store.steampowered.com/privacy_agreement/), [ntfy](https://ntfy.sh/docs/privacy/), [Discord](https://discord.com/privacy).

El link del webhook de Discord es un secreto (quien lo tenga puede publicar en ese canal). Se guarda solo en
`%APPDATA%\AutoReady\autoready.ini` y nunca se escribe en el log.
