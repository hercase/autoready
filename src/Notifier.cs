using System;
using System.Net;
using System.Text;
using System.Threading;

namespace AutoReady
{
    // Push al celular con ntfy (https://ntfy.sh): app gratis, sin cuenta. El usuario se suscribe a su topic.
    static class Notifier
    {
        static Notifier()
        {
            // .NET Framework 4 no activa TLS 1.2 por defecto y ntfy.sh lo exige.
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
        }

        // Webhook de Discord: solo discord.com / discordapp.com y la ruta de webhooks, para no mandar nada a otro lado si el
        // portapapeles tiene cualquier otra cosa.
        static readonly System.Text.RegularExpressions.Regex DiscordUrl = new System.Text.RegularExpressions.Regex(
            @"^https://(?:(?:canary|ptb)\.)?(?:discord|discordapp)\.com/api/(?:v\d+/)?webhooks/\d+/[A-Za-z0-9_\-]+$");

        public static bool IsDiscordWebhook(string url) { return !string.IsNullOrEmpty(url) && DiscordUrl.IsMatch(url); }

        // Aviso a los amigos al aceptar. Sin menciones (@everyone, @here): el mensaje solo suena como cualquier otro.
        public static void Discord(Config cfg, string text)
        {
            if (!IsDiscordWebhook(cfg.DiscordWebhook)) return;
            SendDiscord(cfg.DiscordWebhook, text, null);
        }

        public static void SendDiscord(string url, string text, Action<bool> done)
        {
            string body = "{\"username\":\"AutoReady\",\"content\":" + JsonString(text) + ",\"allowed_mentions\":{\"parse\":[]}}";
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                        wc.UploadString(url, body);
                    }
                    if (done != null) done(true);
                }
                catch (Exception e)
                {
                    Log.Write("Discord: no se pudo enviar: " + e.Message);   // sin la URL: es un secreto
                    if (done != null) done(false);
                }
            });
        }

        // Texto como cadena JSON; lo que no es ASCII sale escapado (\uXXXX) para no depender de la codificacion.
        internal static string JsonString(string s)
        {
            StringBuilder sb = new StringBuilder("\"");
            foreach (char c in s ?? "")
            {
                if (c == '"') sb.Append("\\\"");
                else if (c == '\\') sb.Append("\\\\");
                else if (c < 0x20 || c > 0x7e) sb.Append("\\u").Append(((int)c).ToString("x4"));
                else sb.Append(c);
            }
            return sb.Append('"').ToString();
        }

        // urgent: prioridad maxima de ntfy (suena aunque el celular este en silencio, segun la app).
        public static void Phone(Config cfg, string title, string message, string tags, bool urgent = false)
        {
            if (!cfg.NotifyPhone) return;
            Send(cfg, title, message, tags, null, urgent ? "urgent" : "high");
        }

        // Manda en segundo plano para no trabar el escaneo si la red esta lenta. done (opcional) recibe si se pudo
        // enviar; se llama desde otro hilo.
        public static void Send(Config cfg, string title, string message, string tags, Action<bool> done, string priority = "high")
        {
            // Titulo y tags van en la URL: los headers HTTP no aceptan tildes ni emojis.
            string url = cfg.NtfyServer.TrimEnd('/') + "/" + Uri.EscapeDataString(cfg.NtfyTopic) +
                         "?title=" + Uri.EscapeDataString(title) + "&tags=" + Uri.EscapeDataString(tags) + "&priority=" + priority;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.UploadString(url, message);
                    }
                    if (done != null) done(true);
                }
                catch (Exception e)
                {
                    Log.Write("ntfy: no se pudo enviar: " + e.Message);
                    if (done != null) done(false);
                }
            });
        }
    }
}
