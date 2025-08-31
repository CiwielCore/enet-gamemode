using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading;
using eNetwork.Framework;

namespace eNetwork.External
{
    public class Mailer
    {
        private static readonly Logger _logger = new Logger("mailer");
        private static List<Message> _messages = new List<Message>();

        private static readonly string _mail = "wildproject_gta5@outlook.com";
        private static readonly string _password = "A9j27x6DVqc3f";
        private static readonly string _host = "smtp.office365.com";
        private static readonly int _port = 587;

        public static void Send(string email, string subject, string body)
        {
            try
            {
                var msg = new Message(email, subject, body);
                _messages.Add(msg);
            }
            catch(Exception ex) { _logger.WriteError("Send", ex); }
        }

        public static void Initialize()
        {
            try
            {
                var thread = new Thread(new ParameterizedThreadStart(Worker))
                {
                    IsBackground = true,
                };

                thread.Start();
            }
            catch (Exception ex) { _logger.WriteError("Initialize", ex); }
        }

        public static void Worker(object obj)
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    foreach (var msg in _messages)
                    {
                        if (!msg.IsSended)
                        {
                            var sender = new MailAddress(_mail);
                            var recipient = new MailAddress(msg.Mail);

                            var message = new MailMessage(sender, recipient)
                            {
                                Subject = msg.Subject,
                                Body = msg.Body,
                                IsBodyHtml = true
                            };

                            var smtpClient = new SmtpClient
                            {
                                Host = _host,
                                Port = _port,
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(sender.Address, _password)
                            };

                            try
                            {
                                smtpClient.Send(message);
                                msg.IsSended = true;
                                _logger.WriteDone($"Сообщение с успешно отправлено на {msg.Mail}!");
                            }
                            catch (Exception ex) { _logger.WriteError("Отправка почтового сообщения", ex); msg.IsSended = true; }
                        }
                    }
                }
            }
            catch (Exception ex) { _logger.WriteError("Worker", ex); }
        }

        public class Message
        {
            public string Mail { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public bool IsSended { get; set; } = false;
            public Message(string mail, string sub, string body)
            {
                Mail = mail; 
                Subject = sub; 
                Body = body;
                IsSended = false;
            }
        }
    }
}
