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
